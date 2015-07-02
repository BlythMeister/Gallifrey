using System;
using System.Windows;
using System.Windows.Controls;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for EditTimer.xaml
    /// </summary>
    public partial class EditTimer
    {
        private readonly MainViewModel viewModel;
        private EditTimerModel DataModel { get { return (EditTimerModel)DataContext; } }

        public EditTimer(MainViewModel viewModel, Guid selected)
        {
            this.viewModel = viewModel;
            InitializeComponent();

            DataContext = new EditTimerModel(viewModel.Gallifrey, selected);
        }

        private async void SaveButton(object sender, RoutedEventArgs e)
        {
            var uniqueId = DataModel.UniqueId;

            if (DataModel.HasModifiedRunDate())
            {
                if (!DataModel.RunDate.HasValue)
                {
                    viewModel.MainWindow.ShowMessageAsync("Missing Date", "You Must Enter A Start Date");
                    return;
                }

                if (DataModel.RunDate.Value < DataModel.MinDate || DataModel.RunDate.Value > DataModel.MaxDate)
                {
                    viewModel.MainWindow.ShowMessageAsync("Invalid Date", string.Format("You Must Enter A Start Date Between {0} And {1}", DataModel.MinDate.ToShortDateString(), DataModel.MaxDate.ToShortDateString()));
                    return;
                }

                try
                {
                    uniqueId = viewModel.Gallifrey.JiraTimerCollection.ChangeTimerDate(uniqueId, DataModel.RunDate.Value);
                }
                catch (Exception)
                {
                    viewModel.MainWindow.ShowMessageAsync("Duplicate Timer", "This Timer Already Exists On That Date!");
                    return;
                }
            }

            if (DataModel.HasModifiedJiraReference())
            {
                Issue jiraIssue;
                try
                {
                    jiraIssue = viewModel.Gallifrey.JiraConnection.GetJiraIssue(DataModel.JiraReference);
                }
                catch (NoResultsFoundException)
                {
                    viewModel.MainWindow.ShowMessageAsync("Invalid Jira", "Unable To Locate The Jira");
                    return;
                }

                var result = await viewModel.MainWindow.ShowMessageAsync("Correct Jira?", string.Format("Jira found!\n\nRef: {0}\nName: {1}\n\nIs that correct?", jiraIssue.key, jiraIssue.fields.summary), MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No" });

                if (result == MessageDialogResult.Negative)
                {
                    return;
                }

                try
                {
                    uniqueId = viewModel.Gallifrey.JiraTimerCollection.RenameTimer(uniqueId, jiraIssue);
                }
                catch (Exception)
                {
                    viewModel.MainWindow.ShowMessageAsync("Duplicate Timer", "This Timer Already Exists On That Date!");
                    return;
                }
            }

            if (DataModel.HasModifiedTime())
            {
                var orignalTime = new TimeSpan(DataModel.OriginalHours, DataModel.OriginalMinutes, 0);
                var newTime = new TimeSpan(DataModel.Hours, DataModel.Minutes, 0);
                var difference = newTime.Subtract(orignalTime);
                var addTime = difference.TotalSeconds > 0;

                viewModel.Gallifrey.JiraTimerCollection.AdjustTime(uniqueId, Math.Abs(difference.Hours), Math.Abs(difference.Minutes), addTime);
            }

            viewModel.RefreshModel();
            viewModel.SetSelectedTimer(uniqueId);
            IsOpen = false;
        }
    }
}
