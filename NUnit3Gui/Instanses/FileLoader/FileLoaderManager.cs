using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.FileLoader
{
    [Export(typeof(IFileLoaderManager))]
    public class FileLoaderManager : IFileLoaderManager
    {
        public IEnumerable<IFileItem> LoadFiles(IEnumerable<string> fileNames)
        {
            return fileNames.SelectMany(ParseFile);
        }

        private IEnumerable<IFileItem> ParseFile(string file)
        {
            string fileExtention = Path.GetExtension(file);
            if (".dll".Equals(fileExtention, StringComparison.InvariantCultureIgnoreCase))
            {
                return new[] { new FileItem(file) };
            }
            else if (".sln".Equals(fileExtention, StringComparison.InvariantCultureIgnoreCase))
            {
                return new[] { new FileItem("111"), new FileItem("222") };
            }
            return Enumerable.Empty<IFileItem>();
        }
    }
}