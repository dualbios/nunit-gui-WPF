using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;
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
        public MainViewModel(IProjectViewModel projectViewModel, ITestsViewModel testsViewModel)
        {
            ProjectViewModel = projectViewModel;
            TestsViewModel = testsViewModel;
        }

        public ICommand OpenCommand => ProjectViewModel?.BrowseAssembliesCommand;

        public IProjectViewModel ProjectViewModel { get; }

        public ICommand RunAllTestsCommand => TestsViewModel?.RunAllTestCommand;
        public ICommand CancelRunTestCommand => TestsViewModel?.CancelRunTestCommand;

        public ICommand RunSelectedTestCommand => TestsViewModel?.RunSelectedTestCommand;

        public ITestsViewModel TestsViewModel { get; }
    }
}