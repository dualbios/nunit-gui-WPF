using System;
using System.Windows;

namespace NUnit3GUIWPF.Utils
{
    public static class Splasher
    {
        /// <summary>
        /// Get or set the splash screen window
        /// </summary>
        public static Window Splash { get; set; }

        /// <summary>
        /// Close splash screen
        /// </summary>
        public static void CloseSplash()
        {
            if (Splash != null)
            {
                Splash.Close();
                if (Splash is IDisposable)
                    (Splash as IDisposable).Dispose();
            }
        }

        /// <summary>
        /// Show splash screen
        /// </summary>
        public static void ShowSplash()
        {
            if (Splash != null)
            {
                Splash.Show();
            }
        }
    }
}