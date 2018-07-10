﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit3Gui.Convertres;
using NUnit3Gui.Extensions;
using NUnit3Gui.Interfaces;
using NUnit3Gui.Views;
using ReactiveUI;

namespace NUnit3Gui.ViewModels
{
    [Export(typeof(IProjectViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [TypeConverter(typeof(ViewModelToViewConverter<ProjectViewModel, ProjectView>))]
    public class ProjectViewModel : ReactiveObject, IProjectViewModel
    {
        private readonly IFileLoaderManager _fileLoaderManager;
        private readonly string[] propertiesToRefresh = { nameof(AssembliesCount) };
        private readonly IObservable<bool> selectedAssembly;
        private int _loadingProgress;
        private IFileItem _selectedAssembly;

        [ImportingConstructor]
        public ProjectViewModel(IFileLoaderManager fileLoaderManager, ITestsViewModel testsViewModel)
        {
            _fileLoaderManager = fileLoaderManager;
            TestsViewModel = testsViewModel;

            LoadedAssemblies = new ReactiveList<IFileItem>() { ChangeTrackingEnabled = true };

            selectedAssembly = this.WhenAny(vm => vm.SelectedAssembly, p => p.Value != null);

            TestsViewModel.Tests.Changed.Subscribe(x => this.PropertiesChanged(propertiesToRefresh));

            LoadedAssemblies.ItemsRemoved
                .Subscribe(x => TestsViewModel.Tests.RemoveAll(x.Tests));

            LoadedAssemblies.Changed
                .Where(_ => _.Action == NotifyCollectionChangedAction.Reset)
                .Subscribe(x => TestsViewModel.Tests.Clear());

            LoadedAssemblies.Changed
                .Subscribe(x => this.PropertiesChanged(propertiesToRefresh));

            LoadedAssemblies.ItemChanged
                .Subscribe(x => this.PropertiesChanged(propertiesToRefresh));

            BrowseAssembliesCommand = ReactiveCommand
                .CreateFromObservable(() => Observable
                        .StartAsync(ct => OpenAssemblies(ct))
                        .TakeUntil(CancelBrowseCommand)
                , testsViewModel.IsTestRunningObservable.Invert());

            CancelBrowseCommand = ReactiveCommand.Create(() => { }, BrowseAssembliesCommand.IsExecuting);

            RemoveAssembliesCommand = ReactiveCommand.CreateFromTask(() =>
                {
                    if (SelectedAssembly != null)
                    {
                        LoadedAssemblies.Remove(SelectedAssembly);
                    }
                    return Task.FromResult(default(Unit));
                }
                , Observable.CombineLatest(
                    BrowseAssembliesCommand.IsExecuting.Invert()
                    , selectedAssembly
                    , TestsViewModel.IsTestRunningObservable.Invert()
                    , ResultSelector.And3Result));

            RemoveAllAssembliesCommand = ReactiveCommand.CreateFromTask(() =>
                {
                    LoadedAssemblies.Clear();
                    return Task.FromResult(default(Unit));
                }
                , Observable.CombineLatest(
                    TestsViewModel.HasTests
                    , BrowseAssembliesCommand.IsExecuting.Invert()
                    , TestsViewModel.IsTestRunningObservable.Invert()
                    , ResultSelector.And3Result));
        }

        public int AssembliesCount => LoadedAssemblies.Count();

        public ReactiveCommand<Unit, Unit> BrowseAssembliesCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelBrowseCommand { get; }

        public IReactiveList<IFileItem> LoadedAssemblies { get; }

        public int LoadingProgress
        {
            get => _loadingProgress;
            private set
            {
                _loadingProgress = value;
                IReactiveObjectExtensions.RaisePropertyChanged(this);
            }
        }

        public ReactiveCommand<Unit, Unit> RemoveAllAssembliesCommand { get; }

        public ReactiveCommand<Unit, Unit> RemoveAssembliesCommand { get; }

        public IFileItem SelectedAssembly
        {
            get => _selectedAssembly;
            set => this.RaiseAndSetIfChanged(ref _selectedAssembly, value);
        }

        public int TestCount { get; }

        public ITestsViewModel TestsViewModel { get; }

        private async Task<Unit> OpenAssemblies(CancellationToken ct)
        {
            void CancelationOpenAssemblies(IEnumerable<IFileItem> addedFiles)
            {
                using (LoadedAssemblies.SuppressChangeNotifications())
                {
                    foreach (IFileItem file in addedFiles)
                    {
                        LoadedAssemblies.Remove(file);
                    }
                }

                LoadingProgress = 100;
            }

            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Dll files|*.dll", Multiselect = true, FileName = @"*.test*.dll" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                LoadingProgress = 0;
                if (ofd.FileNames.Length > 0)
                {
                    IEnumerable<IFileItem> addedFiles = _fileLoaderManager.LoadFiles(ofd.FileNames).ToList();
                    foreach (IFileItem fileItem in addedFiles)
                    {
                        LoadedAssemblies.Add(fileItem);
                    }

                    if (ct.IsCancellationRequested)
                    {
                        CancelationOpenAssemblies(addedFiles);
                        return default(Unit);
                    }

                    int index = 1;
                    foreach (IFileItem item in addedFiles)
                    {
                        await item.LoadAsync();
                        foreach (ITest test in item.Tests)
                        {
                            TestsViewModel.Tests.Add(test);
                        }

                        LoadingProgress = (int)(((double)index) / ((double)ofd.FileNames.Length) * 100D);
                        await Task.Delay(25);
                        index++;

                        if (ct.IsCancellationRequested)
                        {
                            CancelationOpenAssemblies(addedFiles);
                            return default(Unit);
                        }
                    }
                    LoadingProgress = 100;
                }
            }

            return default(Unit);
        }
    }
}