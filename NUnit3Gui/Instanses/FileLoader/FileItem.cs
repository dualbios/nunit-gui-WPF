using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit3Gui.Interfaces;
using NUnit3Gui.Tools;

namespace NUnit3Gui.Instanses.FileLoader
{
    [Export(typeof(IFileItem))]
    public class FileItem : NotifyPropertyChanged, IFileItem
    {
        private readonly string TestFixtureAttributeName = typeof(TestFixtureAttribute).Name;
        private readonly string TestAttributeName = typeof(TestAttribute).Name;
        private string _stringState;
        private string _message;
        private IEnumerable<ITest> _tests;

        public FileItem(string filePath)
        {
            StringState = string.Empty;
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
        }

        public string FileName { get; }

        public string FilePath { get; }

        public string StringState
        {
            get => _stringState;
            private set => SetProperty(ref _stringState, value);
        }

        public int TestCount => Tests?.Count() ?? 0;

        public IEnumerable<ITest> Tests
        {
            get => _tests;
            private set
            {
                SetProperty(ref _tests, value);
                OnPropertyChanged(nameof(TestCount));
            }
        }

        public string Message
        {
            get => _message;
            private set => SetProperty(ref _message, value);
        }

        public Task LoadAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            Task.Run(async () =>
            {
                StringState = "loading ...";
                await Task.Delay(25);
                Message = null;

                try
                {
                    Assembly assembly = Assembly.LoadFrom(FilePath);

                    Tests = assembly.GetTypes()
                        .Where(type =>
                            type.GetCustomAttributes(typeof(Attribute), true).Any(_ =>
                                _.GetType().Name == TestFixtureAttributeName))
                        .SelectMany(_ => _.GetMethods())
                        .Where(m => m.GetCustomAttributes(typeof(Attribute), true)
                            .Any(_ => _.GetType().Name == TestAttributeName))
                        .Select(methodInfo => methodInfo.DeclaringType.FullName + "." + methodInfo.Name)
                        .Select(test => new Test(FilePath, test))
                        .ToList();

                    StringState = $"{this.TestCount} classes(s)";
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    StringState = "error loading.";
                    Message = e.Message;
                    tcs.SetResult(false);
                }

            });

            return tcs.Task;
        }
    }
}