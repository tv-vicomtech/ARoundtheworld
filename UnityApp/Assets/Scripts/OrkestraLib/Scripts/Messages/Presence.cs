using System;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        public enum PresenceType
        {
            Online,
            Offline
        };

        public static class PresenceTypeExt
        {
            public static string ToJSON(this PresenceType me)
            {
                return me switch
                {
                    PresenceType.Online => "online",
                    PresenceType.Offline => "offline",
                    _ => "offline",
                };
            }
        }

        [Serializable]
        public class Presence : IPacket
        {
            public string agentID;
            public string presence;

            /// <summary>
            /// Instantiates the object from a json
            /// </summary>
            public Presence(string json)
            {
                this.InstantiateWithJSON(json);
            }

            public Presence(string agentID, PresenceType presence)
            {
                this.agentID = agentID;
                this.presence = presence.ToJSON();
            }

            public string FriendlyName()
            {
                return typeof(Presence).Name;
            }
        }
    }
}
