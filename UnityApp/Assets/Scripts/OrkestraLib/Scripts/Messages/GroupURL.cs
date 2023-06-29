using System;
using static OrkestraLib.Message.IPacket;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class GroupUrl : IPacket
        {
            public string group;

            /// <summary>
            /// Instantiates a group object from a json
            /// </summary>
            public GroupUrl(ArgsType t, string value, bool fixArray = false)
            {
                string val = fixArray ? value.GetArrayValue() : value.FixJSON();
                if (t == ArgsType.JSON) this.InstantiateWithJSON(val);
                else if (t == ArgsType.Value) group = value;
            }

            public string FriendlyName()
            {
                return typeof(GroupUrl).Name;
            }
        }
    }
}
