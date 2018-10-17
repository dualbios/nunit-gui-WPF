using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Composition;
using NUnit3GUIWPF.Interfaces;

namespace NUnit3GUIWPF.Factories
{
    [Export(typeof(IContainerFactory))]
    public class ContainerFactory : IContainerFactory
    {
        private readonly IExportProvider _provider;

        [ImportingConstructor]
        public ContainerFactory(IExportProvider provider)
        {
            _provider = provider;
        }

        public IProjectViewModel CreateProjectViewModel()
        {
            return _provider.Provider.GetExportedValue<IProjectViewModel>();
        }
    }
}