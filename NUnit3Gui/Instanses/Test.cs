using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit3Gui.Enums;
using ReactiveUI;

namespace NUnit3Gui.Instanses
{
    public class Test : ReactiveObject, Interfaces.ITest
    {
        private readonly string CategoryAttributeName = typeof(CategoryAttribute).Name;

        private bool _isRunning;
        private bool _isSelected;
        private TimeSpan _ranningTime;
        private TestState _status = TestState.Unrunned;
        private string _stringStatus;

        public Test(string filePath, MethodInfo methodInfo, string testName)
        {
            AssemblyPath = filePath;
            var parts = testName.Split(new[] { '.' });
            if (parts != null)
            {
                if (parts.Length > 2)
                {
                    Namespaces = parts.Take(parts.Length - 1).ToArray();
                    ClassName = parts[parts.Length - 2];
                    TestName = parts[parts.Length - 1];
                }
                else if (parts.Length == 1)
                {
                    TestName = parts[0];
                }
            }

            Categories = methodInfo.GetCustomAttributes(typeof(Attribute))
                .Where(_=>_.GetType().Name == CategoryAttributeName)
                           .OfType<CategoryAttribute>()
                           .Select(_ => _.Name)
                           .ToArray();
        }

        public string AssemblyPath { get; }

        public string[] Categories { get; } = new string[0];

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

        public string[] Namespaces { get; } = new string[0];

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