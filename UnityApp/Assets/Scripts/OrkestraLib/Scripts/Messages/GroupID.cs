using System;
using static OrkestraLib.Message.IPacket;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class GroupId : IPacket
        {
            public string groupId;

            /// <summary>
            /// Instantiates a groupId object from a json
            /// </summary>
            public GroupId(ArgsType t, string value)
            {
                if (t == ArgsType.JSON) this.InstantiateWithJSON(value); 
                else if (t == ArgsType.Value) groupId = value;
            }

            public string FriendlyName()
            {
                return typeof(GroupId).Name;
            }
        }
    }
}
