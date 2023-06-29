using System;
using System.Collections.Generic;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class AgentChangeNotification : IPacket
        {
            public string agentid;
            public string agentContext;
            public EventCRUD diff;

            public AgentChangeNotification(string json)
            {
                this.InstantiateWithJSON(json);
            }

            public AgentChangeNotification(string agentid, string agentContext, Dictionary<string, List<string>> diff)
            {
                this.agentid = agentid;
                this.agentContext = agentContext;
                this.diff = new EventCRUD(diff);
            }

            public AgentChangeNotification(string agentid, string agentContext)
            {
                this.agentid = agentid;
                this.agentContext = agentContext;
                this.diff = new EventCRUD(new Dictionary<string, List<string>>());
            }

            public string FriendlyName()
            {
                return typeof(AgentChangeNotification).Name;
            }
        }
    }
}
