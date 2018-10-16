using System.ComponentModel.Composition;
using System.Reactive;
using Microsoft.VisualStudio.Composition;
using NUnit3GUIWPF.Interfaces;
using ReactiveUI;

namespace NUnit3GUIWPF.ViewModels
{
    [Export(typeof(IMainWindowViewModel))]
    public class MainWindowViewModel : ReactiveObject, IMainWindowViewModel
    {
        private readonly ExportProvider _provider;
        private string _fileName;
        private ObservableAsPropertyHelper<bool> _isProjectLoaded;
        private ObservableAsPropertyHelper<bool> _isProjectLoading;
        private IProjectViewModel _currentViewModel;

        [ImportingConstructor]
        public MainWindowViewModel(ExportProvider provider)
        {
            _provider = provider;
            OpenFileCommand = ReactiveCommand.Create(OpenFile);
            //, this.WhenAny(vm => vm.FileName, p => !string.IsNullOrEmpty(p.Value)));
            //CancelLoadingProjectCommand = ReactiveCommand.Create(
            //    () => { },
            //    OpenFileCommand.IsExecuting);

            //_isProjectLoaded = OpenFileCommand.ToProperty(this, p => p.IsProjectLoaded);
        }

        public ReactiveCommand<Unit, Unit> CancelLoadingProjectCommand { get; }

        public string FileName
        {
            get => _fileName;
            set => this.RaiseAndSetIfChanged(ref _fileName, value);
        }

        public bool IsProjectLoaded => _isProjectLoaded?.Value ?? false;

        public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }

        [Import]
        public IPackageSettingsViewModel PackageSettingsViewModel { get; private set; }

        [Import]
        public IProjectViewModel ProjectViewModel { get; private set; }

        public IProjectViewModel CurrentViewModel
        {
            get => _currentViewModel;
            private set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }

        [Import(RequiredCreationPolicy = System.ComponentModel.Composition.CreationPolicy.Shared)]
        public IExportProvider Provider { get; private set; }

        private void OpenFile()
        {
            var rrr = Provider.Provider.GetExportedValue<IProjectViewModel>();
            CurrentViewModel = rrr;
            rrr.SetProjectFileAsync(FileName, PackageSettingsViewModel.GetSettings());
        }
    }
}