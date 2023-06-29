using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class HelpMessage : Message
        {
            public string helpType;
            public string content;

            public HelpMessage(string json) : base(json) { }

            public HelpMessage(string sender, string content, string helpType) :
              base(typeof(HelpMessage).Name, sender)
            {
                this.content = content;
                this.helpType = helpType;
            }

            public string GetContent()
            {
                return content;
            }

            public override string FriendlyName()
            {
                return typeof(HelpMessage).Name;
            }
        }
    }
}
