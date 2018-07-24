using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using NSubstitute;
using NUnit.Framework;
using NUnit3Gui.Interfaces;
using NUnit3Gui.ViewModels;

namespace NUnit3Gui.UnitTest
{
    [TestFixture]
    public class ProjectViewModelTests
    {
        [Test]
        public void BrowseAssembliesCommand_Call_FillIn_LoadedAssemblies()
        {
            //Arrange
            IFileLoaderManager fileLoaderManager = Substitute.For<IFileLoaderManager>();
            IOpenFileDialog openFileDialog = Substitute.For<IOpenFileDialog>();

            openFileDialog.ShowDialog().ReturnsForAnyArgs(DialogResult.OK);
            openFileDialog.FileNames.ReturnsForAnyArgs(new string[] { "file1.dll", "file2.dll" });

            IFileItem file1 = Substitute.For<IFileItem>();
            file1.FilePath.Returns("file1.dll");
            file1.FileName.Returns("file1.dll");

            IFileItem file2 = Substitute.For<IFileItem>();
            file2.FilePath.Returns("file2.dll");
            file2.FileName.Returns("file2.dll");

            fileLoaderManager.LoadFiles(Arg.Any<IEnumerable<string>>()).ReturnsForAnyArgs(new[] { file1, file2 });

            var projectViewModel = new ProjectViewModel(fileLoaderManager, openFileDialog);
            projectViewModel.IsTestRunningObservable = Observable.Empty(true);

            //Act
            projectViewModel.BrowseAssembliesCommand.Execute();

            //Assert
            Assert.True(projectViewModel.LoadedAssemblies.Count == 2);
            Assert.That(projectViewModel.LoadedAssemblies.Select(_ => _.FileName), Is.EquivalentTo(new[] { "file1.dll", "file2.dll" }));
        }

        [Test]
        public void BrowseAssembliesCommand_Calls_FileItem_Received_LoadAsync()
        {
            //Arrange
            IFileLoaderManager fileLoaderManager = Substitute.For<IFileLoaderManager>();
            IOpenFileDialog openFileDialog = Substitute.For<IOpenFileDialog>();

            openFileDialog.ShowDialog().ReturnsForAnyArgs(DialogResult.OK);
            openFileDialog.FileNames.ReturnsForAnyArgs(new string[] { "file1.dll" });

            IFileItem file1 = Substitute.For<IFileItem>();
            file1.FilePath.Returns("file1.dll");
            file1.FileName.Returns("file1.dll");

            fileLoaderManager.LoadFiles(Arg.Any<IEnumerable<string>>()).ReturnsForAnyArgs(new[] { file1 });

            var projectViewModel = new ProjectViewModel(fileLoaderManager, openFileDialog);
            projectViewModel.IsTestRunningObservable = Observable.Empty(true);

            //Act
            projectViewModel.BrowseAssembliesCommand.Execute();

            //Assert
            file1.Received().LoadAsync(fileLoaderManager, CancellationToken.None);
        }

        [Test]
        public void BrowseAssembliesCommand_Calls_FillIn_Tests()
        {
            //Arrange
            IFileLoaderManager fileLoaderManager = Substitute.For<IFileLoaderManager>();
            IOpenFileDialog openFileDialog = Substitute.For<IOpenFileDialog>();

            openFileDialog.ShowDialog().ReturnsForAnyArgs(DialogResult.OK);
            openFileDialog.FileNames.ReturnsForAnyArgs(new string[] { "file1.dll" });

            ITest test1 = Substitute.For<ITest>();
            ITest test2 = Substitute.For<ITest>();

            IFileItem file1 = Substitute.For<IFileItem>();
            file1.FilePath.Returns("file1.dll");
            file1.FileName.Returns("file1.dll");
            file1.Tests.ReturnsForAnyArgs(new[] { test1, test2 });

            fileLoaderManager.LoadFiles(Arg.Any<IEnumerable<string>>()).ReturnsForAnyArgs(new[] { file1 });

            var projectViewModel = new ProjectViewModel(fileLoaderManager, openFileDialog);
            projectViewModel.IsTestRunningObservable = Observable.Empty(true);

            //Act
            projectViewModel.BrowseAssembliesCommand.Execute();

            //Assert
            Assert.That(projectViewModel.Tests, Is.EquivalentTo(new[] { test1, test2 }));
        }

        [Test]
        public void RemoveAllAssembliesCommand_Calls_Clear_AssemblyList()
        {
            //Arrange
            IFileLoaderManager fileLoaderManager = Substitute.For<IFileLoaderManager>();
            IOpenFileDialog openFileDialog = Substitute.For<IOpenFileDialog>();

            var projectViewModel = new ProjectViewModel(fileLoaderManager, openFileDialog);

            IFileItem file1 = Substitute.For<IFileItem>();
            file1.FilePath.Returns("file1.dll");
            file1.FileName.Returns("file1.dll");

            IFileItem file2 = Substitute.For<IFileItem>();
            file2.FilePath.Returns("file2.dll");
            file2.FileName.Returns("file2.dll");

            projectViewModel.LoadedAssemblies.Add(file1);
            projectViewModel.LoadedAssemblies.Add(file2);

            //Act
            projectViewModel.RemoveAllAssembliesCommand.Execute();

            //Assert

            Assert.That(projectViewModel.LoadedAssemblies, Is.Empty);
        }

        [Test]
        public void RemoveAssembliesCommand_Calls_Remove_Assembly()
        {
            //Arrange
            IFileLoaderManager fileLoaderManager = Substitute.For<IFileLoaderManager>();
            IOpenFileDialog openFileDialog = Substitute.For<IOpenFileDialog>();

            var projectViewModel = new ProjectViewModel(fileLoaderManager, openFileDialog);

            IFileItem file1 = Substitute.For<IFileItem>();
            file1.FilePath.Returns("file1.dll");
            file1.FileName.Returns("file1.dll");

            IFileItem file2 = Substitute.For<IFileItem>();
            file2.FilePath.Returns("file2.dll");
            file2.FileName.Returns("file2.dll");

            projectViewModel.LoadedAssemblies.Add(file1);
            projectViewModel.LoadedAssemblies.Add(file2);

            projectViewModel.SelectedAssembly = file2;

            //Act
            projectViewModel.RemoveAssembliesCommand.Execute();

            //Assert

            Assert.That(projectViewModel.LoadedAssemblies, Is.EquivalentTo(new[] { file1 }));
        }

        [Test]
        public void Unset_SelectedAssembly_Disable_RemoveAssembliesCommand()
        {
            //Arrange
            IFileLoaderManager fileLoaderManager = Substitute.For<IFileLoaderManager>();
            IOpenFileDialog openFileDialog = Substitute.For<IOpenFileDialog>();

            var projectViewModel = new ProjectViewModel(fileLoaderManager, openFileDialog);

            IFileItem file1 = Substitute.For<IFileItem>();
            file1.FilePath.Returns("file1.dll");
            file1.FileName.Returns("file1.dll");

            IFileItem file2 = Substitute.For<IFileItem>();
            file2.FilePath.Returns("file2.dll");
            file2.FileName.Returns("file2.dll");

            projectViewModel.LoadedAssemblies.Add(file1);
            projectViewModel.LoadedAssemblies.Add(file2);

            //Act
            projectViewModel.SelectedAssembly = null;

            //Assert
            Assert.IsFalse((projectViewModel.RemoveAssembliesCommand as ICommand).CanExecute(null));
        }

        [Test]
        [Ignore("Command doe not received signal then SelectedAssembly changed")]
        public void Set_SelectedAssembly_Enable_RemoveAssembliesCommand()
        {
            //Arrange
            IFileLoaderManager fileLoaderManager = Substitute.For<IFileLoaderManager>();
            IOpenFileDialog openFileDialog = Substitute.For<IOpenFileDialog>();

            var projectViewModel = new ProjectViewModel(fileLoaderManager, openFileDialog);

            IFileItem file1 = Substitute.For<IFileItem>();
            file1.FilePath.Returns("file1.dll");
            file1.FileName.Returns("file1.dll");

            IFileItem file2 = Substitute.For<IFileItem>();
            file2.FilePath.Returns("file2.dll");
            file2.FileName.Returns("file2.dll");

            projectViewModel.LoadedAssemblies.Add(file1);
            projectViewModel.LoadedAssemblies.Add(file2);

            //Act
            projectViewModel.SelectedAssembly = file2;

            //Assert
            Assert.IsTrue((projectViewModel.RemoveAssembliesCommand as ICommand).CanExecute(null));
        }
    }
}