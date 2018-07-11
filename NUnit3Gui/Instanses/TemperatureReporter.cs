using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnit3Gui.Instanses
{
    public class TemperatureReporter : IObserver<bool>
    {
        private IDisposable unsubscriber;
        private bool first = true;
        private bool last;

        public virtual void Subscribe(IObservable<bool> provider)
        {
            unsubscriber = provider.Subscribe(this);
        }

        public virtual void Unsubscribe()
        {
            unsubscriber.Dispose();
        }

        public virtual void OnCompleted()
        {
            Console.WriteLine("Additional temperature data will not be transmitted.");
        }

        public virtual void OnError(Exception error)
        {
            // Do nothing.
        }

        public virtual void OnNext(bool value)
        {
           
        }
    }
}
