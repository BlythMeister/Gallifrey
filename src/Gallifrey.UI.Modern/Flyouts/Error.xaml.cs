using Exceptionless;
using Exceptionless.Models;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using System.Windows;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class Error
    {
        private readonly ModelHelpers modelHelpers;
        private ErrorModel DataModel => (ErrorModel)DataContext;

        public Error(ModelHelpers modelHelpers, Event exceptionlessEvent)
        {
            this.modelHelpers = modelHelpers;
            InitializeComponent();
            var initialEmailAddress = string.Empty;
            if (modelHelpers.Gallifrey.JiraConnection.IsConnected && !string.IsNullOrWhiteSpace(modelHelpers.Gallifrey.JiraConnection.CurrentUser.emailAddress))
            {
                initialEmailAddress = modelHelpers.Gallifrey.JiraConnection.CurrentUser.emailAddress;
            }
            DataContext = new ErrorModel(exceptionlessEvent, initialEmailAddress);
        }

        private async void SendReport(object sender, RoutedEventArgs e)
        {
            DataModel.SetUserDetailsInEvent();

            ExceptionlessClient.Default.SubmitEvent(DataModel.ExceptionlessEvent);

            await modelHelpers.ShowMessageAsync("Thank You", "You're Helping Make Gallifrey Better!");

            modelHelpers.CloseFlyout(this);
        }
    }
}
