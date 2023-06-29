using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class Result : Message
        {
            public string value;

            public Result(string json) : base(json) { }

            public Result(string sender, string value) :
                base(typeof(Result).Name, sender)
            {
                this.value = value;
            }

            public override string FriendlyName()
            {
                return typeof(Result).Name;
            }
        }
    }
}
