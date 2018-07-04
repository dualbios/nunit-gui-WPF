using System.Collections.Generic;
using System.Threading.Tasks;

namespace NUnit3Gui.Interfaces
{
    public interface IFileLoader
    {
        string Extension { get; }

        Task<IEnumerable<IFileItem>> LoadAsync(string filePath);
    }
}