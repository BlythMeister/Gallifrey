using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interactivity;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.UI.Modern.Flyouts;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

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
                    ViewModel.Gallifrey.JiraTimerCollection.StopTimer(timerId.Value, false);
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
                        DialogCoordinator.Instance.ShowMessageAsync(ViewModel.DialogContext, "Wrong Day!", "Use The Version Of This Timer For Today!");
                    }
                }

            }
        }

        private void TabDragOver(object sender, DragEventArgs e)
        {
            var url = GetUrl(e);
            if (!string.IsNullOrWhiteSpace(url))
            {
                var uriDrag = new Uri(url);
                var jiraUri = new Uri(ViewModel.Gallifrey.Settings.JiraConnectionSettings.JiraUrl);
                if (uriDrag.Host == jiraUri.Host)
                {
                    e.Effects = DragDropEffects.Copy;
                    e.Handled = true;
                }
            }

            if (!e.Handled)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private async void TagDragDrop(object sender, DragEventArgs e)
        {
            var url = GetUrl(e);
            if (!string.IsNullOrWhiteSpace(url))
            {
                var uriDrag = new Uri(url).AbsolutePath;
                var jiraRef = uriDrag.Substring(uriDrag.LastIndexOf("/") + 1);
                var todaysDate = DateTime.Now.Date;
                var dayTimers = ViewModel.Gallifrey.JiraTimerCollection.GetTimersForADate(todaysDate).ToList();

                if (dayTimers.Any(x => x.JiraReference == jiraRef))
                {
                    ViewModel.Gallifrey.JiraTimerCollection.StartTimer(dayTimers.First(x => x.JiraReference == jiraRef).UniqueId);
                    ViewModel.RefreshModel();
                    ViewModel.SelectRunningTimer();
                }
                else
                {
                    //Validate jira is real
                    try
                    {
                        ViewModel.Gallifrey.JiraConnection.GetJiraIssue(jiraRef);
                    }
                    catch (Exception)
                    {
                        DialogCoordinator.Instance.ShowMessageAsync(ViewModel.DialogContext, "Invalid Jira", string.Format("Unable To Locate That Jira.\n\nJira Ref Dropped: '{0}'", jiraRef));
                        return;
                    }

                    //show add form, we know it's a real jira & valid
                    await ViewModel.OpenFlyout(new AddTimer(ViewModel, startDate: todaysDate, jiraRef: jiraRef, startNow: true));
                }
            }
        }

        private string GetUrl(DragEventArgs e)
        {
            if ((e.Effects & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                if (e.Data.GetDataPresent("Text"))
                {
                    return (string)e.Data.GetData("Text");
                }
            }

            return string.Empty;
        }
    }
}
