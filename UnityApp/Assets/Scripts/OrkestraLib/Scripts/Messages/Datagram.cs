using System;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class Datagram : IPacket
        {
            public string type;
            public string key;
            public string value;

            /// <summary>
            /// Instantiates the object from a json
            /// </summary>
            public Datagram(string json)
            {
                this.InstantiateWithJSON(json);
            }

            public string FriendlyName()
            {
                return typeof(Datagram).Name;
            }
        }
    }
}
