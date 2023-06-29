using System;
using UnityEngine;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class ObjectTransform : Message
        {
            public float posX, posY, posZ;
            public float scaleX, scaleY, scaleZ;
            public float rotX, rotY, rotZ;
            public string name;
            public float[] quat;

            public ObjectTransform(string json) : base(json) { }

            public ObjectTransform(GameObject obj) : this("", obj) { }

            public ObjectTransform(string sender, GameObject obj) :
                base(typeof(ObjectTransform).Name, sender)
            {
                UnityEngine.Transform t = obj.transform;

                posX = 0;
                posY = 0;
                posZ = 0;

                name = t.name;

                if (Vector3.Distance(Vector3.zero, t.localPosition) > 0.00001f)
                {
                    posX = t.localPosition.x;
                    posY = t.localPosition.y;
                    posZ = t.localPosition.z;
                }

                scaleX = t.localScale.x;
                scaleY = t.localScale.y;
                scaleZ = t.localScale.z;

                rotX = t.localEulerAngles.x;
                rotY = t.localEulerAngles.y;
                rotZ = t.localEulerAngles.z;

                quat = new float[] { t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w };
            }

            public void Update(UnityEngine.Transform obj)
            {
                obj.localPosition = new Vector3(posX, posY, posZ);
                obj.localScale = new Vector3(scaleX, scaleY, scaleZ);
                obj.localRotation = Quaternion.Euler(rotX, rotY, rotZ);
            }

            public override string FriendlyName()
            {
                return typeof(ObjectTransform).Name;
            }
        }
    }
}

