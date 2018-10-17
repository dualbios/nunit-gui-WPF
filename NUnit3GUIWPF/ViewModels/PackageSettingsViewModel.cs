using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Composition;
using NUnit;
using NUnit.Engine;
using NUnit3GUIWPF.Converters;
using NUnit3GUIWPF.Interfaces;
using NUnit3GUIWPF.Models;
using NUnit3GUIWPF.Views;
using ReactiveUI;

namespace NUnit3GUIWPF.ViewModels
{
    [Export(typeof(IPackageSettingsViewModel))]
    [PartCreationPolicy(System.ComponentModel.Composition.CreationPolicy.NonShared)]
    [TypeConverter(typeof(ViewModelToViewConverter<PackageSettingsViewModel, PackageSettingsView>))]
    public class PackageSettingsViewModel : ReactiveObject, IPackageSettingsViewModel
    {
        [ImportingConstructor]
        public PackageSettingsViewModel(IUnitTestEngine engine)
        {
            Runtimes = new[] {new DefaultRuntimeFramework()}
                .Concat(engine.TestEngine.Services.GetService<IAvailableRuntimes>().AvailableRuntimes)
                .ToList();
        }

        public IEnumerable<string> DomainUsages => new List<string>()
        {
            "Default",
            "Single",
            "Multiple",
        };

        public PackageSettings PackageSettngs { get; } = new PackageSettings();

        public IEnumerable<string> ProcessModels => new List<string>()
        {
            "Default",
            "InProcess",
            "Single",
            "Multiple"
        };

        public IEnumerable<IRuntimeFramework> Runtimes { get; }

        public IDictionary<string, object> GetSettings()
        {
            IDictionary<string, object> settings = new Dictionary<string, object>();
            if (PackageSettngs.IsRunAsX86)
            {
                settings.Add(EnginePackageSettings.RunAsX86, true);
            }

            settings.Add(EnginePackageSettings.ProcessModel, PackageSettngs.ProcessModel);
            settings.Add(EnginePackageSettings.DomainUsage, PackageSettngs.DomainUsage);
            settings.Add(EnginePackageSettings.RuntimeFramework, PackageSettngs.RuntimeFramework);

            return settings;
        }
    }
}