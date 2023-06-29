using System;
using UnityEngine;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class ScoreMessage : Message
        {
            public string name;
            public string score;

            public ScoreMessage(string json) : base(json) { }

            public ScoreMessage(string userId, string name, string score) :
                base(typeof(ScoreMessage).Name, userId)
            {
                this.name = name;
                this.score = score;
            }

            public override string FriendlyName()
            {
                return typeof(ScoreMessage).Name;
            }
        }
    }
}
