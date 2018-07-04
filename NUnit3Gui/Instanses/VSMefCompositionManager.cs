using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Composition;
using NUnit3Gui.Interfaces;

namespace NUnit3Gui.Instanses
{
    public class VSMefCompositionManager : ICompositionManager
    {
        public ExportProvider ExportProvider { get; private set; }

        public async Task<bool> Compose(object parentInstance)
        {
            ExportProvider = null;
            
            var discovery = PartDiscovery.Combine(
                new AttributedPartDiscovery(Resolver.DefaultInstance),
                new AttributedPartDiscoveryV1(Resolver.DefaultInstance)); // ".NET MEF" attributes (System.ComponentModel.Composition)

            var parentAssembly = parentInstance.GetType().Assembly;
            var parentLocation = parentAssembly.Location;
            var assemblies = new List<string> { parentLocation };

            var assemblyPath = parentLocation;
            assemblyPath = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));

            foreach (var f in Directory.EnumerateFiles(assemblyPath, "*.dll", SearchOption.TopDirectoryOnly))
            {
                assemblies.Add(f);
            }

            try
            {
                var discoveredParts = await discovery.CreatePartsAsync(assemblies);
                discoveredParts.ThrowOnErrors();

                var catalog = ComposableCatalog.Create(Resolver.DefaultInstance)
                    .AddParts(discoveredParts)
                    .WithCompositionService(); // Makes an ICompositionService export available to MEF parts to import

                var config = CompositionConfiguration.Create(catalog);
                //config.ThrowOnErrors();

                var epf = config.CreateExportProviderFactory();
                ExportProvider = epf.CreateExportProvider();

                var service = ExportProvider.GetExportedValue<System.ComponentModel.Composition.ICompositionService>();
                service.SatisfyImportsOnce(parentInstance);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }
    }
}