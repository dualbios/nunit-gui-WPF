using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework.Interfaces;
using NUnit3Gui.Instanses;
using NUnit3Gui.Interfaces;
using ReactiveUI;
using ITest = NUnit3Gui.Interfaces.ITest;

namespace NUnit3Gui
{
    public class MainWindowViewModel : ReactiveObject
    {
        private int _loadingProgress;
        private IFileItem _selectedAssembly;
        private ITest _selectedTest;

        private ObservableAsPropertyHelper<bool> isTestRunning;

        public MainWindowViewModel()
        {
            LoadedAssemblies = new ReactiveList<IFileItem>() { ChangeTrackingEnabled = true };

            BrowseAssembliesCommand = ReactiveCommand
                .CreateFromObservable(() => Observable
                    .StartAsync(ct => OpenAssemblies(ct))
                    .TakeUntil(this.CancelBrowseCommand));

            RemoveAssembliesCommand = ReactiveCommand.CreateFromTask(
                () => RemoveSelecteddAssemblies(),
                     Observable.Merge(
                        BrowseAssembliesCommand.IsExecuting.Select(_ => !_)
                        , (this).WhenAny(vm => vm.SelectedAssembly, p => p.Value != null)
                      )
                    );

            LoadedAssemblies.ItemChanged
                .Subscribe(x => this.RaisePropertyChanged(nameof(Tests)));

            CancelBrowseCommand = ReactiveCommand.Create(() => { }, BrowseAssembliesCommand.IsExecuting);
            IObservable<bool> hasTests = this.WhenAny(vm => vm.Tests, p => p.Value != null && p.Value.Any());

            RunSelectedTestCommand = ReactiveCommand.CreateFromTask(
                () => RunSelectedTestCommandExecute(),
                Observable.Merge(
                    this.WhenAny(vm => vm.SelectedTest, p => p.Value != null)
                    , hasTests)
                );

            RunAllTestCommand = ReactiveCommand.CreateFromTask(
                () => RunAllTestCommandExecute(),
                Observable.Merge(
                    this.WhenAny(vm => vm.SelectedTest, p => p.Value != null)
                    , RunSelectedTestCommand.IsExecuting.Select(_ => !_)
                    , hasTests)
                );

            isTestRunning = RunSelectedTestCommand.IsExecuting.ToProperty(this, x => x.IsTestRunning);

            FileLoaderManager = AppRoot.Current.CompositionManager.ExportProvider.GetExportedValue<IFileLoaderManager>();
        }

        public ReactiveCommand<Unit, Unit> BrowseAssembliesCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelBrowseCommand { get; }

        public IFileLoaderManager FileLoaderManager { get; private set; }

        public bool IsTestRunning => isTestRunning.Value;

        public ReactiveList<IFileItem> LoadedAssemblies { get; }

        public int LoadingProgress
        {
            get => _loadingProgress;
            private set
            {
                _loadingProgress = value;
                this.RaisePropertyChanged();
            }
        }

        public ReactiveCommand<Unit, Unit> RemoveAssembliesCommand { get; }

        public ReactiveCommand<Unit, Unit> RunAllTestCommand { get; }

        public ReactiveCommand<Unit, Unit> RunSelectedTestCommand { get; }

        public IFileItem SelectedAssembly
        {
            get => _selectedAssembly;
            set => this.RaiseAndSetIfChanged(ref _selectedAssembly, value);
        }

        public ITest SelectedTest
        {
            get => _selectedTest;
            set => this.RaiseAndSetIfChanged(ref _selectedTest, value);
        }

        public IEnumerable<ITest> Tests
        {
            get => LoadedAssemblies.Where(_ => _.Tests != null).SelectMany(a => a.Tests);
        }

        private async Task<Unit> OpenAssemblies(CancellationToken ct)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Dll files|*.dll", Multiselect = true, FileName = @"C:\Repositories\nunit-gui-WPF\NUnit3Gui\bin\Debug\NUnit3Gui.UnitTest.dll" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadingProgress = 0;
                if (ofd.FileNames.Length > 0)
                {
                    foreach (IFileItem fileItem in FileLoaderManager.LoadFiles(ofd.FileNames))
                    {
                        LoadedAssemblies.Add(fileItem);
                    }

                    if (ct.IsCancellationRequested)
                    {
                        LoadedAssemblies.Clear();
                        return default(Unit);
                    }

                    int index = 0;
                    foreach (IFileItem item in LoadedAssemblies)
                    {
                        await item.LoadAsync();
                        LoadingProgress = (int)(((double)index) / ((double)ofd.FileNames.Length) * 100D);
                        await Task.Delay(25);
                        index++;

                        if (ct.IsCancellationRequested)
                        {
                            LoadedAssemblies.Clear();
                            return default(Unit);
                        }
                    }
                    LoadingProgress = 100;
                }
            }

            return default(Unit);
        }

        private Task<Unit> RemoveSelecteddAssemblies()
        {
            if (SelectedAssembly != null)
            {
                LoadedAssemblies.Remove(SelectedAssembly);
                this.RaisePropertyChanged(nameof(Tests));
            }

            return Task.FromResult(default(Unit));
        }

        private async Task<Unit> RunAllTestCommandExecute()
        {
            foreach (ITest test in Tests)
            {
                await RunTest(test);
            }
            return Unit.Default;
        }

        private Task<Unit> RunSelectedTestCommandExecute()
        {
            return RunTest(SelectedTest);
        }

        private async Task<Unit> RunTest(ITest test)
        {
            try
            {
                test.IsRunning = true;
                test.StringStatus = null;
                await Task.Delay(25);

                var rrr = new RunProcess(test.AssemblyPath, test.TestName);
                var result = await rrr.Run();
                await Task.Delay(25);

                test.StringStatus = rrr.StandardOutput.ToString();
                test.Status = result ? TestStatus.Passed : TestStatus.Failed;
            }
            catch (Exception e)
            {
                test.Status = TestStatus.Failed;
                test.StringStatus = e.Message;
            }
            finally
            {
                test.IsRunning = false;
            }

            await Task.Delay(25);
            return Unit.Default;
        }
    }
}