using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
                item.IsOpen = false;
            }
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
    }
}