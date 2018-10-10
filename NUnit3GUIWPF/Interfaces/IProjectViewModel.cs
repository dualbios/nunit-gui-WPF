using System.Threading.Tasks;

namespace NUnit3GUIWPF.Interfaces
{
    public interface IProjectViewModel
    {
        Task SetProjectFileAsync(string fileName);
    }
}