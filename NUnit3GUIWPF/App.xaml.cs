using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using DesktopBridge;
using Microsoft.VisualStudio.Composition;
using NUnit.Engine;
using NUnit3GUIWPF.Interfaces;
using NUnit3GUIWPF.ViewModels;
using NUnit3GUIWPF.Views;

namespace NUnit3GUIWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private async Task<ExportProvider> Compose(object parentInstance)
        {
            ExportProvider exportProvider = null;
            PartDiscovery discovery = PartDiscovery.Combine(
                new AttributedPartDiscovery(Resolver.DefaultInstance),
                new AttributedPartDiscoveryV1(Resolver.DefaultInstance)); // ".NET MEF" attributes (System.ComponentModel.Composition)

            Assembly parentAssembly = parentInstance.GetType().Assembly;
            string parentLocation = parentAssembly.Location;
            string assemblyPath = parentLocation;
            assemblyPath = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));

            Helpers desktopBridgeHelper = new Helpers();
            List<string> assemblies = new[] { parentLocation }
                .Concat(
                    Directory.EnumerateFiles(assemblyPath, "*.dll", SearchOption.TopDirectoryOnly)
                        .Where(_=>_.Contains("NUnit3GUI")))
                .ToList();

            try
            {
                DiscoveredParts discoveredParts = await discovery.CreatePartsAsync(assemblies);
                discoveredParts.ThrowOnErrors();

                ComposableCatalog catalog = ComposableCatalog.Create(Resolver.DefaultInstance)
                    .AddParts(discoveredParts)
                    .WithCompositionService();

                CompositionConfiguration config = CompositionConfiguration.Create(catalog);
                config.ThrowOnErrors();

                IExportProviderFactory epf = config.CreateExportProviderFactory();
                exportProvider = epf.CreateExportProvider();

                ICompositionService service = exportProvider.GetExportedValue<ICompositionService>();
                service.SatisfyImportsOnce(parentInstance);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return null;
            }

            return exportProvider;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            ExportProvider provider = await Compose(this);

            IExportProvider ggg =  provider.GetExportedValue<IExportProvider>();
            PropertyInfo pi = typeof(ExportProviderImp).GetProperty("Provider");
            pi.SetValue(ggg, provider, null);

            IMainWindowViewModel mainWindowViewModel =  provider.GetExportedValue<IMainWindowViewModel>();

            this.MainWindow = new MainWindow() { DataContext = mainWindowViewModel };
            this.MainWindow.Show();
        }

        [Export(typeof(IExportProvider))]
        sealed public class ExportProviderImp : IExportProvider
        {
            public ExportProvider Provider { get; private set; }
        }
    }
}