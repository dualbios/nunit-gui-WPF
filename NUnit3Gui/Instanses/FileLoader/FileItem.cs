using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit3Gui.Interfaces;
using NUnit3Gui.Tools;

namespace NUnit3Gui.Instanses.FileLoader
{
    [Export(typeof(IFileItem))]
    public class FileItem : NotifyPropertyChanged, IFileItem
    {
        private string _message;
        private string _stringState;
        private IEnumerable<ITest> _tests;

        public FileItem(string filePath)
        {
            StringState = string.Empty;
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
        }

        public string FileName { get; }

        public string FilePath { get; }

        public string Message
        {
            get => _message;
            private set => SetProperty(ref _message, value);
        }

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

        public Task LoadAsync(IFileParserManager fileParserManager, CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<bool>();

            Task.Run(async () =>
            {
                StringState = "loading ...";
                await Task.Delay(25);
                Message = null;

                try
                {
                    Tests = await fileParserManager.CurrentFileParser.ParseFileAsync(FilePath, ct);

                    StringState = $"{this.TestCount} classes(s)";
                    tcs.SetResult(true);
                }
                catch (Exception e)
                {
                    StringState = "error loading.";
                    Message = e.Message;
                    tcs.SetResult(false);
                }
            }
                , ct);

            return tcs.Task;
        }
    }
}