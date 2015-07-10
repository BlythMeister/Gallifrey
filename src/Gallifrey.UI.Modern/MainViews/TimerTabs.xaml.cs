using System.Windows.Controls;
using System.Windows.Input;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.MainViews
{
    public partial class TimerTabs : UserControl
    {
        private MainViewModel ViewModel { get { return (MainViewModel)DataContext; } }

        public TimerTabs()
        {
            InitializeComponent();
        }

        private void TimerList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var timerId = ViewModel.GetSelectedTimerId();

            if (timerId.HasValue)
            {
                var runningTimer = ViewModel.Gallifrey.JiraTimerCollection.GetRunningTimerId();

                if (runningTimer.HasValue && runningTimer.Value == timerId.Value)
                {
                    ViewModel.Gallifrey.JiraTimerCollection.StopTimer(timerId.Value);
                }
                else
                {
                    try
                    {
                        ViewModel.Gallifrey.JiraTimerCollection.StartTimer(timerId.Value);
                        ViewModel.RefreshModel();
                        ViewModel.SelectRunningTimer();
                    }
                    catch (DuplicateTimerException)
                    {
                        ViewModel.DialogCoordinator.ShowMessageAsync(ViewModel, "Wrong Day!", "Use The Version Of This Timer For Today!");                       
                    }
                }
                
            }
        }
    }
}
