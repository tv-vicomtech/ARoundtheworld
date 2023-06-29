using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class TimeoutTurn : Message
        {
            public string username;
            public string questionId;

            public TimeoutTurn(string json) : base(json) { }

            public TimeoutTurn(string userId, string pUsername, string pQuestionId) :
                base(typeof(TimeoutTurn).Name, userId)
            {
                this.username = pUsername;
                this.questionId = pQuestionId;
            }
            public override string FriendlyName()
            {
                return typeof(TimeoutTurn).Name;
            }
        }
    }
}
