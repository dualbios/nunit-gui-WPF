using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Microsoft.VisualStudio.Composition;
using NUnit3GUIWPF.Interfaces;
using NUnit3GUIWPF.Views;
using ReactiveUI;

namespace NUnit3GUIWPF.ViewModels
{
    [Export(typeof(IMainWindowViewModel))]
    public class MainWindowViewModel : ReactiveObject, IMainWindowViewModel
    {
        private const string NewProjectName = "Project";
        private const string projectNameFormat = "{0}({1})";
        private readonly IContainerFactory _containerFactory;
        private IProjectViewModel _currentViewModel;

        [ImportingConstructor]
        public MainWindowViewModel(IContainerFactory containerFactory)
        {
            _containerFactory = containerFactory;

            Projects = new ObservableCollection<IProjectViewModel>();
            AddProjectCommand = ReactiveCommand.Create(() => InternalAddNewProject());

            CloseProjectCommand = ReactiveCommand.Create<IProjectViewModel, Unit>(
                p => CloseProject(p));

            InternalAddNewProject();
        }

        public ReactiveCommand<Unit, Unit> AddProjectCommand { get; }

        public ReactiveCommand<IProjectViewModel, Unit> CloseProjectCommand { get; }

        public IProjectViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }

        public IList<IProjectViewModel> Projects { get; }

        public string CreateUniqueName(string name, IEnumerable<string> list)
        {
            int index = 1;
            while (list.Contains(string.Format(projectNameFormat, name, index)))
            {
                index++;
            }

            return string.Format(projectNameFormat, name, index);
        }

        private Unit CloseProject(IProjectViewModel project)
        {
            if (project != null)
            {
                int index = Projects.IndexOf(project);
                Projects.Remove(project);

                if (CurrentViewModel == null)
                    CurrentViewModel = Projects.FirstOrDefault();
            }

            return Unit.Default;
        }

        private void InternalAddNewProject()
        {
            var viewModel = _containerFactory.CreateProjectViewModel();
            viewModel.Header = Projects.Any(_ => _.Header == NewProjectName) == false ? NewProjectName : CreateUniqueName(NewProjectName, Projects.Select(_ => _.Header).ToList());
            Projects.Add(viewModel);
            CurrentViewModel = viewModel;
        }

        public void ShowAbout()
        {
            
        }
    }
}