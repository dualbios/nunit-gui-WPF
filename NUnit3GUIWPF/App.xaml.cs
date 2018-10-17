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
using NUnit3GUIWPF.Interfaces;
using NUnit3GUIWPF.Models;
using NUnit3GUIWPF.Views;

namespace NUnit3GUIWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            ExportProvider provider = null;
            try
            {
                provider = await Compose(this);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Current.Shutdown(-1);
                return;
            }

            IExportProvider providerImp = provider.GetExportedValue<IExportProvider>();
            PropertyInfo pi = typeof(ExportProviderImp).GetProperty("Provider");
            pi.SetValue(providerImp, provider, null);

            IMainWindowViewModel mainWindowViewModel = provider.GetExportedValue<IMainWindowViewModel>();

            this.MainWindow = new MainWindow() { DataContext = mainWindowViewModel };
            this.MainWindow.Show();
        }

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
                        .Where(_ => _.Contains("NUnit3GUI")))
                .ToList();

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

            return exportProvider;
        }
    }
}