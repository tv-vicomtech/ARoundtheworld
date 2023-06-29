using System;
using UnityEngine;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class QuizFinishMessage : Message
        {
            public string name;
            public string quiz;

            public QuizFinishMessage(string json) : base(json) { }

            public QuizFinishMessage(string userId, string name, string quiz) :
                base(typeof(QuizFinishMessage).Name, userId)
            {
                this.name = name;
                this.quiz = quiz;
            }

            public override string FriendlyName()
            {
                return typeof(QuizFinishMessage).Name;
            }
        }
    }
}
