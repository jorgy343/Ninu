// ReSharper disable ShiftExpressionRealShiftCountIsZero

using System;
using Ninu.ViewModels;
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

            ViewModel.StopRenderingThread();
            ViewModel.StopRendering();
        }
    }
}