using System;
using UnityEngine;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class SelectObject : Message
        {
            public string name;

            public SelectObject(string json) : base(json) { }

            public SelectObject(string userId, string scene) :
                base(typeof(SelectObject).Name, userId)
            {
                this.name = scene;
            }

            public override string FriendlyName()
            {
                return typeof(SelectObject).Name;
            }
        }
    }
}
