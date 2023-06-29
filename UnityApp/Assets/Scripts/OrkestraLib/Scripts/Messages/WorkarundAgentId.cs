using System;
using static OrkestraLib.Message.IPacket;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class WorkarundAgentId : IPacket
        {
            public string agentid;

            /// <summary>
            /// Instantiates a agentid object from a json
            /// </summary>
            public WorkarundAgentId(ArgsType t, string value)
            {
                if (t == ArgsType.JSON) this.InstantiateWithJSON(value);
                else if (t == ArgsType.Value) agentid = value;
            }

            public string FriendlyName()
            {
                return typeof(WorkarundAgentId).Name;
            }
        }
    }
}
