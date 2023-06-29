using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class DataMessage : Message
        {
            public string value;

            public DataMessage(string json) : base(json) { }

            public DataMessage(string sender, string data) :
                base(typeof(DataMessage).Name, sender)
            {
                this.value = data;
            }

            public override string FriendlyName()
            {
                return typeof(DataMessage).Name;
            }
        }
    }
}