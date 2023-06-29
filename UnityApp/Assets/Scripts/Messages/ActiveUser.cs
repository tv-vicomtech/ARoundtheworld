using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class ActiveUser : Message
        {
            public string username;

            public ActiveUser(string json) : base(json) { }

            public ActiveUser(string userId, string username) :
                base(typeof(ActiveUser).Name, userId)
            {
                this.username = username;
            }

            public override string FriendlyName()
            {
                return typeof(ActiveUser).Name;
            }
        }
    }
}
