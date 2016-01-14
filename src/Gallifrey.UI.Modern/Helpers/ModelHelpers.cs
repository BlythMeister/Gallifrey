using System;
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
        private readonly FlyoutsControl flyoutsControl;
        public IBackend Gallifrey { get; }
        public DialogContext DialogContext { get; }
        public event EventHandler<Guid> SelectTimerEvent;
        public event EventHandler SelectRunningTimerEvent;
        public event EventHandler RefreshModelEvent;
        public event EventHandler<RemoteButtonTrigger> RemoteButtonTrigger;

        public ModelHelpers(IBackend gallifrey, FlyoutsControl flyoutsControl)
        {
            this.flyoutsControl = flyoutsControl;
            Gallifrey = gallifrey;
            DialogContext = new DialogContext();
        }

        public void CloseAllFlyouts()
        {
            foreach (Flyout item in flyoutsControl.Items)
            {
                CloseFlyout(item);
            }
        }

        public void CloseFlyout(Flyout flyout)
        {
            flyout.IsOpen = false;
        }

        public Task<Flyout> OpenFlyout(Flyout flyout)
        {
            var actualType = flyout.GetType();
            var taskCompletion = new TaskCompletionSource<Flyout>();

            //Prevent 2 identical flyouts from opening
            if (flyoutsControl.Items.Cast<Flyout>().Any(item => item.GetType() == actualType))
            {
                taskCompletion.SetResult(flyout);
            }
            else
            {
                flyoutsControl.Items.Add(flyout);

                // when the flyout is closed, remove it from the hosting FlyoutsControl
                RoutedEventHandler closingFinishedHandler = null;
                closingFinishedHandler = (o, args) =>
                {
                    flyout.ClosingFinished -= closingFinishedHandler;
                    flyoutsControl.Items.Remove(flyout);
                    taskCompletion.SetResult(flyout);
                };

                flyout.ClosingFinished += closingFinishedHandler;
                flyout.IsOpen = true;
            }

            return taskCompletion.Task;
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