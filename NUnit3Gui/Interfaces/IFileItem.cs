using System.Collections.Generic;
using System.Threading.Tasks;

namespace NUnit3Gui.Interfaces
{
    public interface IFileItem
    {
        string FileName { get; }

        string FilePath { get; }

        string StringState { get; }

        int TestCount { get; }

        IEnumerable<string> Tests { get; set; }

        Task LoadAsync();
    }
}