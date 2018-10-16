using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NUnit3GUIWPF.Interfaces
{
    public interface IProjectViewModel
    {
        void SetProjectFileAsync(string fileName, IDictionary<string, object> packageSettings);
    }
}