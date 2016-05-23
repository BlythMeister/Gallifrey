using System.Windows;
using Exceptionless;
using Exceptionless.Models;
using Gallifrey.UI.Helpers;
using Gallifrey.UI.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Flyouts
{
    public partial class Error
    {
        private readonly ModelHelpers modelHelpers;
        private ErrorModel DataModel => (ErrorModel)DataContext;

        public Error(ModelHelpers modelHelpers, Event exceptionlessEvent)
        {
            this.modelHelpers = modelHelpers;
            InitializeComponent();
            DataContext = new ErrorModel(exceptionlessEvent);
        }

        private async void SendReport(object sender, RoutedEventArgs e)
        {
            DataModel.SetUserDetailsInEvent();

            ExceptionlessClient.Default.SubmitEvent(DataModel.ExceptionlessEvent);

            await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Thank You", "You're Helping Make Gallifrey Better!");

            modelHelpers.CloseFlyout(this);
        }
    }
}
