using System.Collections.Generic;
using ReactiveUI;

namespace NUnit3GUIWPF.Models
{
    public class PackageSettings : ReactiveObject
    {
        private string _domainUsage;
        private bool _isRunAsX86;
        private string _runtimeFramework;
        private string _selectedProcessModel;
        private IDictionary<string, object> settings = new Dictionary<string, object>();

        public string DomainUsage
        {
            get => _domainUsage;
            set => this.RaiseAndSetIfChanged(ref _domainUsage, value);
        }

        public bool IsRunAsX86
        {
            get => _isRunAsX86;
            set => this.RaiseAndSetIfChanged(ref _isRunAsX86, value);
        }

        public string ProcessModel
        {
            get => _selectedProcessModel;
            set => this.RaiseAndSetIfChanged(ref _selectedProcessModel, value);
        }

        public string RuntimeFramework
        {
            get => _runtimeFramework;
            set => this.RaiseAndSetIfChanged(ref _runtimeFramework, value);
        }
    }
}