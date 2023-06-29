using NUnit.Framework;
using OrkestraLib.Message;
using OrkestraLib.Network;
using System;
using System.Collections.Generic;

namespace Tests
{
    [TestFixture]
    public class MappingService
    {
        OrkestraLib.MappingService service;

        [SetUp]
        public void SetupMappingService()
        {
            var ork = new OrkestraMock
            {
                OpenSocket = delegate (string id, string url, Action<IWebSocketAdapter> conf, Action<bool> onConnect)
                {
                    var mock = new WebSocketAdapterMock();
                    conf(mock);
                    onConnect(true);
                },

                CloseSocket = delegate (IWebSocketAdapter socket)
                {
                    socket?.Disconnect();
                }
            };

            service = new OrkestraLib.MappingService(ork);
        }

        [Test]
        public void Connect()
        {
            Assert.That(service.ReadyState, Is.EqualTo(StateType.CONNECTING));
            service.Connect("http://..", (s) => { });
            Assert.That(true, Is.EqualTo(true));
        }

        [Test]
        public void GetGroupMapping()
        {
            service.Connect("http://..", (s) => { });
            Assert.That(service.WaitingGroupPromises.Count, Is.EqualTo(0));
            service.GetGroupMapping("var", (string s) => { });
            Assert.That(service.WaitingGroupPromises.Count, Is.EqualTo(1));
        }

        [Test]
        public void InvokeCallbacks()
        {
            service.Callbacks.Add("Test", new List<Action<string>> { (s) => { } });
            service.InvokeCallbacks("Test", "test");
        }

     
        
    }
}