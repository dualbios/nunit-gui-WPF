using System.ComponentModel.Composition;
using System.Threading.Tasks;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.FileLoader
{
    [Export(typeof(IFileLoader))]
    public class SolutionLoader : IFileLoader
    {
        public string Extension { get; } = ".sln";

        public Task<IFileItem> LoadAsync(string filePath)
        {
            IFileItem fileItem = new FileItem(filePath);
            fileItem.Tests = new[] { "1111111 1111", "22222" };
            return Task.FromResult(fileItem);
        }
    }
}