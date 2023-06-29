using System;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class Sender : IPacket
        {
            public string sender;

            public Sender(string value)
            {
                this.InstantiateWithJSON(value);
            }

            public string FriendlyName()
            {
                return typeof(Sender).Name;
            }
        }
    }
}
