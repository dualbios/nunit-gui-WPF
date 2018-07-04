using System.Threading.Tasks;

namespace NUnit3Gui.Interfaces
{
    public interface IFileLoader
    {
        string Extension { get; }

        Task<IFileItem> LoadAsync(string filePath);
    }
}