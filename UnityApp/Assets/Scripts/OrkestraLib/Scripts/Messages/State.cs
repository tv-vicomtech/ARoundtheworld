using System;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        public enum StateType
        {
            UNDEFINED,
            CONNECTING,
            CONNECTED,
            DISCONNECTING,
            OPEN,
            CLOSED
        };

        public static class StateTypeExt
        {
            public static string ToJSON(this StateType me)
            {
                return me switch
                {
                    StateType.UNDEFINED => null,
                    StateType.CONNECTING => "connecting",
                    StateType.CONNECTED => "connected",
                    StateType.DISCONNECTING => "disconnecting",
                    StateType.OPEN => "open",
                    StateType.CLOSED => "closed",
                    _ => "",
                };
            }
        }

        [Serializable]
        public class State : IPacket
        {
            public KeyTypeValue[] presence;

            public State(string json)
            {
                this.InstantiateWithJSON(json);
            }

            public string FriendlyName()
            {
                return typeof(State).Name;
            }
        }
    }
}