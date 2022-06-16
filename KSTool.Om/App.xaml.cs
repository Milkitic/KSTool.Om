using System.Windows;
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
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            KeyboardHook.Dispose();
        }
    }
}
