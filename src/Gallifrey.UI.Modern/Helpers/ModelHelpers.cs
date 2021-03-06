using Gallifrey.UI.Modern.Models;
using Gallifrey.Versions;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;

namespace Gallifrey.UI.Modern.Helpers
{
    public class ModelHelpers
    {
        private readonly FlyoutsControl flyoutsControl;
        private readonly List<OpenFlyoutDetails> openFlyouts;
        private readonly Notifier toastNotifier;
        public IBackend Gallifrey { get; }
        public DialogContext DialogContext { get; }
        public bool FlyoutOpenOrDialogShowing => openFlyouts.Count(x => !x.IsHidden) > 0 || DialogContext.InUse;

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
            toastNotifier = new Notifier(cfg =>
            {
                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(TimeSpan.FromSeconds(10), MaximumNotificationCount.FromCount(5));
                cfg.PositionProvider = new PrimaryScreenPositionProvider(Corner.BottomRight, 10, 40);
                cfg.Dispatcher = Application.Current.Dispatcher;
                cfg.DisplayOptions.TopMost = true;
                cfg.DisplayOptions.Width = 400;
            });
        }

        #region Flyouts

        public void CloseAllFlyouts()
        {
            foreach (var item in openFlyouts.ToList())
            {
                CloseFlyout(item.Flyout);
            }
        }

        public void HideAllFlyouts()
        {
            foreach (var item in openFlyouts.ToList())
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
            {
                openFlyoutDetail.IsHidden = true;
            }

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
            if (openFlyoutDetail == null)
            {
                return;
            }

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

        #endregion Flyouts

        #region Dialogs

        public async Task<MessageDialogResult> ShowMessageAsync(string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings settings = null)
        {
            try
            {
                DialogContext.InUse = true;
                return await DialogCoordinator.Instance.ShowMessageAsync(DialogContext, title, message, style, settings);
            }
            finally
            {
                DialogContext.InUse = false;
            }
        }

        public async Task<ProgressDialogController> ShowIndeterminateProgressAsync(string title, string message, bool canCancel = false, MetroDialogSettings settings = null)
        {
            DialogContext.InUse = true;
            var progress = await DialogCoordinator.Instance.ShowProgressAsync(DialogContext, title, message, canCancel, settings);
            progress.SetIndeterminate();
            progress.Closed += (sender, args) => DialogContext.InUse = false;
            progress.Canceled += (sender, args) => DialogContext.InUse = false;
            return progress;
        }

        public async Task<string> ShowInputAsync(string title, string message, MetroDialogSettings settings = null)
        {
            try
            {
                DialogContext.InUse = true;
                return await DialogCoordinator.Instance.ShowInputAsync(DialogContext, title, message, settings);
            }
            finally
            {
                DialogContext.InUse = false;
            }
        }

        public async Task ShowDialogAsync(BaseMetroDialog dialog, MetroDialogSettings settings = null)
        {
            DialogContext.InUse = true;
            await DialogCoordinator.Instance.ShowMetroDialogAsync(DialogContext, dialog, settings);
        }

        public async Task HideDialogAsync(BaseMetroDialog dialog, MetroDialogSettings settings = null)
        {
            try
            {
                await DialogCoordinator.Instance.HideMetroDialogAsync(DialogContext, dialog, settings);
            }
            finally
            {
                DialogContext.InUse = false;
            }
        }

        public async Task<LoginDialogData> ShowLoginAsync(string title, string message, LoginDialogSettings settings = null)
        {
            try
            {
                DialogContext.InUse = true;
                return await DialogCoordinator.Instance.ShowLoginAsync(DialogContext, title, message, settings);
            }
            finally
            {
                DialogContext.InUse = false;
            }
        }

        public void ShowNotification(string message)
        {
            var instanceType = Gallifrey.VersionControl.InstanceType;
            var title = (instanceType == InstanceType.Stable ? "Gallifrey" : $"Gallifrey ({instanceType})").ToUpper();

            toastNotifier.Notify(() => new ToastNotification(title, message));
        }

        #endregion Dialogs

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

        #endregion Fire Events

        public void CloseApp(bool restart = false)
        {
            if (restart && Gallifrey.VersionControl.IsAutomatedDeploy)
            {
                System.Diagnostics.Process.Start(Gallifrey.VersionControl.GetApplicationReference());
            }

            Application.Current.Shutdown();
        }
    }
}
