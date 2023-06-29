using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class InstrumentMap
    {
        OrkestraLib.InstrumentMap instrumentMap;

        [SetUp]
        public void SetupInstrumentMap()
        {
            instrumentMap = new OrkestraLib.InstrumentMap("val");
        }

        [Test]
        public void Val()
        {
            Assert.That(instrumentMap.Value, Is.EqualTo("val"));
        }

        [Test]
        public void UpdateValue()
        {
            Assert.That(instrumentMap.Value, Is.EqualTo("val"));
            instrumentMap.Value = "newVal";
            Assert.That(instrumentMap.Value, Is.EqualTo("newVal"));
        }
    }
}
