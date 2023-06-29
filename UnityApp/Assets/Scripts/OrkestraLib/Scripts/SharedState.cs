using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using OrkestraLib.Network;
using OrkestraLib.Message;
using static OrkestraLib.Message.IPacket;
using static OrkestraLib.Utilities.ParserExtensions;
using static OrkestraLib.Utilities.StringExtensions;
using System.Collections.Concurrent;

namespace OrkestraLib
{
    /// <summary>
    /// The Shared State maintains distributed agreement concerning dynamic, 
    /// online application resources in multi-device applications. Application 
    /// resources may for instance include application data, context, data, or 
    /// timing resources. Agreement about current state is maintained continuously, 
    /// even if the resources are dynamically changing.
    /// 
    /// Shared State is based on two parts; 1) an online service and 2) a local 
    /// proxy for that service. The proxy provides a local representation of the 
    /// current state of the remote service, and programmers may always depend 
    /// on this representation being updated as soon as possible.
    /// 
    /// Shared state solutions defines a high-level programming model, where distributed 
    /// agents/clients interact with the application and each other - not directly - 
    /// but indirectly through a common data model. This includes querying and updating 
    /// shared state, as well as reacting to events as shared state changes. 
    /// This programming model hides complexities regarding connection management, 
    /// message-passing, discovery and pairing. Furthermore, the shared state approach 
    /// encourages clients to communicate directly with servers (reliable) instead of 
    /// clients (unreliable).
    /// 
    /// Documentation on GitHub: https://github.com/mediascape/shared-state/tree/master/API
    /// </summary>
    public class SharedState : ISerializable
    {
        private readonly ConcurrentDictionary<string, List<Action<string>>> Events;
        private readonly ConcurrentDictionary<string, KeyTypeValue> StateChanges;
        private readonly ConcurrentDictionary<string, string> SharedStates;
        private readonly ConcurrentDictionary<string, object> PresenceMap;
        private readonly ConcurrentDictionary<string, string> State;
        private StateType ConnectionState = StateType.CONNECTING;
        private readonly IOrkestra Orkestra;

        private bool IsRequestingFlag; // TODO: check if this is threadsafe
        private bool IsConnected;

        public IWebSocketAdapter Connection { get; private set; }

        public string AgentID { get; private set; }

        public string Presence
        {
            get; private set;
        }

        public SharedState(IOrkestra orkestra, string agentID)
        {
            IsRequestingFlag = false;
            IsConnected = false;
            Orkestra = orkestra;

            AgentID = agentID;
            Presence = "";

            State = new ConcurrentDictionary<string, string>();
            State.TryAdd("CONNECTING", StateType.CONNECTING.ToJSON());
            State.TryAdd("OPEN", StateType.OPEN.ToJSON());
            State.TryAdd("CLOSED", StateType.CLOSED.ToJSON());

            Events = new ConcurrentDictionary<string, List<Action<string>>>();
            Events.TryAdd("change", new List<Action<string>>());
            Events.TryAdd("remove", new List<Action<string>>());
            Events.TryAdd("readystatechange", new List<Action<string>>());
            Events.TryAdd("presence", new List<Action<string>>());

            PresenceMap = new ConcurrentDictionary<string, object>();
            SharedStates = new ConcurrentDictionary<string, string>();
            StateChanges = new ConcurrentDictionary<string, KeyTypeValue>();
        }

        public void Connect(string url, Action onConnected, Action<bool, string> onConnectionInfo)
        {
            UpdateConnectionState(StateType.CONNECTING);
            //Debug.Log("Shared State: Connect: " + url);
            Orkestra.OpenSocket("SharedState", url,
                (conn) =>
                {
                    // Here we are not connected
                    Connection = conn;
                    Connection.On("disconnect", OnDisconnect);
                    Connection.On("joined", OnJoined);
                    Connection.On("status", OnStatus);
                    Connection.On("changeState", OnChangeState);
                    Connection.On("initState", OnInitState);
                    Connection.On("ssError", (string data) => { onConnectionInfo?.Invoke(true, data); });
                },
                (connected) =>
                {
                    IsConnected = connected;
                    onConnected();
                    if (connected)
                    {
                        //Debug.Log("Shared State: Connected " + url);
                        UpdateConnectionState(StateType.CONNECTING);
                        SendDatagram("join", new AgentId(ArgsType.Value, AgentID));
                    }
                }
            );
        }

        void OnDisconnect(string data)
        {
            IsConnected = false;
            UpdateConnectionState(StateType.DISCONNECTING);
        }

        void OnJoined(string data)
        {
            try
            {
                AgentId[] datagram = data.FromJsonList<AgentId>();
                //Debug.Log("OnJoined " + data);
                foreach (AgentId agent in datagram)
                {
                    if (agent.agentID.Equals(AgentID))
                    {
                        int[] array = { };
                        Connection.Emit("getInitState", JsonConvert.SerializeObject(array)); // TODO: {} ?
                    }
                    else
                    {
                        UpdateConnectionState(StateType.OPEN);
                        SetPresence(PresenceType.Online);
                    }
                }
                CleanLoop();
            }
            catch (Exception ex)
            {
                Debug.LogError(data + " E: " + ex);
            }
        }

        void OnStatus(string data)
        {
            try
            {
                Status datagram = new Status(data);
                KeyValue[] presenceList = datagram.presence;
                for (int i = 0; i < presenceList.Length; i++)
                {
                    KeyValue p = presenceList[i];
                    if (p != null && !string.IsNullOrEmpty(p.key))
                    {
                        if (p.value.Equals(StateType.CONNECTED.ToJSON()))
                            p.value = PresenceType.Online.ToJSON();
                        PresenceMap[p.key] = p.value;
                        InvokeCallbacks("presence", p.ToJSON());
                    }
                    /*else
                    {
                        Debug.LogError("reveived 'presence' already saved or something wrong" + presenceList[i]);
                    }*/
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        void OnChangeState(string data)
        {
            //Debug.Log("SharedState::OnChangeState -> data = " + data);
            try
            {
                // The 'true' will extract the json out of the first array element
                // For some reason data is inside an array
                KeyTypeValue[] datagram = data.FromJsonList<KeyTypeValue>();
                foreach (KeyTypeValue kvt in datagram)
                {
                    //Debug.Log("SharedState::OnChangeState -> kvt = " + kvt.ToJSON());
                    if (kvt.type.Equals("set"))
                    {
                        kvt.type = !SharedStates.ContainsKey(kvt.key) ? "add" : "update";
                        SharedStates[kvt.key] = kvt.value;
                        InvokeCallbacks("change", kvt.ToJSON());
                    }
                    else if (kvt.type.Equals("remove"))
                    {
                        if (SharedStates.ContainsKey(kvt.key))
                        {
                            KeyTypeValue state = new KeyTypeValue(
                                kvt.key,
                                "delete",
                                SharedStates[kvt.key]
                            );
                            SharedStates.TryRemove(kvt.key, out string value);
                            //Debug.Log("Key removes: " + kvt.key);
                            InvokeCallbacks("remove", state.ToJSON());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex + data);
            }
        }

        void OnInitState(string data)
        {
            try
            {
                //Debug.Log("INIT STATE -START");
                // The 'true' will extract the json out of the first array element
                // For some reason data is inside an array
                KeyTypeValue[] datagram = data.FromJsonList<KeyTypeValue>();
                foreach (var state in datagram)
                {
                    if (state.type == "set")
                    {
                        if (SharedStates.ContainsKey(state.key))
                        {
                            if (!SharedStates[state.key].Equals(state.value))
                            {
                                SharedStates[state.key] = state.value;
                                var st = new KeyTypeValue(state.key, "add", state.value);
                                //if (state.value.Length > 0) //revisar
                                //Debug.Log("INIT STATE SET- ")
                                InvokeCallbacks("change", st.ToJSON());
                            }
                        }
                        else
                        {
                            SharedStates.TryAdd(state.key, state.value);
                        }
                    }
                }
                UpdateConnectionState(StateType.OPEN);
                Presence = PresenceType.Online.ToJSON();
                SetPresence(PresenceType.Online);
                //Debug.Log("INIT STATE - END");
            }
            catch (Exception ex)
            {
                Debug.LogError(data + " " + ex);
            }
        }

        public void SetItem(string key, string value)
        {
            if (IsRequestingFlag)
            {
                StateChanges[key] = new KeyTypeValue(key, "set", value);
            }
            else
            {
                StateType state = UpdateConnectionState(StateType.UNDEFINED);
                if (state == StateType.OPEN)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        string json = "[" + new KeyTypeValue(key, "set", value).ToJSON() + "]";
                        SendDatagram("changeState", json);
                    }
                    /*else
                    {
                        Debug.LogError("SHAREDSTATE - params with error - key:" + key + "value:" + value);
                    }*/
                }
                /*else
                {
                    Debug.LogError("setItem not possible - connection status:" + state.ToJSON());
                }*/
            }
        }

        public void RemoveItem(string key)
        {
            var ktv = new KeyTypeValue(key, "remove");
            SendDatagram("changeState", "[" + ktv.ToJSON() + "]");
        }

        public string GetItem(string key)
        {
            if (string.IsNullOrEmpty(key)) // HERE HERE
            {
                int[] datagram = { };
                SendDatagram("getState", JsonConvert.SerializeObject(datagram));
            }
            else
            {
                if (SharedStates.ContainsKey(key))
                {
                    // @TODO: serialize of a string?
                    return JsonConvert.SerializeObject(SharedStates[key]);
                }
            }
            return null;
        }

        public List<string> Keys()
        {
            return new List<string>(SharedStates.Keys);
        }

        public void Request()
        {
            IsRequestingFlag = true;
        }

        public void Send()
        {
            if (UpdateConnectionState(StateType.UNDEFINED) == StateType.OPEN)
            {
                IsRequestingFlag = false;
                if (StateChanges.Keys.Count > 0)
                {
                    JArray datagram = new JArray();
                    foreach (string key in StateChanges.Keys)
                    {
                        datagram.Add(StateChanges[key]);
                    }
                    SendDatagram("changeState", JsonConvert.SerializeObject(datagram));
                    StateChanges.Clear();
                }
            }
            /*else
            {
                Debug.LogWarning("SHAREDSTATE - send not possible - connection status:" + UpdateConnectionState(StateType.UNDEFINED));
            }*/
        }

        // TODO: this following case should be replaced with GetConnectionState()
        // UpdateConnectionState(StateType.UNDEFINED)
        StateType UpdateConnectionState(StateType newState)
        {
            if (newState != StateType.UNDEFINED)
            {
                //Debug.Log("UpdateConnectionState(" + newState.ToJSON() + ")");
                bool found = State.ContainsKey(newState.ToJSON().ToUpper());
                if (!found && ConnectionState == StateType.CLOSED) return StateType.UNDEFINED;
                if (!newState.Equals(ConnectionState))
                {
                    ConnectionState = newState;
                    InvokeCallbacks("readystatechange", newState.ToJSON());
                }
            }
            else return ConnectionState;
            return StateType.UNDEFINED;
        }

        public void SendDatagram(string type, IPacket datagram)
        {
            Connection.Emit(type, datagram);
        }

        public void SendDatagram(string type, string datagram)
        {
            Connection.Emit(type, datagram);
        }

        void InvokeCallbacks(string what, string e)
        {
            if (!Events.ContainsKey(what)) Debug.LogWarning("Unsupported event " + what);
            else
            {
                //Debug.Log("INVOKE " + what + " " + e + JsonConvert.SerializeObject(Events.Keys));
                foreach (Action<string> action in Events[what])
                {
                    try
                    {
                        action.Invoke(e);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error in " + what + ": " + ex);
                    }
                }
            }
        }

        private void DoAutoClean(string agentid)
        {
            // Go through the dataset and remove anything left from this node
            foreach (var state in SharedStates)
            {
                if (SharedStates.ContainsKey(state.Key))
                {
                    if (state.Key.IndexOf("__") == 0 && state.Key.IndexOf("__" + agentid) > -1)
                    {
                        //  removeItem(key.Key);
                    }
                }
            }
        }

        public void SetPresence(PresenceType state)
        {
            if (state.Equals(PresenceType.Online))
            {
                Connection.Emit("changePresence", new Presence(AgentID, PresenceType.Online));
            }
        }

        private void CleanLoop()
        {
            foreach (var key in SharedStates)
            {
                if (SharedStates.ContainsKey(key.Key))
                {
                    if (key.Key.IndexOf(Constants.META_VARIABLE) == 0)
                    {
                        string agentid = key.Key.Substring(8);
                        if (PresenceMap.ContainsKey(agentid))
                        {
                            DoAutoClean(agentid);
                        }
                    }
                }
            }
        }

        /** listen for event **/
        public void On(string what, Action<string> handler)
        {
            try
            {

                if (!Events.ContainsKey(what)) Debug.LogWarning("Unsupported event " + what);
                else if (Events[what].IndexOf(handler) == -1)
                {
                    Events[what].Add(handler);
                    switch (what)
                    {
                        case "change":
                            if (IsConnected)
                            {
                                List<string> sent = new List<string>();
                                foreach (string key in SharedStates.Keys)
                                {
                                    var kv = new KeyTypeValue(key, "update", SharedStates[key]);
                                    if (!sent.Contains(kv.ToJSON()))
                                    {
                                        sent.Add(kv.ToJSON());
                                        InvokeCallbacks("change", kv.ToJSON());
                                    }
                                }
                            }
                            break;
                        case "presence":
                            List<KeyValue> sentMsg = new List<KeyValue>();
                            foreach (string key in PresenceMap.Keys)
                            {
                                var msg = new KeyValue(key, "" + PresenceMap[key]);
                                if (!sentMsg.Contains(msg))
                                {
                                    sentMsg.Add(msg);
                                    InvokeCallbacks("presence", msg.ToJSON());
                                }
                            }
                            break;
                        case "remove":
                            // handler._immediate_pending = false;
                            break;
                        case "readystatechange":
                            InvokeCallbacks("readystatechange",
                                    UpdateConnectionState(StateType.UNDEFINED).ToJSON());
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public void Off(string what, Action<string> handler)
        {
            if (Events.ContainsKey(what) && Events[what].Count > 0)
            {
                List<Action<string>> list = Events[what];
                var index = list.IndexOf(handler);
                if (index > -1)
                {
                    Events[what] = list.Splice(index, 1);
                }
            }
        }

        public void Disconnect()
        {
            Connection.Disconnect();
        }

        public string GetStateValue(string state)
        {
            return State[state];
        }

        public Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "AgentID", AgentID },
                { "Conn state", ConnectionState.ToJSON() },
                { "Presence map", JsonConvert.SerializeObject(PresenceMap) },
                { "State changes", JsonConvert.SerializeObject(StateChanges) },
                { "Request flag", IsRequestingFlag ? "true":"false" },
                { "Is connected", IsConnected ? "true":"false" },
                { "Presence", Presence },
            };
        }
    }
}