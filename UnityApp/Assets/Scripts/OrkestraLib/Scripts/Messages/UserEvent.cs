using System;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        public enum UserEventType
        {
            Joined,
            Left,
            AgentEvent
        };

        public static class UserEventTypeExt
        {
            public static string ToJSON(this UserEventType me)
            {
                return me switch
                {
                    UserEventType.Left => "left",
                    UserEventType.AgentEvent => "agent_event",
                    UserEventType.Joined => "join",
                    _ => "",
                };
            }
        }

        [Serializable]
        public class UserEvent : IPacket
        {
            public string agentid;
            public string evt;
            public KeyValue data;

            public UserEvent(string json)
            {
                this.InstantiateWithJSON(json);
            }

            public UserEvent(string agentid, UserEventType evt)
            {
                this.agentid = agentid;
                this.evt = evt.ToJSON();

            }
            public UserEvent(string agentid, UserEventType evt, KeyValue data)
            {
                this.agentid = agentid;
                this.evt = evt.ToJSON();
                this.data = data;
            }

            public bool IsUser(string id)
            {
                return agentid.Equals(id);
            }

            public bool IsJoinEvent()
            {
                return evt.Equals(UserEventTypeExt.ToJSON(UserEventType.Joined));
            }

            public bool IsLeftEvent()
            {
                return evt.Equals(UserEventTypeExt.ToJSON(UserEventType.Left));
            }

            public bool IsPresenceEvent()
            {
                return evt.Equals(UserEventTypeExt.ToJSON(UserEventType.Joined)) ||
                       evt.Equals(UserEventTypeExt.ToJSON(UserEventType.Left));
            }

            public bool IsEvent(Type eventKey)
            {
                return data.key.Equals(eventKey.Name) && !data.value.IsSystemInitMessage();
            }

            public string FriendlyName()
            {
                return typeof(UserEvent).Name;
            }
        }
    }
}
