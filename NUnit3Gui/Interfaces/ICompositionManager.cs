using System.Threading.Tasks;
using Microsoft.VisualStudio.Composition;

namespace NUnit3Gui.Interfaces
{
    public interface ICompositionManager
    {
        ExportProvider ExportProvider { get; }

        Task<bool> Compose(object parentInstance);
    }
}