using System;
using UnityEngine;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class CameraTransform : ObjectTransform
        {
            public float fieldOfView = 60f;
            public float posX2, posY2, posZ2;
            public float[] camQuat;

            public CameraTransform(string json) : base(json) { }

            public CameraTransform(GameObject obj, GameObject rs) : this("", obj, rs) { }

            public CameraTransform(string sender, GameObject obj, GameObject rs) :
                base(sender, obj)
            {
                type = typeof(CameraTransform).Name;
                Camera cm = obj.GetComponent<Camera>();
                fieldOfView = cm.fieldOfView;

                camQuat = new float[] { cm.transform.rotation.x, cm.transform.rotation.y, cm.transform.rotation.z, cm.transform.rotation.w };

                posX2 = rs.transform.localPosition.x;
                posY2 = rs.transform.localPosition.y;
                posZ2 = rs.transform.localPosition.z;
            }

            public void Update(UnityEngine.Transform obj, UnityEngine.Transform rs)
            {
                Update(obj);
                if (rs != null)
                {
                    Camera cm = obj.gameObject.GetComponent<Camera>();
                    if (cm)
                    {
                        cm.fieldOfView = fieldOfView;
                    }
                    rs.localPosition = new Vector3(posX2, posY2, posZ2);
                }
            }

            public override string FriendlyName()
            {
                return typeof(CameraTransform).Name;
            }
        }
    }
}

