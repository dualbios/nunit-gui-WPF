using NUnit.Framework;

namespace NUnit3Gui.UnitTest
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void TestMethod1()
        {
            Assert.True(false);
        }

        [Test]
        public void TestMethod2()
        {
            Assert.True(true);
        }
    }
}