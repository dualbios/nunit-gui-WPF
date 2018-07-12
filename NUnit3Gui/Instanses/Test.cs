using System;
using NUnit3Gui.Enums;
using ReactiveUI;

namespace NUnit3Gui.Instanses
{
    public class Test : ReactiveObject, Interfaces.ITest
    {
        private bool _isRunning;
        private bool _isSelected;
        private TimeSpan _ranningTime;
        private TestState _status = TestState.Unrunned;
        private string _stringStatus;

        public Test(string filePath, string testName)
        {
            AssemblyPath = filePath;
            var parts = testName.Split(new[] { '.' });
            if (parts != null)
            {
                if (parts.Length > 2)
                {
                    ClassName = parts[parts.Length - 2];
                    TestName = parts[parts.Length - 1];
                }
                else if (parts.Length == 1)
                {
                    TestName = parts[0];
                }
            }
        }

        public string AssemblyPath { get; }

        public string ClassName { get; }

        public bool IsRunning
        {
            get => _isRunning;
            set => this.RaiseAndSetIfChanged(ref _isRunning, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        public TimeSpan RunningTime
        {
            get => _ranningTime;
            set => this.RaiseAndSetIfChanged(ref _ranningTime, value);
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