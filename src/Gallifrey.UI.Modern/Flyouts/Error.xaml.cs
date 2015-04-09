using System.Windows;
using Exceptionless;
using Exceptionless.Models;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Error
    {
        private readonly MainViewModel viewModel;

        public Error(MainViewModel viewModel, Event exceptionlessEvent)
        {
            this.viewModel = viewModel;
            InitializeComponent();
            DataContext = new ErrorModel(exceptionlessEvent);
        }

        private async void SendReport(object sender, RoutedEventArgs e)
        {
            var model = (ErrorModel) DataContext;

            model.SetUserDetailsInEvent();

            ExceptionlessClient.Default.SubmitEvent(model.ExceptionlessEvent);

            await viewModel.MainWindow.ShowMessageAsync("Thank You", "You're Helping Make Gallifrey Better!");

            IsOpen = false;
        }
    }
}
