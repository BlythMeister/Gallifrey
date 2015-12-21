using System.Windows;
using Exceptionless;
using Exceptionless.Models;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class Error
    {
        private readonly MainViewModel viewModel;
        private ErrorModel DataModel => (ErrorModel)DataContext;

        public Error(MainViewModel viewModel, Event exceptionlessEvent)
        {
            this.viewModel = viewModel;
            InitializeComponent();
            DataContext = new ErrorModel(exceptionlessEvent);
        }

        private async void SendReport(object sender, RoutedEventArgs e)
        {
            DataModel.SetUserDetailsInEvent();

            ExceptionlessClient.Default.SubmitEvent(DataModel.ExceptionlessEvent);

            await DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "Thank You", "You're Helping Make Gallifrey Better!");

            IsOpen = false;
        }
    }
}
