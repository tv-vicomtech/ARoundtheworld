using System;
using static OrkestraLib.Message.IPacket;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class AgentId : IPacket
        {
            public string agentID;

            /// <summary>
            /// Instantiates a agentID object from a json
            /// </summary>
            public AgentId(ArgsType t, string value)
            {
                if (t == ArgsType.JSON) this.InstantiateWithJSON(value);
                else if (t == ArgsType.Value) agentID = value;
            }

            public string FriendlyName()
            {
                return typeof(AgentId).Name;
            }
        }
    }
}
