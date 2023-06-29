using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class ActiveCamera : Message
        {
       
            public bool value;
            public string user;

            public ActiveCamera(string json) : base(json) { }


            public ActiveCamera(string userId, bool value, string user) :
                base(typeof(ActiveCamera).Name, userId)
            {
                this.user = user;
                this.value = value;
            }


            public override string FriendlyName()
            {
                return typeof(ActiveCamera).Name;
            }
        }
    }
}
