using System;
using UnityEngine;

namespace OrkestraLib
{
    namespace Message
    {
        [Serializable]
        public class Answer : Message
        {
            public string name;

            public string screenshot;

            public float px;
            public float py;
            public float pz;

            public float sx;
            public float sy;
            public float sz;

            public float rx;
            public float ry;
            public float rz;

            public float time_left = 0;

            public Answer(string json) : base(json) { }

            public Answer(string sender, GameObject go, PushPin pin) :
                base(typeof(Answer).Name, sender)
            {
                name = go.name;

                px = pin.gameObject.transform.localPosition.x;
                py = pin.gameObject.transform.localPosition.y;
                pz = pin.gameObject.transform.localPosition.z;

                sx = pin.transform.localScale.x;
                sy = pin.transform.localScale.y;
                sz = pin.transform.localScale.z;

                rx = pin.transform.localRotation.x;
                ry = pin.transform.localRotation.y;
                rz = pin.transform.localRotation.z;
            }

            public Answer(PendingAnswer pAnswer)
            {
                name = pAnswer.name;
                px = pAnswer.px;
                py = pAnswer.py;
                pz = pAnswer.pz;

                sx = pAnswer.sx;
                sy = pAnswer.sy;
                sz = pAnswer.sz;

                rx = pAnswer.rx;
                ry = pAnswer.ry;
                rz = pAnswer.rz;
            }

            public override string FriendlyName()
            {
                return typeof(Answer).Name;
            }

            public PendingAnswer toPendingAnswer()
            {
                PendingAnswer pendingAnswer = new PendingAnswer();
                pendingAnswer.sender = this.sender;
                pendingAnswer.screenshot = this.screenshot;
                pendingAnswer.name = this.name;
                pendingAnswer.px = this.px;
                pendingAnswer.py = this.py;
                pendingAnswer.pz = this.pz;
                pendingAnswer.rx = this.rx;
                pendingAnswer.ry = this.ry;
                pendingAnswer.rz = this.rz;
                pendingAnswer.sx = this.sx;
                pendingAnswer.sy = this.sy;
                pendingAnswer.sz = this.sz;
                pendingAnswer.type = "PendingAnswer";
                return pendingAnswer;
            }

            public void Apply(GameObject hitObject, PushPin pin)
            {
                pin.transform.SetParent(hitObject.transform, false);
                pin.transform.localPosition = new Vector3(px, py, pz);
                pin.transform.localScale = new Vector3(sx, sy, sz);
                pin.transform.localRotation = Quaternion.Euler(rx, ry, rz);
                pin.transform.LookAt(hitObject.transform);
                pin.transform.rotation = pin.transform.rotation * Quaternion.Euler(180, 0, 0);

                pin.SetColor(Color.red);
            }

            public Vector3 OnPlanet()
            {
                return UnityEngine.Random.onUnitSphere * sx;
            }

            public Vector3 Location()
            {
                return new Vector3(px, py, pz);
            }

            public void SetScreenShot(string s)
            {
                screenshot = s;
            }
        }
    }
}
