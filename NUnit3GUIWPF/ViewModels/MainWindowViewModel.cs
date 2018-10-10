using System.ComponentModel.Composition;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.Win32;
using NUnit3GUIWPF.Interfaces;
using ReactiveUI;

namespace NUnit3GUIWPF.ViewModels
{
    [Export(typeof(IMainWindowViewModel))]
    public class MainWindowViewModel : ReactiveObject, IMainWindowViewModel
    {
        [ImportingConstructor]
        public MainWindowViewModel()
        {
            OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFileAsync);
        }

        public ReactiveCommand<Unit, string> OpenFileCommand { get; }

        [Import]
        public IProjectViewModel ProjectViewModel { get; private set; }

        private Task<string> OpenFileAsync()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

            Task.Run(async () =>
            {
                OpenFileDialog ofd = new OpenFileDialog()
                {
                    CheckFileExists = true,
                    DefaultExt = "*.dll"
                };
                string file = (ofd.ShowDialog() == true) ? ofd.FileName : null;
                if (file != null)
                {
                    await ProjectViewModel.SetProjectFileAsync(file);
                }

                tcs.SetResult(file);
            });

            return tcs.Task;
        }
    }
}