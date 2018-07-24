using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.FileLoader
{
    [Export(typeof(IFileLoaderManager))]
    public class FileLoaderManager : IFileLoaderManager
    {
        private readonly string currentParserAlias = "NUnitParser";

        public IFileParser CurrentFileParser => FileParsers.First(_ => _.Alias == currentParserAlias);

        [ImportMany]
        public IEnumerable<IFileParser> FileParsers { get; private set; }

        public IEnumerable<IFileItem> LoadFiles(IEnumerable<string> fileNames)
        {
            return fileNames.SelectMany(ParseFile);
        }

        public IEnumerable<IFileItem> ParseFile(string file)
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

        public Task<IEnumerable<ITest>> ParseFileAsync(string fileName, CancellationToken ct)
        {
            return CurrentFileParser.ParseFileAsync(fileName, ct);
        }

        public Task RunTestAsync(ITest test, CancellationToken ct)
        {
            return CurrentFileParser.RunTestAsync(test, ct);
        }
    }
}