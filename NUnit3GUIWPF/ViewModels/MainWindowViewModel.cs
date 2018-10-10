using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit3GUIWPF.Interfaces;
using ReactiveUI;

namespace NUnit3GUIWPF.ViewModels
{
    [Export(typeof(IMainWindowViewModel))]
    public class MainWindowViewModel : ReactiveObject, IMainWindowViewModel
    {
        private string _fileName;
        private ObservableAsPropertyHelper<bool> _isProjectLoaded;
        private ObservableAsPropertyHelper<bool> _isProjectLoading;

        [ImportingConstructor]
        public MainWindowViewModel()
        {
            OpenFileCommand = ReactiveCommand.CreateFromObservable(
                () => Observable.StartAsync(ct => OpenFileAsync(ct))
                    .TakeUntil(CancelLoadingProjectCommand),
                this.WhenAny(vm => vm.FileName, p => !string.IsNullOrEmpty(p.Value)));
            CancelLoadingProjectCommand = ReactiveCommand.Create(
                () => { },
                OpenFileCommand.IsExecuting);

            _isProjectLoaded = OpenFileCommand.ToProperty(this, p => p.IsProjectLoaded);
            _isProjectLoading = OpenFileCommand.IsExecuting.ToProperty(this, p => p.IsProjectLoading);
        }

        public ReactiveCommand<Unit, Unit> CancelLoadingProjectCommand { get; }

        public string FileName
        {
            get => _fileName;
            set => this.RaiseAndSetIfChanged(ref _fileName, value);
        }

        public bool IsProjectLoaded => _isProjectLoaded?.Value ?? false;

        public bool IsProjectLoading => _isProjectLoading?.Value ?? false;

        public ReactiveCommand<Unit, bool> OpenFileCommand { get; }

        [Import]
        public IProjectViewModel ProjectViewModel { get; private set; }

        private async Task<bool> OpenFileAsync(CancellationToken ct)
        {
            try
            {
                await ProjectViewModel.SetProjectFileAsync(FileName, ct);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}