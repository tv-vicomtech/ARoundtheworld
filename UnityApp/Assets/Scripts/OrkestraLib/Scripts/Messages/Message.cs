using System;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class Message : IPacket
        {
            private readonly string data;
            public string type;
            public string sender;

            protected Message() { }

            public Message(string value)
            {
                data = value;
                this.InstantiateWithJSON(value);
            }

            public Message(string type, string sender)
            {
                this.type = type;
                this.sender = sender;
            }

            public string Data()
            {
                return data;
            }

            public virtual string FriendlyName()
            {
                return typeof(Message).Name;
            }
        }
    }
}
