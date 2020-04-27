using System.Windows;
using Ninu.ViewModels;

namespace Ninu
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var viewModel = new MainWindowViewModel();

            var window = new MainWindow(viewModel);
            window.Show();
        }
    }
}