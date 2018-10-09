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
using NUnit3GUIWPF.ViewModels;
using NUnit3GUIWPF.Views;

namespace NUnit3GUIWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private async Task<bool> Compose(object parentInstance)
        {
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
                ExportProvider exportProvider = epf.CreateExportProvider();

                ICompositionService service = exportProvider.GetExportedValue<ICompositionService>();
                service.SatisfyImportsOnce(parentInstance);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return false;
            }

            return true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Task.Run(async () => { await Compose(this); });
            var testEngine = TestEngineActivator.CreateInstance(true);

            this.MainWindow = new MainWindow() { DataContext = new MainWindowViewModel(testEngine) };
            this.MainWindow.Show();
        }
    }
}