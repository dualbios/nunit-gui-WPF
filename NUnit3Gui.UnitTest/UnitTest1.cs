using System.Threading.Tasks;
using NUnit.Framework;

namespace NUnit3Gui.UnitTest
{
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public async Task TestMethod1()
        {
            await Task.Delay(1550);
            Assert.True(false);
        }

        [Test]
        public void TestMethod2()
        {
            Assert.True(true);
        }
    }
}