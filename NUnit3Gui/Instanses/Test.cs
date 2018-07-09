using NUnit.Framework.Interfaces;
using NUnit3Gui.Enums;
using ReactiveUI;

namespace NUnit3Gui.Instanses
{
    public class Test : ReactiveObject, Interfaces.ITest
    {
        private bool _isRunning;
        private TestState _status = TestState.Unrunned;
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

        public TestState Status
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