using Ninu.ViewModels;
using System.Windows;

namespace Ninu
{
    public partial class App : Application
    {
        private InputManager? _inputManager;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _inputManager = new InputManager();

            var viewModel = new MainWindowViewModel(_inputManager);

            var window = new MainWindow(viewModel);
            window.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _inputManager?.Dispose();
        }
    }
}