using System.Collections.Generic;

namespace NUnit3GUIWPF.Interfaces
{
    public interface IPackageSettingsViewModel
    {
        IDictionary<string, object> GetSettings();
    }
}