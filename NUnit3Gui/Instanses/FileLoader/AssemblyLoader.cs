using System.ComponentModel.Composition;
using System.Threading.Tasks;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses.FileLoader
{
    [Export(typeof(IFileLoader))]
    public class AssemblyLoader : IFileLoader
    {
        public string Extension { get; } = ".dll";

        public Task<IFileItem> LoadAsync(string filePath)
        {
            IFileItem fileItem = new FileItem(filePath);
            fileItem.Tests = new[] { "wwww" };
            return Task.FromResult(fileItem);
        }
    }
}