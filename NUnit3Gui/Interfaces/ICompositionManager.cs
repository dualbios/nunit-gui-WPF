using System.Threading.Tasks;

namespace NUnit3Gui.Interfaces
{
    public interface ICompositionManager
    {
        Task<bool> Compose(object parentInstance);
    }
}