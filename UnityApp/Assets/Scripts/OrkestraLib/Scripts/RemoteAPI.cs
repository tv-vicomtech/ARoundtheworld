using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OrkestraLib
{
    public class RemoteAPI
    {
        public string AgentID { get; set; }

        public Func<string, Action<string>, string> On { get; set; }

        public Func<string, Action<string>, string> Off { get; set; }

        public Func<string, string, string> SetItem { get; set; }

        public Func<string, string> GetItem { get; set; }

        public Func<JObject> Capabilities { get; set; }

        public Func<List<string>> Keys { get; set; }
    }
}