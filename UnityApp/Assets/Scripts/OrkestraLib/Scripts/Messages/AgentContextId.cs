using System;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class AgentContextId : IPacket
        {
            public string agentid;
            public string agentContext;

            public AgentContextId(string json)
            {
                this.InstantiateWithJSON(json);
            }

            public AgentContextId(string agentId, string agentContext)
            {
                this.agentid = agentId;
                this.agentContext = agentContext;
            }

            public string FriendlyName()
            {
                return typeof(AgentContextId).Name;
            }
        }
    }
}
