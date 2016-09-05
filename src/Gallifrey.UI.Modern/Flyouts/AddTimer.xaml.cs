using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.IdleTimers;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class AddTimer
    {
        private readonly ModelHelpers modelHelpers;
        private AddTimerModel DataModel => (AddTimerModel)DataContext;
        public bool AddedTimer { get; set; }
        public Guid NewTimerId { get; set; }
        
        public AddTimer(ModelHelpers modelHelpers, string jiraRef = null, DateTime? startDate = null, bool? enableDateChange = null, List<IdleTimer> idleTimers = null, bool? startNow = null)
        {
            this.modelHelpers = modelHelpers;
            InitializeComponent();

            if (!modelHelpers.Gallifrey.Settings.InternalSettings.IsPremium && startNow.HasValue && startNow.Value)
            {
                startNow = false;
            }

            DataContext = new AddTimerModel(modelHelpers.Gallifrey, jiraRef, startDate, enableDateChange, idleTimers, startNow);
            AddedTimer = false;
        }

        private async void AddButton(object sender, RoutedEventArgs e)
        {
            if (!DataModel.StartDate.HasValue)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Missing Date", "You Must Enter A Start Date");
                Focus();
                return;
            }

            if (DataModel.StartDate.Value < DataModel.MinDate || DataModel.StartDate.Value > DataModel.MaxDate)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Date", $"You Must Enter A Start Date Between {DataModel.MinDate.ToShortDateString()} And {DataModel.MaxDate.ToShortDateString()}");
                Focus();
                return;
            }

            //Validate Premium Features
            if (!modelHelpers.Gallifrey.Settings.InternalSettings.IsPremium)
            {
                if (DataModel.LocalTimer)
                {
                    if (modelHelpers.Gallifrey.JiraTimerCollection.GetAllLocalTimers().Count() >= 2)
                    {
                        modelHelpers.ShowGetPremiumMessage("Without Gallifrey Premium You Are Limited To A Maximum Of 2 Local Timers");
                        Focus();
                        return;
                    }
                }

                if (DataModel.StartNow)
                {
                    modelHelpers.ShowGetPremiumMessage("Without Gallifrey Premium You Cannot Start Timer Now.");
                    DataModel.StartNow = false;
                    Focus();
                    return;
                }

                if (DataModel.AssignToMe)
                {
                    modelHelpers.ShowGetPremiumMessage("Without Gallifrey Premium You Cannot Assign To Yourself.");
                    DataModel.AssignToMe = false;
                    Focus();
                    return;
                }

                if (DataModel.ChangeStatus)
                {
                    modelHelpers.ShowGetPremiumMessage("Without Gallifrey Premium You Cannot Change Timer Status.");
                    DataModel.ChangeStatus = false;
                    Focus();
                    return;
                }
            }

            TimeSpan seedTime;
            if (DataModel.TimeEditable)
            {
                seedTime = new TimeSpan(DataModel.StartHours ?? 0, DataModel.StartMinutes ?? 0, 0);
            }
            else
            {
                seedTime = new TimeSpan();
            }

            Issue jiraIssue = null;

            if (!DataModel.LocalTimer)
            {
                try
                {
                    jiraIssue = modelHelpers.Gallifrey.JiraConnection.GetJiraIssue(DataModel.JiraReference);
                }
                catch (NoResultsFoundException)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Jira", "Unable To Locate The Jira");
                    Focus();
                    return;
                }

                if (DataModel.JiraReferenceEditable)
                {
                    var result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Correct Jira?", $"Jira found!\n\nRef: {jiraIssue.key}\nName: {jiraIssue.fields.summary}\n\nIs that correct?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                    if (result == MessageDialogResult.Negative)
                    {
                        Focus();
                        return;
                    }
                }
            }

            if (DataModel.DatePeriod)
            {
                AddedTimer = await AddPeriodTimer(jiraIssue, seedTime);
            }
            else
            {
                AddedTimer = await AddSingleTimer(jiraIssue, seedTime, DataModel.StartDate.Value);
            }

            if (!AddedTimer)
            {
                Focus();
                return;
            }

            var stillDoingThings = false;

            if (!DataModel.LocalTimer)
            {
                if (DataModel.AssignToMe)
                {
                    try
                    {
                        modelHelpers.Gallifrey.JiraConnection.AssignToCurrentUser(DataModel.JiraReference);
                    }
                    catch (JiraConnectionException)
                    {
                        await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Assign Jira Error", "Unable To Locate Assign Jira To Current User");
                    }
                }

                if (DataModel.ChangeStatus)
                {
                    try
                    {
                        var transitionsAvaliable = modelHelpers.Gallifrey.JiraConnection.GetTransitions(DataModel.JiraReference);

                        var timeSelectorDialog = (BaseMetroDialog)this.Resources["TransitionSelector"];
                        await DialogCoordinator.Instance.ShowMetroDialogAsync(modelHelpers.DialogContext, timeSelectorDialog);

                        var comboBox = timeSelectorDialog.FindChild<ComboBox>("Items");
                        comboBox.ItemsSource = transitionsAvaliable.Select(x=>x.name).ToList();

                        stillDoingThings = true;
                    }
                    catch (Exception)
                    {
                        await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Status Update Error", "Unable To Change The Status Of This Issue");
                    }
                }
            }

            if (!stillDoingThings)
            {
                modelHelpers.CloseFlyout(this);
                modelHelpers.RefreshModel();
            }
        }

        private async Task<bool> AddPeriodTimer(Issue jiraIssue, TimeSpan seedTime)
        {
            var workingDate = DataModel.StartDate.Value;
            

            while (workingDate <= DataModel.EndDate.Value)
            {
                var addTimer = true;
                if (!modelHelpers.Gallifrey.Settings.AppSettings.ExportDays.Contains(workingDate.DayOfWeek))
                {
                    var result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Add For Non-Working Day?", $"The Date {workingDate.ToString("ddd, dd MMM")} Is Not A Working Day.\nWould You Still Like To Add A Timer For This Date?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                    if (result == MessageDialogResult.Negative)
                    {
                        addTimer = false;
                    }
                }

                if (addTimer)
                {
                    var added = await AddSingleTimer(jiraIssue, seedTime, workingDate);
                    if (!added && workingDate < DataModel.EndDate.Value)
                    {
                        var result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Continue Adding?", $"The Timer For {workingDate.ToString("ddd, dd MMM")} Was Not Added.\nWould You Like To Carry On Adding For The Remaining Dates?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                        if (result == MessageDialogResult.Negative)
                        {
                            return false;
                        }
                    }
                }
                
                workingDate = workingDate.AddDays(1);
            }

            return true;
        }

        private async Task<bool> AddSingleTimer(Issue jiraIssue, TimeSpan seedTime, DateTime startDate)
        {
            try
            {
                if (DataModel.LocalTimer)
                {
                    NewTimerId = modelHelpers.Gallifrey.JiraTimerCollection.AddLocalTimer(DataModel.LocalTimerDescription, startDate, seedTime, DataModel.StartNow);
                }
                else
                {
                    NewTimerId = modelHelpers.Gallifrey.JiraTimerCollection.AddTimer(jiraIssue, startDate, seedTime, DataModel.StartNow);
                }
                AddedTimer = true;
                if (!DataModel.TimeEditable)
                {
                    modelHelpers.Gallifrey.JiraTimerCollection.AddIdleTimer(NewTimerId, DataModel.IdleTimers);
                }
            }
            catch (DuplicateTimerException ex)
            {
                var doneSomething = false;
                if (seedTime.TotalMinutes > 0 || !DataModel.TimeEditable)
                {
                    var result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Duplicate Timer", $"The Timer Already Exists On {DataModel.StartDate.Value.ToString("ddd, dd MMM")}, Would You Like To Add The Time?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                    if (result == MessageDialogResult.Affirmative)
                    {
                        if (DataModel.TimeEditable)
                        {
                            modelHelpers.Gallifrey.JiraTimerCollection.AdjustTime(ex.TimerId, seedTime.Hours, seedTime.Minutes, true);
                        }
                        else
                        {
                            modelHelpers.Gallifrey.JiraTimerCollection.AddIdleTimer(ex.TimerId, DataModel.IdleTimers);
                        }

                        doneSomething = true;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (DataModel.StartNow)
                {
                    modelHelpers.Gallifrey.JiraTimerCollection.StartTimer(ex.TimerId);
                    doneSomething = true;
                }

                if (!doneSomething)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Duplicate Timer", "This Timer Already Exists!");
                    return false;
                }

                NewTimerId = ex.TimerId;
            }

            return true;
        }

        private async void SearchButton(object sender, RoutedEventArgs e)
        {
            modelHelpers.HideFlyout(this);
            var searchFlyout = new Search(modelHelpers, openFromAdd: true);
            await modelHelpers.OpenFlyout(searchFlyout);
            if (searchFlyout.SelectedJira != null)
            {
                DataModel.SetJiraReference(searchFlyout.SelectedJira.Reference);
            }
            modelHelpers.OpenFlyout(this);
        }

        private async void TransitionSelected(object sender, RoutedEventArgs e)
        {
            var selectedTransition = string.Empty;

            try
            {
                var dialog = (BaseMetroDialog)this.Resources["TransitionSelector"];
                var comboBox = dialog.FindChild<ComboBox>("Items");
                
                selectedTransition = (string)comboBox.SelectedItem;

                await DialogCoordinator.Instance.HideMetroDialogAsync(modelHelpers.DialogContext, dialog);

                modelHelpers.Gallifrey.JiraConnection.TransitionIssue(DataModel.JiraReference, selectedTransition);
            }
            catch (StateChangedException)
            {
                if (string.IsNullOrWhiteSpace(selectedTransition))
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Status Update Error", "Unable To Change The Status Of This Issue");
                }
                else
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Status Update Error", $"Unable To Change The Status Of This Issue To {selectedTransition}");
                }
            }

            modelHelpers.CloseFlyout(this);
            modelHelpers.RefreshModel();
        }

        private async void CancelTransition(object sender, RoutedEventArgs e)
        {
            var dialog = (BaseMetroDialog)this.Resources["TransitionSelector"];
            await DialogCoordinator.Instance.HideMetroDialogAsync(modelHelpers.DialogContext, dialog);

            modelHelpers.CloseFlyout(this);
            modelHelpers.RefreshModel();
        }
    }
}
