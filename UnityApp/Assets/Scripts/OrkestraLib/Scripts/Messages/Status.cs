using System;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class Status : IPacket
        {
            public int clients;
            public KeyValue[] presence;

            public Status(string json)
            {
                this.InstantiateWithJSON(json.GetArrayValue());
            }

            public Status(int numClients, string key, string value)
            {
                clients = numClients;
                presence = new KeyValue[] { new KeyValue(key, value) };
            }

            public string FriendlyName()
            {
                return typeof(Status).Name;
            }
        }
    }
}