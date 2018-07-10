using System.ComponentModel;
using System.ComponentModel.Composition;
using NUnit3Gui.Convertres;
using NUnit3Gui.Interfaces;
using NUnit3Gui.Views;

namespace NUnit3Gui.ViewModels
{
    [Export(typeof(IMainViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [TypeConverter(typeof(ViewModelToViewConverter<MainViewModel, MainWindow>))]
    public class MainViewModel : IMainViewModel
    {
        [ImportingConstructor]
        public MainViewModel(IProjectViewModel projectViewModel)
        {
            ProjectViewModel = projectViewModel;
        }

        public IProjectViewModel ProjectViewModel { get; }

        public ITestsViewModel TestsViewModel => ProjectViewModel?.TestsViewModel;
    }
}