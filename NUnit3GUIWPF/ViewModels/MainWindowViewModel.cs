using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly IContainerFactory _containerFactory;
        private IContainerViewModel _currentViewModel;

        [ImportingConstructor]
        public MainWindowViewModel(IContainerFactory containerFactory)
        {
            _containerFactory = containerFactory;

            Projects = new ObservableCollection<IContainerViewModel>();
            AddProjectCommand = ReactiveCommand.Create(() =>
            {
                var viewModel = containerFactory.CreateProjectViewModel();
                Projects.Add(viewModel);
                CurrentViewModel = viewModel;
            });
        }

        public ReactiveCommand<Unit, Unit> AddProjectCommand { get; }

        public IContainerViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }

        public IList<IContainerViewModel> Projects { get; }
    }
}