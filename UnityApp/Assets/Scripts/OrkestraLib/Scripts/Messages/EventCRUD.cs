using System;
using System.Collections.Generic;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class EventCRUD : IPacket
        {
            public List<string> added;
            public List<string> removed;
            public List<string> updated;

            public EventCRUD(string json)
            {
                this.InstantiateWithJSON(json);
            }

            public EventCRUD(Dictionary<string, List<string>> diff)
            {
                added = diff.ContainsKey("added") ? diff["added"] : new List<string>();
                removed = diff.ContainsKey("removed") ? diff["removed"] : new List<string>();
                updated = diff.ContainsKey("updated") ? diff["updated"] : new List<string>();
            }

            public string FriendlyName()
            {
                return typeof(EventCRUD).Name;
            }
        }
    }
}
