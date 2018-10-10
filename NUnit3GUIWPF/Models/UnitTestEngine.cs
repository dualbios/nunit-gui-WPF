using System.Composition;
using NUnit.Engine;
using NUnit3GUIWPF.Interfaces;

namespace NUnit3GUIWPF.Models
{
    [Export(typeof(IUnitTestEngine))]
    public class UnitTestEngine : IUnitTestEngine
    {
        public UnitTestEngine()
        {
            TestEngine = TestEngineActivator.CreateInstance(true);
        }
        public ITestEngine TestEngine { get; }
    }
}