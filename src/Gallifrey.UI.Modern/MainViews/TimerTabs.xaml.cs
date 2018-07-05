using Exceptionless;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.UI.Modern.Flyouts;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;

namespace Gallifrey.UI.Modern.MainViews
{
    public partial class TimerTabs
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;
        private ModelHelpers ModelHelpers => ((MainViewModel)DataContext).ModelHelpers;

        public TimerTabs()
        {
            InitializeComponent();
        }

        private async void TimerList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var timerIds = ViewModel.GetSelectedTimerIds().ToList();

            if (timerIds.Any())
            {
                var timerId = timerIds.First();

                if (timerIds.Count > 1)
                {
                    var timer = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimer(timerId);
                    await DialogCoordinator.Instance.ShowMessageAsync(ModelHelpers.DialogContext, "Multiple Timers Selected", $"You Have 2 Timers Selected, Can Only Start 1.\nWill Start Timer: {timer.JiraReference}\n\n{timer.JiraName}");
                }

                var runningTimerId = ModelHelpers.Gallifrey.JiraTimerCollection.GetRunningTimerId();

                if (runningTimerId.HasValue && runningTimerId.Value == timerId)
                {
                    ModelHelpers.Gallifrey.JiraTimerCollection.StopTimer(timerId, false);
                }
                else
                {
                    try
                    {
                        ModelHelpers.Gallifrey.JiraTimerCollection.StartTimer(timerId);
                    }
                    catch (DuplicateTimerException ex)
                    {
                        ModelHelpers.Gallifrey.JiraTimerCollection.StartTimer(ex.TimerId);
                    }

                    ModelHelpers.RefreshModel();
                    ModelHelpers.SelectRunningTimer();
                }
            }
        }

        private void TabHeaderRightClick(object sender, RoutedEventArgs e)
        {
            ((TabItem)sender).IsSelected = true;
            e.Handled = true;
        }

        private void TabDragOver(object sender, DragEventArgs e)
        {
            var url = GetUrl(e);
            if (!string.IsNullOrWhiteSpace(url))
            {
                var uriDrag = new Uri(url);
                var jiraUri = new Uri(ModelHelpers.Gallifrey.Settings.JiraConnectionSettings.JiraUrl);
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
                await AddFromUrl(url);
            }
        }

        private static string GetUrl(DragEventArgs e)
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

        private async Task AddFromUrl(string url)
        {
            var uriDrag = new Uri(url).AbsolutePath;
            var jiraRef = uriDrag.Substring(uriDrag.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase) + 1);
            var todaysDate = DateTime.Now.Date;
            var dayTimers = ModelHelpers.Gallifrey.JiraTimerCollection.GetTimersForADate(todaysDate).ToList();

            if (dayTimers.Any(x => x.JiraReference == jiraRef))
            {
                ModelHelpers.Gallifrey.JiraTimerCollection.StartTimer(dayTimers.First(x => x.JiraReference == jiraRef).UniqueId);
                ModelHelpers.RefreshModel();
                ModelHelpers.SelectRunningTimer();
            }
            else
            {
                //Validate jira is real
                try
                {
                    if (!ModelHelpers.Gallifrey.JiraConnection.DoesJiraExist(jiraRef))
                    {
                        await DialogCoordinator.Instance.ShowMessageAsync(ModelHelpers.DialogContext, "Invalid Jira", $"Unable To Locate That Jira.\n\nJira Ref Dropped: '{jiraRef}'");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionlessClient.Default.CreateEvent().SetException(ex).AddTags("Handled").Submit();
                    await DialogCoordinator.Instance.ShowMessageAsync(ModelHelpers.DialogContext, "Invalid Jira", $"Unable To Locate That Jira.\n\nJira Ref Dropped: '{jiraRef}'");
                    return;
                }

                //show add form, we know it's a real jira & valid
                var addTimer = new AddTimer(ModelHelpers, startDate: todaysDate, jiraRef: jiraRef, startNow: true);
                await ModelHelpers.OpenFlyout(addTimer);
                if (addTimer.AddedTimer)
                {
                    ModelHelpers.SetSelectedTimer(addTimer.NewTimerId);
                }
            }
        }

        private void ContextMenu_Add(object sender, RoutedEventArgs e)
        {
            ModelHelpers.TriggerRemoteButtonPress(RemoteButtonTrigger.Add);
        }

        private void ContextMenu_AddToFill(object sender, RoutedEventArgs e)
        {
            ModelHelpers.TriggerRemoteButtonPress(RemoteButtonTrigger.AddToFill);
        }

        private void ContextMenu_Copy(object sender, RoutedEventArgs e)
        {
            ModelHelpers.TriggerRemoteButtonPress(RemoteButtonTrigger.Copy);
        }

        private void ContextMenu_Delete(object sender, RoutedEventArgs e)
        {
            ModelHelpers.TriggerRemoteButtonPress(RemoteButtonTrigger.Delete);
        }

        private void ContextMenu_Edit(object sender, RoutedEventArgs e)
        {
            ModelHelpers.TriggerRemoteButtonPress(RemoteButtonTrigger.Edit);
        }

        private void ContextMenu_Export(object sender, RoutedEventArgs e)
        {
            ModelHelpers.TriggerRemoteButtonPress(RemoteButtonTrigger.Export);
        }

        private void ContextMenu_StartStop(object sender, RoutedEventArgs e)
        {
            TimerList_MouseDoubleClick(sender, null);
        }
    }
}
