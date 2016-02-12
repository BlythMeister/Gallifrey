using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Gallifrey.UI.Modern.MainViews;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls;

namespace Gallifrey.UI.Modern.Helpers
{
    public class ModelHelpers
    {
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
            var openFlyout = flyoutsControl.Items.Cast<Flyout>().FirstOrDefault(item => item.GetType() == actualType);
            //Prevent 2 identical flyouts from opening
            if (openFlyout != null)
            {
                if (openFlyoutDetail != null)
                {
                    return openFlyoutDetail.TaskCompletionSource.Task;
                }
                else
                {
                    openFlyoutDetail = new OpenFlyoutDetails(openFlyout);
                    openFlyouts.Add(openFlyoutDetail);
                    return openFlyoutDetail.TaskCompletionSource.Task;
                }
            }

            if (openFlyoutDetail == null)
            {
                openFlyoutDetail = new OpenFlyoutDetails(flyout);
                openFlyouts.Add(openFlyoutDetail);
            }

            openFlyoutDetail.IsHidden = false;
            flyoutsControl.Items.Add(flyout);

            // when the flyout is closed, remove it from the hosting FlyoutsControl
            RoutedEventHandler closingFinishedHandler = null;
            closingFinishedHandler = (o, args) =>
            {
                var openFlyoutCheck = openFlyouts.FirstOrDefault(x => x.OpenFlyoutDetailGuid == openFlyoutDetail.OpenFlyoutDetailGuid);
                if (openFlyoutCheck == null) return;

                openFlyoutCheck.Flyout.ClosingFinished -= closingFinishedHandler;
                flyoutsControl.Items.Remove(openFlyoutCheck.Flyout);
                if (!openFlyoutCheck.IsHidden)
                {
                    openFlyoutDetail.TaskCompletionSource.SetResult(flyout);
                    openFlyouts.Remove(openFlyoutDetail);
                }
            };

            flyout.ClosingFinished += closingFinishedHandler;
            flyout.IsOpen = true;

            return openFlyoutDetail.TaskCompletionSource.Task;
        }

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

        public void CloseApp(bool restart = false)
        {
            if (restart && Gallifrey.VersionControl.IsAutomatedDeploy)
            {
                System.Windows.Forms.Application.Restart();
            }

            Application.Current.Shutdown();
        }

        public void TriggerRemoteButtonPress(RemoteButtonTrigger buttonTrigger)
        {
            RemoteButtonTrigger?.Invoke(null, buttonTrigger);
        }
    }
}