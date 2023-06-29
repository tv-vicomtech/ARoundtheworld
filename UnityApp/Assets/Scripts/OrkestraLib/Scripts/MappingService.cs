using System;
using System.Collections.Generic;
using UnityEngine;

using OrkestraLib.Message;
using OrkestraLib.Network;
using static OrkestraLib.Message.IPacket;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    public class MappingService : ISerializable
    {
        public IWebSocketAdapter Connection { get; set; }

        public Dictionary<string, List<Action<string>>> Callbacks { get; private set; }

        public Stack<Action<string>> WaitingUserPromises { get; private set; }

        public Stack<Action<string>> WaitingGroupPromises { get; private set; }

        public Dictionary<string, string> State { get; private set; }

        public StateType ReadyState { get; private set; }

        private readonly IOrkestra Orkestra;

        public MappingService(IOrkestra orkestra)
        {
            Orkestra = orkestra;

            WaitingGroupPromises = new Stack<Action<string>>();
            WaitingUserPromises = new Stack<Action<string>>();
            Callbacks = new Dictionary<string, List<Action<string>>>();

            ReadyState = StateType.CONNECTING;
            State = new Dictionary<string, string>
            {
                { "CONNECTING", StateType.CONNECTING.ToJSON() },
                { "OPEN", StateType.OPEN.ToJSON() },
                { "CLOSED", StateType.CLOSED.ToJSON() }
            };

            Callbacks.Add("readystatechange", new List<Action<string>>());

#if UNITY_EDITOR && ORKESTRALIB
            UnityDataViewer.Register("MappingService", this);
#endif
        }

        public void Connect(string url, Action<bool> resolve)
        {
            if(Connection == null)
            {
                Orkestra.OpenSocket("MappingService", url,
                    (conn) =>
                    {
                        Connection = conn;
                        Connection.On("mapping", OnMapping);
                    },
                    (connected) =>
                    {
                        UpdateReadyState(connected ? StateType.OPEN : StateType.CLOSED);
                        resolve(connected);
                    });
            }
            else
            {
                Connection.Connect(url, 
                    null,
                    (connected) =>
                    {
                        UpdateReadyState(connected ? StateType.OPEN : StateType.CLOSED);
                        resolve(connected);
                    });
            }
        }

        public void OnMapping(string response)
        {
            try
            {
                var host = Connection.GetURL();
                GroupUrl groupURL = new GroupUrl(ArgsType.JSON, response, true);
                string url = host + (host.EndsWith("/") ? "" : "/") + groupURL.group;
                //Debug.Log("ON MAPPING " + url);
                GroupUrl g = new GroupUrl(ArgsType.Value, url);
                if (WaitingGroupPromises.Count > 0)
                {
                    Action<string> promise = WaitingGroupPromises.Pop();
                    promise.Invoke(g.ToJSON());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public void UpdateReadyState(StateType newState)
        {
            //Debug.Log("UpdateReadyState " + newState);
            if (newState != StateType.UNDEFINED && newState != StateType.CLOSED && newState != ReadyState)
            {
                ReadyState = newState;
                InvokeCallbacks("readystatechange", newState.ToJSON());
            }
        }

        public void InvokeCallbacks(string what, string e)
        {
            if (!Callbacks.ContainsKey(what)) Debug.Log("Unsupported event " + what);
            foreach (var cb in Callbacks[what])
            {
                try
                {
                    cb.Invoke(e);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error in " + what + ": " + ex);
                }
            }
        }

        public void GetGroupMapping(string groupId, Action<string> resolve)
        {
            if (Connection == null || !Connection.IsConnected()) throw new Exception("no connection");
            if (!string.IsNullOrEmpty(groupId))
            {
                Connection.Emit("getMapping", new GroupId(ArgsType.Value, groupId));
            }
            
            WaitingGroupPromises.Push(resolve);
        }

        public Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                { "ReadyState", ReadyState.ToJSON() }
            };
        }
    }
}