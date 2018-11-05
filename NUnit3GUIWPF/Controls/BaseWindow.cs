using System;
using System.Windows;
using System.Windows.Input;

namespace NUnit3GUIWPF.Controls
{
    public class BaseWindow : Window
    {
        private FrameworkElement _maximizeButton;
        private FrameworkElement _minimiseButton;
        private FrameworkElement _restoreButton;
        private FrameworkElement titleBar;

        public BaseWindow()
        {
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand,
                (t, e) => SystemCommands.CloseWindow(this)));

            this.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand,
                (t, e) => SystemCommands.MaximizeWindow(this),
                (s, e) => e.CanExecute = this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip));

            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand,
                (t, e) => SystemCommands.MinimizeWindow(this),
                (s, e) => e.CanExecute = this.ResizeMode != ResizeMode.NoResize));

            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand,
                (t, e) => SystemCommands.RestoreWindow(this),
                (s, e) => e.CanExecute = this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip));

            Loaded += Window_Loaded;
            StateChanged += MainWindow_StateChanged;
        }

        protected void TitlebarRectMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
                WindowState = (WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal);
        }

        protected void TitlebarRectMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        protected void TitlebarRectMouseMove(object sender, System.Windows.Input.MouseEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Pressed)
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                }

                this.DragMove();
            }
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (_restoreButton != null && _maximizeButton != null)
            {
                if (ResizeMode == ResizeMode.NoResize)
                {
                    _maximizeButton.Visibility = Visibility.Collapsed;
                    _minimiseButton.Visibility = Visibility.Collapsed;
                    _restoreButton.Visibility = Visibility.Collapsed;
                }
                else if (WindowState != WindowState.Maximized)
                {
                    _maximizeButton.Visibility = Visibility.Visible;
                    _restoreButton.Visibility = Visibility.Collapsed;
                }
                else if (WindowState == WindowState.Maximized)
                {
                    _maximizeButton.Visibility = Visibility.Collapsed;
                    _restoreButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _restoreButton = this.Template.FindName("RestoreButton", this) as FrameworkElement;
            _maximizeButton = this.Template.FindName("MaximizeButton", this) as FrameworkElement;
            _minimiseButton = this.Template.FindName("MinimizeButton", this) as FrameworkElement;

            MainWindow_StateChanged(this, e);

            titleBar = this.Template.FindName("TitlebarControl", this) as FrameworkElement;
            if (titleBar != null)
            {
                titleBar.PreviewMouseLeftButtonDown += (o, args) => TitlebarRectMouseLeftButtonDown(this, args);
                titleBar.PreviewMouseLeftButtonUp += (s, args) => TitlebarRectMouseLeftButtonUp(this, args);
                titleBar.PreviewMouseMove += (o, args) => TitlebarRectMouseMove(this, args);
            }
        }
    }
}