using System;


namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class MobileDevice : Message
        {
            public bool device;

            public MobileDevice(string json) : base(json) { }

            public MobileDevice(string sender, bool device) :
                base(typeof(MobileDevice).Name, sender)
            {
                this.device = device;
            }
        }
    }
}
