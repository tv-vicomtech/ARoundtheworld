using NUnit.Framework;
using System;

namespace Tests
{
    [TestFixture]
    public class RemoteAgent
    {
        OrkestraLib.RemoteAgent agent;

        [SetUp]
        public void SetupAgent()
        {
            agent = new OrkestraLib.RemoteAgent("testId");
        }

        [Test]
        public void AgentID()
        {
            Assert.That(agent.AgentID, Is.EqualTo("testId"));
        }

        [Test]
        public void UpdateValue()
        {
            Func<string, string, string, string> testUpdateValue = (string a, string b, string c) =>
            {
                Assert.That(a, Is.EqualTo("testValue"));
                return a;
            };
            agent.UpdateValue = testUpdateValue;
            Assert.That(agent.UpdateValue("testValue", "", ""), Is.EqualTo("testValue"));
        }

        [Test]
        public void UpdateMeta()
        {
            Func<string, string> testUpdateMeta = (string value) =>
            {
                Assert.That(value, Is.EqualTo("testValue"));
                return value;
            };
            agent.UpdateMeta = testUpdateMeta;
            Assert.That(agent.UpdateMeta("testValue"), Is.EqualTo("testValue"));
        }

        [Test]
        public void PublicAPI()
        {
            Assert.That(agent.AgentID, Is.EqualTo("testId"));
        }
    }
}