using System.Windows;
using HandyControl.Controls;
using KSTool.Om.Core;
using KSTool.Om.Windows;
using Milki.Extensions.MouseKeyHook;

namespace KSTool.Om
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            KeyboardHook = KeyboardHookFactory.CreateApplication();
            Current = this;
        }

        public IKeyboardHook KeyboardHook { get; }
        public new static App Current { get; private set; } = null!;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += Current_DispatcherUnhandledException;

            _ = AudioManager.Instance.Engine;
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        private static void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Growl.Error($"Unhandled error occurs: \r\n{e.Exception.Message}");
            e.Handled = true;
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            KeyboardHook.Dispose();
        }
    }
}
