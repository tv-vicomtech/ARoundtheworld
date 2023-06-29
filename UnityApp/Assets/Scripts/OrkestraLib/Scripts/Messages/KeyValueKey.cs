using System;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        public enum KeyValueType
        {
            Data,
            OrderAgentId,
            Cameras
        };

        public static class KeyValueTypeExt
        {
            public static string ToJSON(this KeyValueType me)
            {
                return me switch
                {
                    KeyValueType.Data => "data",
                    KeyValueType.OrderAgentId => "order_agentid",
                    KeyValueType.Cameras => "cameras",
                    _ => "",
                };
            }
        }

        [Serializable]
        public class KeyValueKey : IPacket
        {
            public string key;

            public KeyValueKey(KeyValueType key)
            {
                this.key = key.ToJSON();
            }

            public KeyValueKey(string json)
            {
                this.InstantiateWithJSON(json);
            }

            public bool IsType(KeyValueType key)
            {
                return this.key.Equals(key.ToJSON());
            }

            public string FriendlyName()
            {
                return typeof(KeyValueKey).Name;
            }
        }
    }
}
