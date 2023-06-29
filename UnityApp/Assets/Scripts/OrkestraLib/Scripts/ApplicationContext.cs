using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using UnityEngine;
using OrkestraLib.Message;
using static OrkestraLib.Utilities.ParserExtensions;
using System.Collections.Concurrent;

namespace OrkestraLib
{
    /// <summary>
    /// The application context represents an execution of a multi-device application, 
    /// where all the participating agents contribute. It can be shared between multiple 
    /// devices of a single user or multiple users, whatever is suitable for the application. 
    /// It is used to share data between all the agents connected to the application.
    /// Documentation on GitHub: https://github.com/mediascape/application-context/tree/master/API#application-context
    /// </summary>
    public class ApplicationContext : ISerializable
    {
        public Dictionary<string, RemoteAgent> RemoteAgents { get; private set; }
        public AgentContext SelfAgent;

        private readonly Dictionary<string, List<Action<string>>> GlobalEventActions;

        // @BUG: Not TF
        private readonly List<Action<string>> EventActions;
        private readonly ConcurrentDictionary<string, JObject> CurrentAgentState;
        private readonly ConcurrentDictionary<string, string> LastCapabilities;
        private readonly List<string> ApplicationEventKeys;
        private readonly SharedState ApplicationSharedState;

        // Remote subscriptions, parameter -> [agentid, ...]
        private readonly Dictionary<string, List<string>> AgentSubscriptionsByParam;

        // Remote agent -> [parameter, ...]
        private readonly Dictionary<string, List<string>> AgentSubscriptionsByAgent;
        private readonly Dictionary<string, Action<string>> AgentSubHandlersByParam;

        public ApplicationContext(Orkestra orkestra, string agentid)
        {
            ApplicationEventKeys = orkestra.eventKeys;
            EventActions = new List<Action<string>>();
            GlobalEventActions = new Dictionary<string, List<Action<string>>>();
            CurrentAgentState = new ConcurrentDictionary<string, JObject>();
            LastCapabilities = new ConcurrentDictionary<string, string>();
            RemoteAgents = new Dictionary<string, RemoteAgent>();
            ApplicationSharedState = new SharedState(orkestra, agentid);

            AgentSubscriptionsByParam = new Dictionary<string, List<string>>();
            AgentSubscriptionsByAgent = new Dictionary<string, List<string>>();
            AgentSubHandlersByParam = new Dictionary<string, Action<string>>();

#if UNITY_EDITOR && ORKESTRALIB
            UnityDataViewer.Register("ApplicationContext", this);
#endif
        }

        public void Connect(string url, Action onConnected, Action<bool, string> onConnectionInfo)
        {
            try
            {
                ApplicationSharedState.On("readystatechange", (string data) =>
                {
                    if (data.Equals(ApplicationSharedState.GetStateValue("OPEN")))
                    {
                        SelfAgent = new AgentContext(ApplicationSharedState.AgentID, ApplicationEventKeys);
                        string meta = "{\"keys\":" + JsonConvert.SerializeObject(SelfAgent.Keys()) +
                            ", \"capabilities\":" + JsonConvert.SerializeObject(SelfAgent.Capabilities) + "}";

                        ApplicationSharedState.SetItem(Constants.GetMetaVariable(ApplicationSharedState.AgentID), meta);
                        ApplicationSharedState.SetItem(Constants.GetMetaSubVariable(ApplicationSharedState.AgentID), JsonConvert.SerializeObject(new JArray()));
                        ApplicationSharedState.SetPresence(PresenceType.Online);

                        // Register
                        // Also check for new parameters being added
                        //  moved here to only test if _sharedstate == open
                        SelfAgent.On("keychange", (e) =>
                            {
                                // Update my meta description too
                                ApplicationSharedState.SetItem(Constants.GetMetaVariable(ApplicationSharedState.AgentID),
                                        JsonConvert.SerializeObject(new
                                        {
                                            keys = SelfAgent.Keys(),
                                            capabilities = SelfAgent.Capabilities
                                        }));
                            });

                        ApplicationSharedState.On("presence", (e) =>
                        {
                            //Debug.Log("On presence " + e);
                            KeyValue agent = new KeyValue(e);
                            if (agent.value.Equals(PresenceType.Offline.ToJSON()))
                            {
                                if (RemoteAgents.ContainsKey(agent.key))
                                {
                                    // Clean up this node
                                    RemoteAgents[agent.key] = null;
                                    RemoteAgents.Remove(agent.key);
                                    AgentContextId actx = new AgentContextId(agent.key, "null");
                                    InvokeCallbacks("agentchange", actx.ToJSON());
                                }
                            }
                            else
                            {
                                if (agent.value.Equals(PresenceType.Online.ToJSON()))
                                {
                                    if (!RemoteAgents.ContainsKey(agent.key))
                                    {
                                        RemoteAgents[agent.key] = RemoteAgentContext(agent.key);
                                    }

                                    RemoteAgent agt = RemoteAgents[agent.key];
                                    if (agt != null)
                                    {
                                        var caps = agt.Api.Keys();
                                        UpdateSubscriptions(agent.key, caps);

                                        var acn = new AgentChangeNotification(agent.key, agent.key);
                                        InvokeCallbacks("agentchange", acn.ToJSON());
                                    }
                                }
                            }
                        });

                        ApplicationSharedState.On("change", (ev) =>
                        {
                            try
                            {
                                KeyTypeValue ktv = new KeyTypeValue(ev);
                                if (ktv.IsMetaKey())
                                {
                                    string ktvAgentId = ktv.GetMetaKey();
                                    RemoteAgent rmAgnt;
                                    if (RemoteAgents.TryGetValue(ktvAgentId, out rmAgnt) && rmAgnt != null)
                                    {
                                        rmAgnt.UpdateMeta(ktv.value);
                                        AgentContextId actx = new AgentContextId(ktvAgentId, ktvAgentId);
                                        Task.Delay(1).ContinueWith(t2 =>
                                        {
                                            InvokeCallbacks("agentchange", actx.ToJSON());
                                        });
                                    }
                                }
                                else if (ktv.IsMetaSubKey())
                                {
                                    var ktvAgentId = ktv.GetMetaSubKey();
                                    try
                                    {
                                        JArray tnp = JArray.Parse(ktv.value);
                                        List<string> list = tnp.ToObject<List<string>>();
                                        UpdateSubscriptions(ktvAgentId, list);
                                    }
                                    catch (Exception)
                                    {
                                        Debug.LogError("Expected array but got: " + ktv.value);
                                    }
                                }
                                else if (ktv.IsVarKey())
                                {
                                    string ktvAgentId = ktv.key.Substring(7).Split('_')[0];
                                    string ktvKey = ktv.key.Substring(7).Split('_')[1];
                                    UpdateValue(ktvAgentId, ktvKey, ktv.value);
                                }
                                else if (ktv.IsGlobalKey())
                                {
                                    string keyName = ktv.GetGlobalKey();
                                    if (GlobalEventActions.ContainsKey(keyName))
                                    {
                                        KeyValue kv = new KeyValue(keyName, ktv.value);
                                        InvokeCallbacks(keyName, kv.ToJSON());
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.Log("Exception" + ex);
                            }
                        });
                    }
                });
                ApplicationSharedState.Connect(url, onConnected, onConnectionInfo);
            }
            catch (Exception e){
                Debug.LogError("CONNECT ON APPLICATION CONTEXT FAILED" + e);
            }
        }

        public void Disconnect()
        {
            ApplicationSharedState.Disconnect();
        }

        // BUG: returning null breaks the code
        public RemoteAgent GetAgent(string aid)
        {
            if (RemoteAgents.ContainsKey(aid)) return RemoteAgents[aid];
            else return null;
        }

        public List<string> GetRemoveUserIds()
        {
            if (RemoteAgents == null) return new List<string>();
            return new List<string>(RemoteAgents.Keys);
        }

        public RemoteAgent GetAgentContext(string aid)
        {
            if (RemoteAgents.ContainsKey(aid)) return RemoteAgentContext(aid);
            else return null;
        }

        public string GetSelfAgent()
        {
            return ApplicationSharedState.AgentID;
        }

        public Dictionary<string, RemoteAgent> GetAgents()
        {
            if (!RemoteAgents.ContainsKey(ApplicationSharedState.AgentID))
            {
                RemoteAgents[ApplicationSharedState.AgentID] = RemoteAgentContext(ApplicationSharedState.AgentID);
            }

            Dictionary<string, RemoteAgent> agents = new Dictionary<string, RemoteAgent>();
            agents.Add("self", agents[ApplicationSharedState.AgentID]);

            foreach (KeyValuePair<string, RemoteAgent> entry in agents)
            {
                if (entry.Value.AgentID.Equals(ApplicationSharedState.AgentID)) continue;
                if (agents.ContainsKey(ApplicationSharedState.AgentID))
                {
                    agents[ApplicationSharedState.AgentID] = agents[ApplicationSharedState.AgentID];
                }
            }
            return agents;
        }

        public void AddAgent(RemoteAgent agent)
        {
            RemoteAgents[agent.AgentID] = agent;
            InvokeCallbacks("agentchange", JsonConvert.SerializeObject(new
            {
                agentid = agent.AgentID,
                agentContext = agent,
                diff = new JObject
                {
                    ["added"] = agent.Api.Capabilities()
                }
            }));
        }

        public void RemoveAgent(RemoteAgent agent)
        {
            RemoteAgents.Remove(agent.AgentID);
            AgentContextId actx = new AgentContextId(agent.AgentID, "null");
            InvokeCallbacks("agentchange", actx.ToJSON());
        }

        public void SetItem(string key, string value)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                ApplicationSharedState.SetItem(Constants.GetGlobalVariable(key), value);
            }
        }

        /// <summary>Remove all the keys stored on the shared state</summary>
        public bool RemoveKeys()
        {
            ApplicationSharedState.Keys().ForEach(key =>
            {
                ApplicationSharedState.RemoveItem(key);
            });
            return ApplicationSharedState.Keys().Count == 0;
        }

        /// <summary>Remove a specific key</summary>
        public bool RemoveKey(string key)
        {
            if (ApplicationSharedState.Keys().Contains(key))
            {
                ApplicationSharedState.RemoveItem(key);
                return true;
            }             
            return false;
        }

        public List<string> GetKeys()
        {
            List<string> allKeys = ApplicationSharedState.Keys();
            List<string> globalVars = new List<string>();
            for (int i = 0, len = allKeys.Count; i < len; i++)
            {
                if (allKeys[i].IndexOf(Constants.GLOBAL_VARIABLE) > -1)
                {
                    globalVars.Add(allKeys[i].Substring(10));
                }
            }
            return globalVars;
        }

        public string GetItem(string key)
        {
            return ApplicationSharedState.GetItem(Constants.GetGlobalVariable(key));
        }

        void UpdateSubscriptions(string subAgentId, List<string> subsList)
        {
            if (subsList == null) subsList = new List<string>();

            try
            {
                List<string> theSubs;
                if (!AgentSubscriptionsByAgent.ContainsKey(subAgentId))
                {
                    theSubs = new List<string>();
                }
                else theSubs = AgentSubscriptionsByAgent[subAgentId];

                for (int i = 0; i < subsList.Count; i++)
                {
                    var targetAgent = subAgentId;
                    var targetParam = subsList[i];
                    if (subsList[i].Contains("_"))
                        targetParam = subsList[i].Substring(subsList[i].IndexOf("_") + 1);

                    if (targetAgent.Equals(ApplicationSharedState.AgentID))
                    {
                        // See if this agent already subscribes to my parameter, otherwise add it
                        if (!AgentSubscriptionsByParam.ContainsKey(targetParam))
                        {
                            AgentSubscriptionsByParam[targetParam] = new List<string>();
                        }
                        else
                        {
                            if (!AgentSubscriptionsByParam[targetParam].Contains(subAgentId))
                                AgentSubscriptionsByParam[targetParam].Add(subAgentId);
                        }
                        if (!AgentSubHandlersByParam.ContainsKey(targetParam))
                        {
                            AgentSubHandlersByParam[targetParam] = (e) =>
                            {
                                JObject d = JObject.Parse(e);
                                ApplicationSharedState.SetItem(Constants.GetVariable(
                                    ApplicationSharedState.AgentID + "_" + d["key"].ToString()),
                                        d["value"].ToString());
                            };
                        }
                        SelfAgent.On(targetParam, AgentSubHandlersByParam[targetParam]);
                    }
                }

                theSubs.AddRange(subsList);
                AgentSubscriptionsByAgent[subAgentId] = theSubs;
            }
            catch (Exception ex)
            {
                Debug.LogError("Updated subscription" + ex);
            }
        }

        void UpdateValue(string agentId, string key, string val)
        {
            GetAgent(agentId)?.UpdateValue(agentId, key, val);
        }

        public void On(string what, Action<string> handler)
        {
            if (what.Equals("agentchange"))
            {
                EventActions.Add(handler);
            }
            else
            {
                if (!GlobalEventActions.ContainsKey(what))
                {
                    GlobalEventActions[what] = new List<Action<string>>();
                }
                GlobalEventActions[what].Add(handler);
            }
        }

        // TODO: implement off events
        public void Off(string what, Action<string> handler)
        {
        }

        public void Close()
        {
            ApplicationSharedState.Disconnect();
        }

        void InvokeCallbacks(string what, object d)
        {
            JObject caps = new JObject();
            string json = JsonConvert.SerializeObject(d);
            json = json.Replace("\"[", "[");
            json = json.Replace("\"{", "{");
            json = json.Replace("\\n", "");
            json = json.Replace("\\", "");
            json = json.Replace("}\"}", "}}");
            json = json.Replace("]\"}", "]}");
            if (json[0] == '"') json = json.Substring(1, json.Length - 2);
            else if (json[json.Length - 1] == '"') json = json.Substring(0, json.Length - 1);

            JObject e;
            try
            {
                e = JObject.Parse(json);
            }
            catch (Exception a)
            {
                e = JObject.Parse("" + d);
                Debug.LogError(a.Message);
            }

            if (what.Equals("agentchange"))
            {
                string agentid = e["agentid"].ToString();

                if (e["agentContext"].ToString().Equals("null"))
                {
                    JObject remObject;
                    if (!CurrentAgentState.TryRemove(agentid, out remObject))
                    {
                        Debug.LogError("Could not remove: " + agentid);
                    }
                }
                else if (!CurrentAgentState.ContainsKey(agentid))
                {
                    CurrentAgentState[agentid] = new JObject
                    {
                        ["capabilities"] = new JObject(),
                        ["keys"] = new JArray()
                    };

                }

                JObject tnp = new JObject();
                tnp["capabilities"] = new JObject();
                tnp["keys"] = new JArray();
                e["diff"] = tnp;
                try
                {
                    if (!e["agentContext"].ToString().Equals("null"))
                    {
                        RemoteAPI api = GetAgent((e["agentid"].ToString())).Api;
                        JObject cs = api.Capabilities();
                        foreach (var c in cs)
                        {
                            string c1 = c.Key.ToString();
                            if (cs[c1] != null)
                            {
                                if (!CurrentAgentState.ContainsKey(api.AgentID))
                                {
                                    if(!CurrentAgentState.TryAdd(api.AgentID, new JObject()))
                                    {
                                        Debug.LogError("Could not add: " + api.AgentID);
                                    }
                                }
                                if (CurrentAgentState[api.AgentID].Property("capabilities") == null)
                                {

                                    CurrentAgentState[api.AgentID]["capabilities"] = new JObject();
                                    caps = new JObject();
                                }
                                else
                                {
                                    caps = (JObject)CurrentAgentState[api.AgentID]["capabilities"];
                                }
                                if (caps.Property(c1) == null)
                                {
                                    e["diff"]["capabilities"][c1] = cs[c1];
                                }
                                else
                                {
                                    if (caps[c1].ToString().Equals(cs[c1]))
                                    {
                                        e["diff"]["capabilities"][c1] = cs[c1];
                                    }
                                }
                            }
                        }

                        List<string> keys = api.Keys();
                        for (int i = 0; i < keys.Count; i++)
                        {
                            if (!CurrentAgentState.ContainsKey(api.AgentID))
                            {
                                if (!CurrentAgentState.TryAdd(api.AgentID, new JObject()))
                                {
                                    Debug.LogError("Could not add: " + api.AgentID);
                                }
                            }
                            if (CurrentAgentState[api.AgentID].Property("keys") == null)
                            {

                                CurrentAgentState[api.AgentID]["keys"] = new JArray();

                            }
                            if ((CurrentAgentState[api.AgentID]["keys"] as JArray).IndexOf(keys[i]) == -1)
                            {
                                (e["diff"]["keys"] as JArray).Add(keys[i]);
                            }
                        }
                        CurrentAgentState[api.AgentID] = new JObject
                        {
                            ["capabilities"] = JObject.Parse(JsonConvert.SerializeObject(cs)),
                            ["keys"] = JArray.Parse(JsonConvert.SerializeObject(keys))
                        };
                        ;
                    }

                    for (int i = 0; i < EventActions.Count; i++)
                    {
                        try
                        {
                            if (EventActions[i] != null) EventActions[i].Invoke(JsonConvert.SerializeObject(e));
                        }
                        catch (Exception err)
                        {
                            Debug.LogError("Error in agentchange callback: " + err + " e: " + json);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("expection app: " + ex + " input " + json);
                }
            }
            else
            {
                for (int i = 0, len = GlobalEventActions[what].Count; i < len; i++)
                {
                    try
                    {
                        if (GlobalEventActions.ContainsKey(what))
                            GlobalEventActions[what][i].Invoke(JsonConvert.SerializeObject(e));
                    }
                    catch (Exception err)
                    {
                        Debug.LogError("Error in agentchange callback: " + err);
                    }
                }
            }
        }

        RemoteAgent RemoteAgentContext(string agentId)
        {
            RemoteAgent self = new RemoteAgent(agentId);
            self.Handlers.Register("agentchange", new List<Action<string>>());
            ApplicationSharedState.On("change", (data) =>
            {
                JObject d = JObject.Parse(data);
                if (d["key"].ToString().IndexOf(Constants.GetVariable(self.AgentID)) == 0)
                {
                    var actions = self.Handlers.Get(d["key"].ToString());
                    foreach (var action in actions)
                    {
                        try
                        {
                            action.Invoke(data);
                        }
                        catch (Exception ee)
                        {
                            Debug.LogError("Error in callback: remote change" + ee);
                        }
                    }
                }
            });

            List<string> keys()
            {
                string meta = ApplicationSharedState.GetItem(Constants.GetMetaVariable(self.AgentID));
                if (!string.IsNullOrEmpty(meta))
                {
                    meta = meta.Replace("\\n", "");
                    meta = meta.Replace("\\", "");
                    meta = meta.Substring(1, meta.Length - 2);
                    meta = "[" + meta + "]";

                    JArray _meta = JArray.Parse(meta);
                    if (_meta != null) return new List<string>(JArray.FromObject(_meta[0]["keys"]).Values<string>());
                }
                return new List<string>();
            };

            JObject capabilities()
            {
                string meta = ApplicationSharedState.GetItem(Constants.GetMetaVariable(self.AgentID));
                if (!string.IsNullOrEmpty(meta))
                {
                    meta = meta.Replace("\\n", "");
                    meta = meta.Replace("\\", "");
                    meta = meta.Substring(1, meta.Length - 2);
                    meta = "[" + meta + "]";

                    JArray _meta = JArray.Parse(meta);
                    if (_meta != null) return (JObject)_meta[0]["capabilities"];
                }
                return new JObject();
            }

            string On(string what, Action<string> handler)
            {
                if (what.Equals("agentchange"))
                {
                    self.Handlers.Add("agentchange", handler);
                    return "";
                }

                string subscriptions = ApplicationSharedState.GetItem(Constants.GetMetaSubVariable(ApplicationSharedState.AgentID));
                JArray newSubscriptions;
                if (subscriptions == null) newSubscriptions = new JArray();
                else
                {
                    try
                    {
                        newSubscriptions = JArray.Parse(subscriptions);
                    }
                    catch (Exception)
                    {
                        newSubscriptions = new JArray();
                    }
                }

                string item = self.AgentID + "_" + what;
                if (newSubscriptions.IndexOf(item) == -1)
                {
                    newSubscriptions.Add(item);
                    ApplicationSharedState.SetItem(
                        Constants.GetMetaSubVariable(ApplicationSharedState.AgentID), 
                        JsonConvert.SerializeObject(newSubscriptions));
                }

                try
                {
                    if (!self.Handlers.TryGetValue(what, out List<Action<string>> v))
                    {
                        self.Handlers.Register(what, new List<Action<string>>());
                        self.Handlers.Add(what, handler);
                    }
                }
                catch (Exception exx)
                {
                    Debug.LogError("Adding handler" + exx);
                }

                // check if we have a value already
                string value = ApplicationSharedState.GetItem(Constants.GetVariable(item));
                if (value != null)
                {
                    self.Handlers.Get(what)[0].Invoke(JsonConvert.SerializeObject(value));
                }
                return "";
            }

            string Off(string what, Action<string> handler)
            {
                return "";
            }

            string UpdateMeta(string meta)
            {
                JObject _meta = JObject.Parse(meta);
                JObject caps = (JObject)_meta["capabilities"];
                Dictionary<string, JArray> diff = new Dictionary<string, JArray>
                {
                    { "added", new JArray() },
                    { "altered", new JArray() },
                    { "removed", new JArray() }
                };

                foreach (KeyValuePair<string, string> entry in LastCapabilities)
                {
                    if (caps[entry.Key] == null)
                    {
                        diff["removed"].Add(entry.Key);
                    }
                }

                return "";
            }

            string UpdateValue(string _aid, string updateKey, string updateValue)
            {
                if (!self.Handlers.TryGetValue(updateKey, out List<Action<string>> v)) return "";
                var actions = self.Handlers.Get(updateKey);
                for (int i = 0; i < actions.Count; i++)
                {
                    try
                    {
                        var obj = new
                        {
                            key = updateKey,
                            value = updateValue,
                            agentid = _aid
                        };
                        actions[i].Invoke(JsonConvert.SerializeObject(obj));
                    }
                    catch (Exception err)
                    {
                        Debug.LogError("Error in update: " + err);
                    }
                }
                return "";
            }

            string SetItem(string key, string value)
            {
                ApplicationSharedState.SetItem(Constants.GetVariable(self.AgentID + "_" + key), value);
                return "";
            }

            string GetItem(string key)
            {
                return ApplicationSharedState.GetItem(Constants.GetVariable(self.AgentID + "_" + key));
            }

            self.UpdateValue = UpdateValue;
            self.UpdateMeta = UpdateMeta;
            self.Api.AgentID = self.AgentID;
            self.Api.Keys = keys;

            self.Api.On = On;
            self.Api.Off = Off;
            self.Api.SetItem = SetItem;
            self.Api.GetItem = GetItem;
            self.Api.Capabilities = capabilities;

            return self;
        }

        public Dictionary<string, object> Serialize()
        {
            List<string> agentsIds = new List<string>();
            foreach (var agent in RemoteAgents.Values) agentsIds.Add(agent.AgentID);

            return new Dictionary<string, object>
            {
                { "subParam2Agent", JsonConvert.SerializeObject(AgentSubscriptionsByParam) },
                { "subAgent2Param", JsonConvert.SerializeObject(AgentSubscriptionsByAgent) },
                { "Current agent state", JsonConvert.SerializeObject(CurrentAgentState) },
                { "Last capabilities", JsonConvert.SerializeObject(LastCapabilities) },
                { "Remote agents", JsonConvert.SerializeObject(agentsIds) },
                { "sharedState", ApplicationSharedState },
            };
        }
    }
}