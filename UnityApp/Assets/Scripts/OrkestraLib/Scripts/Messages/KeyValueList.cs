using System;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class KeyValueList : IPacket, IKeyValue
        {
            public string key;
            public string[] value;

            public KeyValueList(string json)
            {
                this.InstantiateWithJSON(json);
            }

            public KeyValueList(string key, string[] value)
            {
                this.key = key;
                this.value = value;
            }

            public string StringVal()
            {
                return "";
            }

            public string[] ListVal()
            {
                return value;
            }

            public string FriendlyName()
            {
                return typeof(KeyValueList).Name;
            }
        }
    }
}

