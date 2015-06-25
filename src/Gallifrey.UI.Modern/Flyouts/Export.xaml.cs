using System;
using System.Windows;
using Gallifrey.UI.Modern.Models;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class Export
    {
        private readonly MainViewModel viewModel;

        public Export(MainViewModel viewModel, Guid timerId, TimeSpan? exportTime)
        {
            this.viewModel = viewModel;
            InitializeComponent();
        }

        private void ExportButton(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
