using System.Collections.Generic;
using NUnit3Gui.Enums;

namespace NUnit3Gui.Interfaces
{
    public interface ITestTreeManager
    {
        IEnumerable<ITestTreeCollector> TestTreeCollectors { get; }

        ITestTreeCollector GetCollector(TestTreeCollectorType type);

        IEnumerable<TestTreeCollectorType> GetCollectorTypes();
    }
}