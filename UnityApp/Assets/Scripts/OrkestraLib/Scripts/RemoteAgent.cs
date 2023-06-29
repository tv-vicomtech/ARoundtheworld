using System;

namespace OrkestraLib
{
    public class RemoteAgent : Agent
    {
        public RemoteAPI Api { get; private set; }

        public Func<string, string, string, string> UpdateValue { get; set; }

        public Func<string, string> UpdateMeta { get; set; }

        public RemoteAgent(string agentId) : base(agentId)
        {
            Api = new RemoteAPI();
        }
    }
}