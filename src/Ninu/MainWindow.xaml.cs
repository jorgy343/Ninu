using Ninu.ViewModels;
using System;
using System.Windows;

namespace Ninu
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
            DataContext = ViewModel;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            ViewModel.StopRendering();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            ViewModel.AcquireDevices();
        }
    }
}