using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class ApplicationEvent : KeyValueEvent
        {
            public ApplicationEvent(string json) : base(json)
            {
            }

            public ApplicationEvent(string key, string value) : base("appEvent", key, value)
            {
            }

            public sealed override string FriendlyName()
            {
                return typeof(ApplicationEvent).Name;
            }

        }
    }
}
