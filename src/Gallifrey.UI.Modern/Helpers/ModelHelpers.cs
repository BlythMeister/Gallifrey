using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Helpers
{
    public class ModelHelpers
    {
        private readonly FlyoutsControl flyoutsControl;
        private readonly List<OpenFlyoutDetails> openFlyouts;
        public IBackend Gallifrey { get; }
        public DialogContext DialogContext { get; }
        public bool FlyoutOpen => openFlyouts.Count(x=>!x.IsHidden) > 0;

        public event EventHandler<Guid> SelectTimerEvent;
        public event EventHandler SelectRunningTimerEvent;
        public event EventHandler RefreshModelEvent;
        public event EventHandler<RemoteButtonTrigger> RemoteButtonTrigger;
        
        public ModelHelpers(IBackend gallifrey, FlyoutsControl flyoutsControl)
        {
            this.flyoutsControl = flyoutsControl;
            Gallifrey = gallifrey;
            DialogContext = new DialogContext();
            openFlyouts = new List<OpenFlyoutDetails>();
        }

        #region Flyouts

        public void CloseAllFlyouts()
        {
            foreach (var item in openFlyouts)
            {
                CloseFlyout(item.Flyout);
            }
        }

        public void HideAllFlyouts()
        {
            foreach (var item in openFlyouts)
            {
                HideFlyout(item.Flyout);
            }
        }
        public void CloseFlyout(Flyout flyout)
        {
            var actualType = flyout.GetType();
            var openFlyoutDetail = openFlyouts.FirstOrDefault(x => x.FlyoutType == actualType);
            if (openFlyoutDetail != null)
            {
                openFlyoutDetail.IsHidden = false;
            }

            flyout.IsOpen = false;
            FlyoutClosedHandler(flyout, null);
        }

        public void HideFlyout(Flyout flyout)
        {
            var actualType = flyout.GetType();
            var openFlyoutDetail = openFlyouts.FirstOrDefault(x => x.FlyoutType == actualType);
            if (openFlyoutDetail != null)
                openFlyoutDetail.IsHidden = true;

            flyout.IsOpen = false;
        }

        public List<Flyout> GetHiddenFlyouts()
        {
            return openFlyouts.Where(x => x.IsHidden).Select(x => x.Flyout).ToList();
        }

        public Task<Flyout> OpenFlyout(Flyout flyout)
        {
            var actualType = flyout.GetType();
            var openFlyoutDetail = openFlyouts.FirstOrDefault(x => x.FlyoutType == actualType);

            if (openFlyoutDetail == null)
            {
                flyoutsControl.Items.Add(flyout);
                flyout.ClosingFinished += FlyoutClosedHandler;
                openFlyoutDetail = new OpenFlyoutDetails(flyout);
                openFlyouts.Add(openFlyoutDetail);
            }

            openFlyoutDetail.IsHidden = false;
            openFlyoutDetail.Flyout.IsOpen = true;

            return openFlyoutDetail.TaskCompletionSource.Task;
        }

        private void FlyoutClosedHandler(object sender, RoutedEventArgs e)
        {
            var openFlyoutDetail = openFlyouts.FirstOrDefault(x => x.FlyoutType == ((Flyout)sender).GetType());
            if (openFlyoutDetail == null) return;

            if (!openFlyoutDetail.IsHidden)
            {
                openFlyoutDetail.Flyout.ClosingFinished -= FlyoutClosedHandler;
                flyoutsControl.Items.Remove(openFlyoutDetail.Flyout);
                openFlyoutDetail.TaskCompletionSource.SetResult(openFlyoutDetail.Flyout);
                openFlyouts.Remove(openFlyoutDetail);
            }
        }

        internal class OpenFlyoutDetails
        {
            public Flyout Flyout { get; set; }
            public TaskCompletionSource<Flyout> TaskCompletionSource { get; set; }
            public Type FlyoutType { get; set; }
            public bool IsHidden { get; set; }
            public Guid OpenFlyoutDetailGuid { get; set; }

            public OpenFlyoutDetails(Flyout flyout)
            {
                Flyout = flyout;
                TaskCompletionSource = new TaskCompletionSource<Flyout>();
                FlyoutType = flyout.GetType();
                IsHidden = false;
                OpenFlyoutDetailGuid = Guid.NewGuid();
            }
        }

        #endregion

        #region Fire Events

        public void SetSelectedTimer(Guid value)
        {
            SelectTimerEvent?.Invoke(this, value);
        }

        public void SelectRunningTimer()
        {
            SelectRunningTimerEvent?.Invoke(this, null);
        }

        public void RefreshModel()
        {
            RefreshModelEvent?.Invoke(this, null);
        }

        public void TriggerRemoteButtonPress(RemoteButtonTrigger buttonTrigger)
        {
            if (buttonTrigger == Models.RemoteButtonTrigger.Export && Gallifrey.Settings.ExportSettings.TrackingOnly)
            {
                return;
            }

            RemoteButtonTrigger?.Invoke(null, buttonTrigger);
        }

        #endregion

        public void CloseApp(bool restart = false)
        {
            if (restart && Gallifrey.VersionControl.IsAutomatedDeploy)
            {
                System.Windows.Forms.Application.Restart();
            }

            Application.Current.Shutdown();
        }

        public async void ShowGetPremiumMessage(string message = "")
        {
            var premiumMessage = "To Get Premium Please Contribute Or Donate To Gallifrey.\n\nPremium Features Include:";
            premiumMessage += "\n  • Ability To Opt-Out Of Tracking";
            premiumMessage += "\n  • Bulk Export More Than 5 Timers";
            premiumMessage += "\n  • Use More Than 2 Local Timers";
            premiumMessage += "\n  • Start Now/Assign To Me/Change Status On Add";
            premiumMessage += "\n\nThink You Should Have Premium?\nPlease Contact Us By Email Or Twitter";

            if (!string.IsNullOrWhiteSpace(message))
            {
                premiumMessage = $"{message}\n\n{premiumMessage}";
            }

            //Could be a custom dialog if can work out how.
            var messageResult = await DialogCoordinator.Instance.ShowMessageAsync(DialogContext, "Get Premium", premiumMessage, MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary, new MetroDialogSettings { AffirmativeButtonText = "Cancel", NegativeButtonText = "Donate", FirstAuxiliaryButtonText = "Contribute", SecondAuxiliaryButtonText = "Contact", DefaultButtonFocus = MessageDialogResult.Affirmative, DialogMessageFontSize = 14 });

            switch (messageResult)
            {
                case MessageDialogResult.SecondAuxiliary:
                    TriggerRemoteButtonPress(Models.RemoteButtonTrigger.Info);
                    break;
                case MessageDialogResult.FirstAuxiliary:
                    TriggerRemoteButtonPress(Models.RemoteButtonTrigger.GitHub);
                    break;
                case MessageDialogResult.Negative:
                    TriggerRemoteButtonPress(Models.RemoteButtonTrigger.PayPal);
                    break;
            }
        }
    }
}