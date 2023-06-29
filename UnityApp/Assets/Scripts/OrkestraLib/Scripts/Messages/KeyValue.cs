using System;
using UnityEngine;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class KeyValue : IPacket, IKeyValue
        {
            public string key;
            public string value;

            public KeyValue()
            {
                key = "";
                value = "";
            }

            public KeyValue(string json)
            {
                this.InstantiateWithJSON(json);
            }

            public KeyValue(string key, string value)
            {
                this.key = key;
                this.value = value;
            }

            public KeyValue(KeyValueType key, string value)
            {
                this.key = key.ToJSON();
                this.value = value;
            }

            public string StringVal()
            {
                return value;
            }

            public string[] ListVal()
            {
                return new string[] { value };
            }

            public bool IsEvent(Type expected)
            {
                // init1 is an hardcoded message in orkestralib
                return key.Equals(expected.Name) && !value.IsSystemInitMessage();
            }

            public bool ValueEquals(string v)
            {
                return value.Equals(v);
            }

            public virtual string FriendlyName()
            {
                return typeof(KeyValue).Name;
            }
        }
    }
}
