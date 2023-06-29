using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class StatusWebRTC : Message
        {
            public string status;

            public StatusWebRTC(string json) : base(json) { }

            public StatusWebRTC(string userId, string status) :
                base(typeof(StatusWebRTC).Name, userId)
            {
                this.status = status;
            }

            public override string FriendlyName()
            {
                return typeof(StatusWebRTC).Name;
            }
        }
    }
}
