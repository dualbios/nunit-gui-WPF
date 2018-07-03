using System;
using System.Reactive;
using NUnit3Gui.Tools;
using ReactiveUI;

namespace NUnit3Gui
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly ReactiveCommand<Unit, Unit> browseAssembliesCommand;
        public ReactiveCommand<Unit, Unit> BrowseAssembliesCommand => this.browseAssembliesCommand;

        public MainWindowViewModel()
        {
            browseAssembliesCommand = ReactiveCommand.Create(() => LoadTweets());
            LoadedAssemblies = new ReactiveList<string>();
        }

        private void LoadTweets()
        {
            LoadedAssemblies.Add("111");
        }

        public ReactiveList<string> LoadedAssemblies { get; } 
    }
}