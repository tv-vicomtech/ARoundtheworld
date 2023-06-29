namespace OrkestraLib
{
    namespace Message
    {
        public interface IPacket
        {
            public enum ArgsType
            {
                JSON,
                Value
            }

            public string FriendlyName();
        }
    }
}
