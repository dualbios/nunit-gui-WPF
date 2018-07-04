using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.FileLoader
{
    [Export(typeof(IFileItem))]
    public class FileItem : IFileItem
    {
        public FileItem(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; }

        public int TestCount => Tests?.Count() ?? 0;

        public IEnumerable<string> Tests { get; set; }
    }
}