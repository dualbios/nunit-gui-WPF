using System.Composition;
using NUnit.Engine;
using NUnit3GUIWPF.Interfaces;

namespace NUnit3GUIWPF.Models
{
    [Export(typeof(IUnitTestEngine))]
    public class UnitTestEngine : IUnitTestEngine
    {
        private static ITestEngine internalITestEngine = null;
        public UnitTestEngine()
        {
            if (internalITestEngine == null) 
                internalITestEngine = TestEngineActivator.CreateInstance(true);

            TestEngine = internalITestEngine;
        }
        public ITestEngine TestEngine { get; }
    }
}