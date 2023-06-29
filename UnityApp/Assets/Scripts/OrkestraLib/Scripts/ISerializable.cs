using System.Collections.Generic;

namespace OrkestraLib
{
    public interface ISerializable
    {
        public Dictionary<string, object> Serialize();
    }
}
