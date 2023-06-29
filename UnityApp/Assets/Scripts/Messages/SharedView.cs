using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class SharedView : Message
        {
            public bool active;

            public SharedView(string json) : base(json) { }

            public SharedView(string userId, bool active) :
                base(typeof(SharedView).Name, userId)
            {
                this.active = active;
            }

            public override string FriendlyName()
            {
                return typeof(SharedView).Name;
            }
        }
    }
}
