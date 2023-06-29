using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class MyNotification : Message
        {
            public string content;

            public MyNotification(string json) : base(json) { }

            public MyNotification(string sender, string content):
              base(typeof(MyNotification).Name, sender)
            {
                this.content = content;
            }

            public string GetContent()
            {
                return content;
            }
     
            public override string FriendlyName()
            {
                return typeof(MyNotification).Name;
            }
        }
    }
}
