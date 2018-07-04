using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.FileLoader
{
    [Export(typeof(IFileLoader))]
    public class SolutionLoader : IFileLoader
    {
        public string Extension { get; } = ".sln";

        public Task<IEnumerable<IFileItem>> LoadAsync(string filePath)
        {
            IEnumerable<IFileItem> fileItem = new[]
            {
                new FileItem(filePath) { Tests = new[] { "1111111 1111", "22222" } },
                new FileItem(filePath) { Tests = new[] { "44", "55 555" } },
            };
            return Task.FromResult(fileItem);
        }
    }
}