using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using Microsoft.VisualStudio.Composition;
using NUnit3GUIWPF.Interfaces;
using ReactiveUI;

namespace NUnit3GUIWPF.ViewModels
{
    [Export(typeof(IMainWindowViewModel))]
    public class MainWindowViewModel : ReactiveObject, IMainWindowViewModel
    {
        private const string projectNameFormat = "{0}({1})";
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
                viewModel.Header = CreateUniqueName("Project", Projects.Select(_=>_.Header).ToList());
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

        public string CreateUniqueName(string name, IEnumerable<string> list)
        {
            int index = 1;
            while (list.Contains(string.Format(projectNameFormat, name, index)))
            {
                index++;
            }

            return string.Format(projectNameFormat, name, index);
        }
    }
}