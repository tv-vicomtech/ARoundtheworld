using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using UnityEngine;
using OrkestraLib.Message;
using static OrkestraLib.Message.IPacket;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    /// <summary>
    /// The agent context provides information about a specific agent, its capabilities and events. 
    /// It is used to store and send data related to a specific agent.
    /// Documentation on GitHub: https://github.com/mediascape/application-context/tree/master/API#agent-context
    /// </summary>
    public class AgentContext
    {
        public Dictionary<string, string> Capabilities { get; private set; }

        // Map key to {currentValue:null, callbacks:[], on:func, off:func}
        public readonly Dictionary<string, ContextItem> AgentContextItems;
        private readonly List<Action<string>> AgentUpdateHandlers;
        private readonly List<Action<string>> UpdateEvents;
        public string AgentID;

        public void InvokeAll(string ev, string data)
        {
            if (AgentContextItems.ContainsKey(ev))
            {
                ContextItem item = AgentContextItems[ev];
                foreach (Action<string> callback in item.EventActions)
                {
                    callback?.Invoke(data);
                    return; // TODO: what is this ? invoke first only ?
                }
            }
        }

        public AgentContext(string id, List<string> eventKeys)
        {
            Capabilities = new Dictionary<string, string>();
            UpdateEvents = new List<Action<string>>();
            AgentContextItems = new Dictionary<string, ContextItem>();
            AgentUpdateHandlers = new List<Action<string>>();

            AgentID = id;
            foreach (string eventKey in eventKeys)
            {
                AddContextElements(new InstrumentMap(eventKey,
                    (e) =>
                    {
                        SetCapability(eventKey, "supported");
                        return null;
                    },
                    (e) =>
                    {
                        SetItem(eventKey, InitializationMessage);
                        return null;
                    }, 
                    (e) => null
                   ));
            }
        }

        public void NotifyAgentChange()
        {
            for (int i = 0; i < AgentUpdateHandlers.Count; i++)
            {
                try
                {
                    // Workaround for a known problem with the server messages
                    var agentId = new WorkarundAgentId(ArgsType.Value, AgentID);
                    AgentUpdateHandlers[i].Invoke(agentId.ToJSON());
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error in meta-update" + ex);
                }
            }
        }

        public void SetCapability(string what, string state)
        {
            try
            {
                Capabilities[what] = state;
                NotifyAgentChange();
                TriggerEvents(what, "keychange");
            }
            catch (Exception ex)
            {
                Debug.LogError("What: " + what + " State: " + state + "\n" + ex);
            }
        }

        public void AddContextElements(InstrumentMap imap)
        {
            AgentContextItems.Add(imap.Value, new ContextItem(imap));
            imap.InitEvent(this);
            NotifyAgentChange();
        }

        public void TriggerEvents(string evt, string args)
        {
            if (evt.Equals("keychange"))
            {
                for (int i = 0; i < UpdateEvents.Count; i++)
                {
                    try
                    {
                        UpdateEvents[i].Invoke(args);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error in keychange handler:" + ex);
                    }
                }
                return;
            }

            if (AgentContextItems.TryGetValue(evt, out ContextItem ctx))
            {
                foreach (Action<string> evtAction in ctx.EventActions)
                {
                    try
                    {
                        evtAction.Invoke(args);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("What: " + evt + " State: " + args + "\n" + ex);
                    }
                }
            }
        }

        public void Off(string what, Action<string> handler)
        {
        }

        public void On(string what, Action<string> handler)
        {
            if (string.IsNullOrEmpty(what)) return;
            if (what.Equals("keychange"))
            {
                UpdateEvents.Add(handler);
                handler.Invoke(JsonConvert.SerializeObject(Keys()));
                return;
            }
            else if (what.Equals("agentchange"))
            {
                AgentUpdateHandlers.Add(handler);
                try
                {
                    handler.Invoke(new WorkarundAgentId(ArgsType.Value, AgentID).ToJSON()); //self as parameter
                }
                catch (Exception err)
                {
                    Debug.LogError("Error in agentchange handler" + err);
                }
                return;
            }

            if (!AgentContextItems.ContainsKey(what)) Debug.Log("Unsupported event " + what);
            else
            {
                var ctx = AgentContextItems[what];
                int index = -1;
                for (int i = 0; i < ctx.EventActions.Count; i++)
                {
                    if (ctx.EventActions[i].Equals(handler)) index = i;
                }

                if (index == -1)
                {
                    if (ctx.EventActions.Count == 0)
                    {
                        try
                        {
                            ctx.OnEvent(this); //self as parameter
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError("Exception agentContext on " + ex);
                        }
                    }

                    // Register handler
                    ctx.EventActions.Add(handler);
                    TriggerEvents(what, new KeyValue(what, ctx.CurrentValue).ToJSON());
                }
            }
        }

        public void SetItem(string what, string value)
        {
            if (!AgentContextItems.ContainsKey(what))
            {
                AddContextElements(new InstrumentMap(value));
            }

            AgentContextItems[what].CurrentValue = value;
            TriggerEvents(what, new KeyValue(what, value).ToJSON());
        }

        public string GetItem(string what)
        {
            if (AgentContextItems.ContainsKey(what))
                return AgentContextItems[what].CurrentValue;
            return null;
        }

        public List<string> Keys()
        {
            return new List<string>(AgentContextItems.Keys);
        }
    }
}

