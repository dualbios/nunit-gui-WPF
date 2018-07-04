using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using NUnit3Gui.Interfaces;
using NUnit3Gui.Tools;

namespace NUnit3Gui.Instanses.FileLoader
{
    [Export(typeof(IFileItem))]
    public class FileItem : NotifyPropertyChanged, IFileItem
    {
        private string _stringState;
        private IEnumerable<string> _tests;

        public FileItem(string filePath)
        {
            StringState = string.Empty;
            FilePath = filePath;
        }

        public string FilePath { get; }

        public string StringState
        {
            get => _stringState;
            set => SetProperty(ref _stringState, value);
        }

        public int TestCount => Tests?.Count() ?? 0;

        public IEnumerable<string> Tests
        {
            get => _tests;
            set
            {
                SetProperty(ref _tests, value);
                OnPropertyChanged(nameof(TestCount));
            }
        }
    }
}