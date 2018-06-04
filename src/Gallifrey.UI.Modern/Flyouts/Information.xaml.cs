using Exceptionless;
using Gallifrey.AppTracking;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class Information
    {
        private readonly ModelHelpers modelHelpers;

        public Information(ModelHelpers modelHelpers)
        {
            this.modelHelpers = modelHelpers;
            InitializeComponent();
            DataContext = new InformationModel(modelHelpers.Gallifrey);
            modelHelpers.Gallifrey.TrackEvent(TrackingType.InformationShown);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private async void ChangeLogButton(object sender, RoutedEventArgs e)
        {
            modelHelpers.HideFlyout(this);
            var changeLog = modelHelpers.Gallifrey.GetChangeLog(XDocument.Parse(Properties.Resources.ChangeLog)).ToList();
            if (changeLog.Any())
            {
                await modelHelpers.OpenFlyout(new ChangeLog(changeLog));
            }
            else
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "No Change Log", "There Is No Change Log To Show");
            }
            await modelHelpers.OpenFlyout(this);
        }

        private async void UserHashClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            try
            {
                await ClipboardHelper.SetClipboard(modelHelpers.Gallifrey.Settings.UserHash);
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Copied Installation Hash", $"Your Installation Hash Of {modelHelpers.Gallifrey.Settings.UserHash} Has Been Copied To The Clipboard");
            }
            catch (Exception ex)
            {
                ExceptionlessClient.Default.SubmitException(ex);
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Error Getting Hash", $"There Was An Error Putting Your Installation Hash Of {modelHelpers.Gallifrey.Settings.UserHash} Onto The Clipboard");
            }
        }
    }
}
