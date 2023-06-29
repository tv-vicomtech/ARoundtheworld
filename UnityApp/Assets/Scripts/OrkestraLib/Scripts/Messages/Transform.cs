using UnityEngine;
using System;
using static OrkestraLib.Utilities.ParserExtensions;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class Transform : IPacket
        {
            public double[] position;
            public double[] rotation;

            /// <summary>
            /// Instantiates the object from a json
            /// </summary>
            public Transform(string json) : this(Vector3.zero, Vector3.zero)
            {
                this.InstantiateWithJSON(json);
            }

            public Transform(Vector3 position, Vector3 rotation)
            {
                this.position = new double[] { position.x, position.y, position.z };
                this.rotation = new double[] { rotation.x, rotation.y, rotation.z };
            }

            public string FriendlyName()
            {
                return typeof(Transform).Name;
            }
        }
    }
}
