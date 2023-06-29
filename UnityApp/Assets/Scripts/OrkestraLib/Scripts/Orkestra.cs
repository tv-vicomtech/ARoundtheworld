using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

using OrkestraLib.Network;
using OrkestraLib.Message;
using OrkestraLib.Utilities;
using static OrkestraLib.Message.IPacket;
using static OrkestraLib.Utilities.ParserExtensions;
using System.Collections.Concurrent;

namespace OrkestraLib
{
    /// <summary>
    /// Main interface for the OrkestraLib
    /// </summary>
    public class Orkestra : MonoBehaviour, IOrkestra
    {
        public enum Channel
        {
            Application,
            User
        };

        /// <summary>
        /// User-defined function to open a websocket with arbitrary libraries.
        /// This function is implemented by the application main controller
        /// </summary>
        public Action<string, string, Action<IWebSocketAdapter>, Action<bool>> OpenSocket { get; set; }

        /// <summary>
        /// User-defined function to close a websocket.
        /// This function is implemented by the application main controller
        /// </summary>
        public Action<IWebSocketAdapter> CloseSocket { get; set; }

        public MappingService MapService { get; private set; }

        public string room;
        public string url;
        public string agentId;
        public List<string> eventKeys;

        public ApplicationContext AppContext { get; private set; }

        public event EventHandler<UserEvent> UserEvents;
        public event EventHandler<ApplicationEvent> ApplicationEvents;

        public ConcurrentDictionary<string, User> Users { get; private set; }

        public ConcurrentDictionary<string, string> ApplicationData { get; private set; }

        public ConcurrentDictionary<string, List<string>> EnabledCapabilities { get; private set; }

        // If true when the client disconnect from the server, all the keys from the server will be remove
        public bool ResetRoomAtDisconnect = false;

        public static string[] PersistentKeys = { "" };
        public readonly List<Action> Events = new List<Action>();

        public Orkestra()
        {
            eventKeys = new List<string>
            {
                "data"
            };
            Users = new ConcurrentDictionary<string, User>();
            ApplicationData = new ConcurrentDictionary<string, string>();
            EnabledCapabilities = new ConcurrentDictionary<string, List<string>>();
            MapService = new MappingService(this);
        }

        public virtual void LateUpdate()
        {
            // Execute next action in main thread while avoiding massive updates
            int maxExecsPerFrame = 100;
            while (Events.Count > 0 && maxExecsPerFrame > 0)
            {
                try
                {
                    Events[0]?.Invoke();
                }
                catch (Exception) { }
                Events.RemoveAt(0);
                maxExecsPerFrame--;
            }
        }

        public void RegisterEvent(string key)
        {
            if (!eventKeys.Contains(key)) eventKeys.Add(key);
        }

        public void RegisterEvents(Type[] events)
        {
            foreach (Type type in events)
            {
                RegisterEvent(type.Name);
            }
        }

        private void OnMapConnect(bool connected, Action onConnectionReady = null, Action<bool, string> onConnectionInfo = null)
        {
            if (connected)
            {
                MapService.GetGroupMapping(room, (e) =>
                {
                    // Create a new connection with a namespace
                    GroupUrl g = new GroupUrl(ArgsType.JSON, e);
                    AppContext = new ApplicationContext(this, agentId);
                    AppContext.On("agentchange", OnAgentChange);
                    AppContext.Connect(g.group, () =>
                    {
                        List<string> keys = AppContext.GetKeys();
                        try
                        {
                            foreach (var s in keys)
                            {
                                AppContext.On(s, OnAppAttributeChange);
                            }

                            foreach (string eventKey in eventKeys) Subscribe(eventKey);

                            foreach (string key in keys)
                            {
                                ApplicationData[key] = AppContext.GetItem(key);
                                AppEventsCall(new ApplicationEvent(key, ApplicationData[key]));
                            }
                            onConnectionReady?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("error" + ex);
                        }
                    }, onConnectionInfo);
                });
            }
        }

        /**
         * onConnectionInfo(isError, message)
         */
        public void Connect(Action onConnectionReady = null, Action<bool, string> onConnectionInfo = null)
        {
            if (string.IsNullOrEmpty(agentId)) agentId = StringExtensions.RandomString(8);
            MapService.Connect(url, (connected) =>
            {
                OnMapConnect(connected, onConnectionReady, onConnectionInfo);
            });
        }

        public string GetURL() { return url; }

        public GameObject GetGameObject() { return gameObject; }

        protected virtual void UserEventsCall(UserEvent evt)
        {
            UserEvents?.Invoke(this, evt);
        }

        protected virtual void AppEventsCall(ApplicationEvent e)
        {
            ApplicationEvents?.Invoke(this, e);
        }

        private void OnAppAttributeChange(string data)
        {
            try
            {
                ApplicationEvent evt = new ApplicationEvent(data);
                JObject j = JObject.Parse(data);
                evt.value = j.SelectToken("value").ToString();

                // Debug.Log("OnApp: " + data);
                if (string.Empty.Equals(evt.value))//@TODO: Here arrives rare events
                {
                    evt.value = InitializationMessage;
                    Debug.LogError("Empty Event value" + data);
                }
                ApplicationData[evt.key] = evt.value;

                //if(evt.value.Length>0)
                AppEventsCall(evt);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex + data);
            }
        }

        //Send a petition to remove all keys from the server
        public bool RemoveAllKeys()
        {
            return AppContext.RemoveKeys();
        }

        public void RemoveKey(string key)
        {
            AppContext.RemoveKey(key);
        }

        void OnAgentChange(string data)
        {
            AgentContextId chg = new AgentContextId(data);
            if (!chg.agentContext.IsSystemInitMessage())
            {
                string id = chg.agentid;
                string ctx = chg.agentContext;

                if (!Users.TryGetValue(id, out User user))
                {
                    try
                    {
                        user = new User(id)
                        {
                            Context = ctx
                        };
                        Users.TryAdd(id, user);

                        // Probably the same as user.AgentId;
                        UserEventsCall(new UserEvent(id, UserEventType.Joined));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(data + " E:" + ex);
                    }
                }

                List<string> keys;
                if (chg.agentid.Equals(agentId)) keys = AppContext.SelfAgent.Keys();
                else
                {
                    RemoteAgent ra = AppContext.GetAgent(id);
                    if (ra == null)
                    {
                        Debug.LogError("Agent with id '" + id + "' does not exist!");
                        keys = new List<string>();
                    }
                    else keys = ra.Api.Keys();
                }

                if (keys.Count > 0)
                {
                    if (EnabledCapabilities.ContainsKey(id))
                    {
                        if (!(EnabledCapabilities[id].IndexOf("data") == -1))
                            EnableInstrument(keys, ctx);
                    }
                    else
                    {
                        EnableInstrument(keys, ctx);
                        EnabledCapabilities[id] = keys;
                    }
                }
            }
            else
            {
                UserEvent evt = new UserEvent(chg.agentid, UserEventType.Left);
                Users.TryRemove(evt.agentid, out User removed);
                UserEventsCall(evt);
            }
        }

        public void Subscribe(string key)
        {
            AppContext.On(key, OnAppAttributeChange);
        }

        void EnableInstrument(List<string> caps, string agentid)
        {
            //Debug.Log("EnableInstrument " + agentid + " " + JsonConvert.SerializeObject(caps));
            try
            {
                foreach (string cap in caps)
                {
                    RemoteAPI context = AppContext.GetAgent(agentid).Api;
                    if (context.AgentID == AppContext.SelfAgent.AgentID)
                    {
                        context.On(cap, (data) =>
                        {
                            KeyValue kv = new KeyValue();
                            try
                            {
                                string fixJSON = data.FixJSON();
                                JsonUtility.FromJsonOverwrite(fixJSON, kv);
                            }
                            catch (Exception)
                            {
                                // Case where data = "init1"
                                string fix = data.Replace("\\\"", "").Replace("\"", "");
                                kv = new KeyValue(cap, fix);
                                //Debug.LogError("ENABLE INSTRUMENTS: " + kv.key + " " + kv.value);
                            }

                            if (Users.ContainsKey(agentId))
                            {
                                User user = Users[agentId];
                                user.Capacity[kv.key] = kv.StringVal();
                            }

                            UserEventsCall(new UserEvent(agentid, UserEventType.AgentEvent, kv));
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public void SetGlobalItem(string key, string value)
        {
            if (value.Length <= 0)
                AppContext?.SetItem(key, InitializationMessage); //Revisar
            else
                AppContext?.SetItem(key, value);
        }

        public string GetGlobalItem(string key, string value)
        {
            return AppContext.GetItem("text");
        }

        public void DispatchUnset(Channel channel, Type var, string toUserId = "")
        {
            if (channel == Channel.Application)
            {
                AppContext.SetItem(var.Name, InitializationMessage);
            }
            else if (toUserId.Length > 0)
            {
                if (agentId.Equals(toUserId))
                {
                    AppContext.SelfAgent.SetItem(var.Name, InitializationMessage);
                }
                else
                {
                    if (AppContext.GetAgent(toUserId) == null) throw new ArgumentException("The user does not exist");
                    AppContext.GetAgent(toUserId).Api.SetItem(var.Name, InitializationMessage);
                }
            }
            else
            {
                throw new ArgumentException("Messages to users require the toUserId param to not be empty UNSET");
            }
        }

        public void Dispatch(Channel channel, Message.Message message, string toUserId = "")
        {
            string data = message.ToJSON();
            if (channel == Channel.Application)
            {
                AppContext?.SetItem(message.type, data);
            }
            else if (toUserId.Length > 0)
            {
                if (agentId.Equals(toUserId))
                {
                    AppContext.SelfAgent.SetItem(message.type, data);
                }
                else
                {
                    RemoteAgent agent = AppContext.GetAgent(toUserId);
                    if (agent == null) throw new ArgumentException("The user does not exist");
                    agent.Api.SetItem(message.type, data);
                }
            }
            else
            {
                throw new ArgumentException("Messages to users require the toUserId param to not be empty");
            }
        }

        public string GetUserItem(string agentid, string key)
        {
            // TODO: bug nullptr
            return AppContext.GetAgent(agentid).Api.GetItem(key);
        }

        public async void Disconnect()
        {
            bool espera = true;
            if (ResetRoomAtDisconnect)
            {
                // TODO: orkestra is a Monobehaviour, better use Coroutine for consistency
                while (espera)
                {
                    espera = !await Task.Run(RemoveAllKeys);
                }

            }
            AppContext?.Disconnect();
        }

        public void OnDestroy()
        {
            Disconnect();
        }
    }
}
