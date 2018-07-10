using System;
using System.Linq;
using System.Reactive.Linq;

namespace NUnit3Gui.Extensions
{
    public static class ObservableExtensions
    {
        public static IObservable<bool> Invert(this IObservable<bool> source)
        {
            return source.Select(_ => !_);
        }
    }
}