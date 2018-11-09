using System.Threading.Tasks;
using NUnit.Framework;

namespace Test4Test
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
        public async Task TestAsync1()
        {
            await Task.Delay(4321);
            Assert.True(true);
        }

        [Test]
        public async Task TestMethodAsync2()
        {
            await Task.Delay(3321);
            Assert.True(true);
        }

        [Test]
        [Ignore("test reason")]
        public void TestMethod2()
        {
            Assert.True(false);
        }
    }
}