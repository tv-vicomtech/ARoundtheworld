using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using OrkestraLib.Message;
using OrkestraLib;
using OrkestraLib.Plugins;
using UnityEditor.SceneManagement;
using System;
using static OrkestraLib.Orkestra;
using Object = UnityEngine.Object;

namespace Tests
{
    [TestFixture]
    public class Cloudflexcontrol
    {
        private bool sceneLoaded;
        private bool connected;
        private OrkestraBasic orkestra;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            EditorSceneManager.OpenScene("Assets/Scripts/OrkestraLib-HSIO/Scenes/H.SocketIO.Tests.unity", OpenSceneMode.Single);
            orkestra = Object.FindObjectOfType<OrkestraBasic>();
            sceneLoaded = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown() { }

        [UnityTest]
        public IEnumerator TestSceneNotNullAfterLoad()
        {
            yield return new WaitWhile(() => sceneLoaded == false);
            Assert.IsNotNull(orkestra);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestAppConnect()
        {
            yield return new WaitWhile(() => sceneLoaded == false);
            Assert.IsNotNull(orkestra);

            bool hasErrors = false;
            bool isConnected = false;
            bool joined = false;
            bool done = false;

            // Use HSocketIO with this instance of Orkestra
            OrkestraWithHSIO.Install(orkestra, (graceful, message) => { });

            // Register custom messages that will be used in communications
            orkestra.RegisterEvents(new Type[]{
                typeof(DataMessage)
            });

            // Listen to user events
            void UserEventSubscriber(object sender, UserEvent evt)
            {
                if (evt.IsPresenceEvent())
                {
                    if (evt.IsUser(orkestra.agentId) && evt.IsJoinEvent())
                    {
                        joined = true;
                    }
                }
                done = true;
            }
            orkestra.UserEvents += UserEventSubscriber;

            // Connection data
            orkestra.url = "https://cloud.flexcontrol.net";
            orkestra.room = "despacho";
            orkestra.agentId = "user1";

            // Connect
            orkestra.Connect(() =>
            {
                isConnected = true;
            },
            (isError, message) =>
            {
                if (isError) hasErrors = true;
            });

            yield return new WaitWhile(() => !hasErrors && !isConnected);
            Assert.IsTrue(isConnected);
            Assert.IsFalse(hasErrors);

            yield return new WaitWhile(() => !done);
            Assert.IsTrue(joined);
            connected = true;

            yield return null;
        }

        [UnityTest]
        public IEnumerator TestSendAppData()
        {
            yield return new WaitWhile(() => connected == false);

            bool done = false;

            // Listen to application messages
            void AppEventSubscriber(object sender, ApplicationEvent evt)
            {
                if (evt.IsEvent(typeof(DataMessage)))
                {
                    DataMessage msg = new DataMessage(evt.value);
                    if (msg.value.Equals("test"))
                    {
                        done = true;
                    }
                }
            }
            orkestra.ApplicationEvents += AppEventSubscriber;

            // Send custom message
            orkestra.Dispatch(Channel.Application, new DataMessage(orkestra.agentId, "test"));

            yield return new WaitWhile(() => done == false);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestSendUserData()
        {
            yield return new WaitWhile(() => connected == false);
            bool done = false;

            // Listen to user events
            void UserEventSubscriber(object sender, UserEvent evt)
            {
                if (evt.IsEvent(typeof(DataMessage)))
                {
                    DataMessage msg = new DataMessage(evt.data.value);
                    if (msg.value.Equals("test") && msg.sender.Equals("fakeUser"))
                    {
                        done = true;
                    }
                }
            }
            orkestra.UserEvents += UserEventSubscriber;

            // Send custom message
            orkestra.Dispatch(Channel.User, new DataMessage("fakeUser", "test"), orkestra.agentId);

            yield return new WaitWhile(() => done == false);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestZeroDatabase()
        {
            yield return new WaitWhile(() => connected == false);
            orkestra.DispatchUnset(Channel.Application, typeof(DataMessage));
            yield return null;
        }
    }
}