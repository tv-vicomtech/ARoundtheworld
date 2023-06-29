using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class User
    {
        OrkestraLib.User user;

        [SetUp]
        public void SetupUser()
        {
            user = new OrkestraLib.User("idVal", "nameVal", "profileVal");
        }

        [Test]
        public void IsMaster()
        {
            Assert.That(user.Master, Is.EqualTo(true));
        }

        [Test]
        public void AgentId()
        {
            Assert.That(user.AgentId, Is.EqualTo("idVal"));
        }

        [Test]
        public void Name()
        {
            Assert.That(user.Name, Is.EqualTo("nameVal"));
        }

        [Test]
        public void Profile()
        {
            Assert.That(user.Profile, Is.EqualTo("profileVal"));
        }
    }
}