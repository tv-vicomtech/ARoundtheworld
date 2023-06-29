using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class KeyValueEvent : KeyValue
        {
            public string evt;

            public KeyValueEvent(string json) : base(json) { }

            public KeyValueEvent(string evt, string key, string value) : base(key, value)
            {
                this.evt = evt;
            }

            public override string FriendlyName()
            {
                return typeof(KeyValueEvent).Name;
            }
        }
    }
}
