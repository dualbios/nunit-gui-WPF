using NUnit.Framework.Interfaces;
using ReactiveUI;

namespace NUnit3Gui.Instanses
{
    public class Test : ReactiveObject, Interfaces.ITest
    {
        private bool _isRunning;
        private TestStatus _status = TestStatus.Inconclusive;
        private string _stringStatus;

        public Test(string filePath, string testName)
        {
            AssemblyPath = filePath;
            TestName = testName;
        }

        public string AssemblyPath { get; private set; }

        public bool IsRunning
        {
            get => _isRunning;
            set => this.RaiseAndSetIfChanged(ref _isRunning, value);
        }

        public TestStatus Status
        {
            get => _status;
            set => this.RaiseAndSetIfChanged(ref _status, value);
        }

        public string StringStatus
        {
            get => _stringStatus;
            set => this.RaiseAndSetIfChanged(ref _stringStatus, value);
        }

        public string TestName { get; private set; }
    }
}