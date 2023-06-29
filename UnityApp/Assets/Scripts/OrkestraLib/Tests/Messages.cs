using NUnit.Framework;
using OrkestraLib.Message;
using System.Collections.Generic;
using UnityEngine;
using static OrkestraLib.Message.IPacket;
using Transform = OrkestraLib.Message.Transform;
using static OrkestraLib.Utilities.ParserExtensions;

namespace Tests
{
    [TestFixture]
    public class Messages
    {
        [Test]
        public void Transform()
        {
            Transform t = new Transform(Vector3.one, Vector3.zero);
            string jsonText = "{\"position\":[1.0,1.0,1.0],\"rotation\":[0.0,0.0,0.0]}";
            Transform t2 = new Transform(jsonText);
            Assert.That(t.ToJSON(), Is.EqualTo(jsonText));
            Assert.That(t2.ToJSON(), Is.EqualTo(jsonText));
        }

        [Test]
        public void Datagram()
        {
            var json = "{\"type\":\"set\",\"key\":\"__meta__torreta\",\"value\":\"\"}";
            Datagram datagram = new Datagram(json);
            Assert.That(datagram.ToJSON(), Is.EqualTo(json));
        }

        [Test]
        public void Presence()
        {
            var json = "{\"agentID\":\"torreta\",\"presence\":\"online\"}";
            Presence presence = new Presence("torreta", PresenceType.Online);
            Assert.That(presence.ToJSON(), Is.EqualTo(json));
        }

        [Test]
        public void GroupId()
        {
            GroupId t = new GroupId(ArgsType.Value, "id");
            string jsonText = "{\"groupId\":\"id\"}";
            GroupId t2 = new GroupId(ArgsType.JSON, jsonText);
            Assert.That(t.ToJSON(), Is.EqualTo(jsonText));
            Assert.That(t2.ToJSON(), Is.EqualTo(jsonText));
        }

        [Test]
        public void UserEvent()
        {
            UserEvent t = new UserEvent("id1", UserEventType.Left);
            t.data = new KeyValue("data", "asd");
            string jsonText = "{\"agentid\":\"id1\",\"event\":\"left\",\"data\":{\"key\":\"data\",\"value\":\"asd\"}}";
            UserEvent t2 = new UserEvent(jsonText);
            Assert.That(t.ToJSON(), Is.EqualTo(jsonText));
            Assert.That(t2.ToJSON(), Is.EqualTo(jsonText));
        }

        [Test]
        public void AppEvent()
        {
            ApplicationEvent t = new ApplicationEvent("id1", "id");
            string jsonText = "{\"key\":\"id1\",\"value\":\"id\",\"event\":\"appEvent\"}";
            ApplicationEvent t2 = new ApplicationEvent(jsonText);
            Assert.That(t.ToJSON(), Is.EqualTo(jsonText));
            Assert.That(t2.ToJSON(), Is.EqualTo(jsonText));
        }

        [Test]
        public void KeyValueString()
        {
            KeyValue t = new KeyValue("torreta", "value");
            string jsonText = "{\"key\":\"torreta\",\"value\":\"value\"}";
            KeyValue t2 = new KeyValue(jsonText);
            Assert.That(t.ToJSON(), Is.EqualTo(jsonText));
            Assert.That(t2.ToJSON(), Is.EqualTo(jsonText));
        }

        [Test]
        public void KeyValueArray()
        {
            KeyValueList t = new KeyValueList("id1", new string[] { "id" });
            string jsonText = "{\"key\":\"id1\",\"value\":[\"id\"]}";
            KeyValueList t2 = new KeyValueList(jsonText);
            Assert.That(t.ToJSON(), Is.EqualTo(jsonText));
            Assert.That(t2.ToJSON(), Is.EqualTo(jsonText));
        }

        [Test]
        public void KeyValueKey()
        {
            string jsonText = "{\"key\":\"data\",\"value\":[\"id\"]}";
            KeyValueKey kv = new KeyValueKey(jsonText);
            Assert.That(kv.IsType(KeyValueType.Data), Is.EqualTo(true));
            Assert.That(kv.IsType(KeyValueType.Cameras), Is.EqualTo(false));
        }

        [Test]
        public void GroupURL()
        {
            string jsonText = "{\"group\":\"https://cloud.flexcontrol.net/5c68542d16df32cc11a9\"}";
            GroupUrl kv = new GroupUrl(ArgsType.JSON, jsonText);
            Assert.That(kv.ToJSON(), Is.EqualTo(jsonText));
        }

        [Test]
        public void AgentContextDiff()
        {
            string jsonText = "{\"agentid\":\"a\",\"agentContext\":\"b\",\"diff\":{\"added\":[\"d\"],\"removed\":[],\"updated\":[]}}";
            var diff = new Dictionary<string, List<string>>
            {
                { "added", new List<string>() { "d" } }
            };
            AgentChangeNotification acd = new AgentChangeNotification("a", "b", diff);
            Assert.That(acd.ToJSON(), Is.EqualTo(jsonText));
        }
    }
}
