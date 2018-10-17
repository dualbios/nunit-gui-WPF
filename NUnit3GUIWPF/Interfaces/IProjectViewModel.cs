using NUnit3GUIWPF.Models;

namespace NUnit3GUIWPF.Interfaces
{
    public interface IProjectViewModel : IContainerViewModel
    {
        string FileName { get; }

        ProjectState State { get; }
    }
}