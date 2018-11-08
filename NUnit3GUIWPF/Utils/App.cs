using System;
using System.Threading;
using System.Windows;
using NUnit3GUIWPF.Views;

namespace NUnit3GUIWPF.Utils
{
    internal class App : Application
    {
        /// <summary>
        ///
        /// </summary>
        [STAThread()]
        private static void Main()
        {
            Splasher.Splash = new SplashScreenWindow();
            Splasher.ShowSplash();

            // Simulate application loading
            for (int i = 0; i < 500; i++)
            {
                MessageListener.Instance.ReceiveMessage
                    (string.Format("Load module {0}", i));
                Thread.Sleep(1);
            }
            var aaa = new NUnit3GUIWPF.App();
            aaa.Run();
        }
    }
}