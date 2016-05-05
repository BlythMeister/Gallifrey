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
        public bool FlyoutOpen => flyoutsControl.Items.Count > 0;

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
            flyout.IsOpen = false;
        }

        public void CloseHiddenFlyout(Flyout flyout)
        {
            var actualType = flyout.GetType();
            var openFlyoutDetail = openFlyouts.FirstOrDefault(x => x.FlyoutType == actualType);
            if (openFlyoutDetail != null) openFlyoutDetail.IsHidden = false;
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
            flyout.IsOpen = true;

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
            string premiumMessage;

            if (string.IsNullOrWhiteSpace(message))
            {
                premiumMessage = "To Get Access To Premium Features You Need To Either Contribute To Gallifrey Or Donate.\n\nPremium Features Include:\n=>Ability To Opt-Out Of Tracking";
            }
            else
            {
                premiumMessage = $"{message}\n\nTo Get Access To Premium Features You Need To Either Contribute To Gallifrey Or Donate.\n\nPremium Features Include:\n=>Ability To Opt-Out Of Tracking";
            }

            var messageResult = await DialogCoordinator.Instance.ShowMessageAsync(DialogContext, "Get Premium", premiumMessage, MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, new MetroDialogSettings { AffirmativeButtonText = "Close", NegativeButtonText = "Contribute", FirstAuxiliaryButtonText = "Donate", DefaultButtonFocus = MessageDialogResult.Affirmative });

            switch (messageResult)
            {
                case MessageDialogResult.Negative:
                    TriggerRemoteButtonPress(Models.RemoteButtonTrigger.GitHub);
                    break;
                case MessageDialogResult.FirstAuxiliary:
                    TriggerRemoteButtonPress(Models.RemoteButtonTrigger.PayPal);
                    break;
            }
        }
    }
}