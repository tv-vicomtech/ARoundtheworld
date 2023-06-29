using System;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class Question : Message
        {
            public string content;
            public string id;
            public string username;
            public string correctAnswer;

            public Question(string json) : base(json) { }

            public Question(string sender, string pContent, string pId, string pUsername, string pCorrectAnswer) :
                base(typeof(Question).Name, sender)
            {
                this.content = pContent;
                this.id = pId;
                this.username = pUsername;
                this.correctAnswer = pCorrectAnswer;
            }

            public override string FriendlyName()
            {
                return typeof(Question).Name;
            }
        }
    }
}
