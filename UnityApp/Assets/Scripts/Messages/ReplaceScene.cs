using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class ReplaceScene : Message
        {
            bool replaceScene;
            public ReplaceScene(string json) : base(json) { }
    
            public ReplaceScene(string sender, bool replace) :
              base(typeof(ReplaceScene).Name, sender)
            {
                replaceScene = replace;
            }
    
            public override string FriendlyName()
            {
                return typeof(Notification).Name;
            }
        }
    }
}