using System;
using System.Collections.Generic;

namespace OrkestraLib
{
    public class User
    {
        public string Name { get; private set; }

        public string AgentId { get; private set; }

        public string Profile { get; private set; }

        public bool Master { get; private set; }

        public Dictionary<string, string> Capacity { get; private set; }

        public string Context { get; set; }

        public User(string agentId) : this(agentId, agentId){}

        public User(string agentId, string name = "", string profile = "")
        {
            if (string.IsNullOrWhiteSpace(agentId))
            {
                throw new ArgumentException("User: invalid agentId");
            }

            Context = "";
            AgentId = agentId;
            Name = name;
            Profile = profile;
            Capacity = new Dictionary<string, string>();
            Master = true;
        }
    }

}