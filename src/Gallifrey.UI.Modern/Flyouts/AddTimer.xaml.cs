using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly DateTime? startDate;
        private readonly ProgressDialogHelper progressDialogHelper;
        private AddTimerModel DataModel => (AddTimerModel)DataContext;
        public bool AddedTimer { get; set; }
        public Guid NewTimerId { get; set; }
        
        public AddTimer(ModelHelpers modelHelpers, string jiraRef = null, DateTime? startDate = null, bool? enableDateChange = null, List<IdleTimer> idleTimers = null, bool? startNow = null)
        {
            this.modelHelpers = modelHelpers;
            this.startDate = startDate;
            progressDialogHelper = new ProgressDialogHelper(modelHelpers.DialogContext);
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
                if (DataModel.TempTimer)
                {
                    if (modelHelpers.Gallifrey.JiraTimerCollection.GetAllTempTimers().Count() >= 2)
                    {
                        modelHelpers.ShowGetPremiumMessage("Without Gallifrey Premium You Are Limited To A Maximum Of 2 Temp Timers");
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

            if (!DataModel.TempTimer)
            {
                try
                {
                    var jiraSearchRef = DataModel.JiraReference;
                    var result = await progressDialogHelper.Do(() => modelHelpers.Gallifrey.JiraConnection.GetJiraIssue(jiraSearchRef), "Jira Check In Progress", true, true, TimeSpan.Zero);
                    if (result.Status == ProgressResult.JiraHelperStatus.Success)
                    {
                        jiraIssue = result.RetVal;
                    }
                    else
                    {
                        Focus();
                        return;
                    }
                }
                catch (NoResultsFoundException)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Jira", "Unable To Locate The Jira");
                    Focus();
                    return;
                }

                if (DataModel.JiraReferenceEditable)
                {
                    var correctJiraResult = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Correct Jira?", $"Jira found!\n\nRef: {jiraIssue.key}\nName: {jiraIssue.fields.summary}\n\nIs that correct?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                    if (correctJiraResult == MessageDialogResult.Negative)
                    {
                        Focus();
                        return;
                    }
                }
            }

            try
            {
                if (DataModel.TempTimer)
                {
                    NewTimerId = modelHelpers.Gallifrey.JiraTimerCollection.AddTempTimer(DataModel.TempTimerDescription, DataModel.StartDate.Value, seedTime, DataModel.StartNow);
                }
                else
                {
                    NewTimerId = modelHelpers.Gallifrey.JiraTimerCollection.AddTimer(jiraIssue, DataModel.StartDate.Value, seedTime, DataModel.StartNow);
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
                    var result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Duplicate Timer", "The Timer Already Exists, Would You Like To Add The Time?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

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
                        Focus();
                        return;
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
                    Focus();
                    return;
                }

                NewTimerId = ex.TimerId;
            }

            AddedTimer = true;
            var stillDoingThings = false;

            if (!DataModel.TempTimer)
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

        private async void SearchButton(object sender, RoutedEventArgs e)
        {
            modelHelpers.HideFlyout(this);
            var searchFlyout = new Search(modelHelpers, true, startDate ?? DateTime.Now.Date);
            await modelHelpers.OpenFlyout(searchFlyout);
            if (searchFlyout.SelectedJira != null)
            {
                DataModel.SetJiraReference(searchFlyout.SelectedJira.Reference);
            }
            modelHelpers.OpenFlyout(this);
        }

        private void StartDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataModel.StartDate.HasValue && DataModel.StartDate.Value.Date != DateTime.Now.Date)
            {
                DataModel.SetStartNowEnabled(false);
            }
            else
            {
                DataModel.SetStartNowEnabled(true);
            }
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
