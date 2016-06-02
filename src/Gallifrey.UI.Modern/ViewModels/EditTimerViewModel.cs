using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Helpers;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.ViewModels
{
    public class EditTimerViewModel : ViewModelBase
    {
        private readonly ModelHelpers modelHelpers;
        private readonly bool hasExportedTime;

        private bool tempTimer;
        private string jiraReference;
        private string tempTimerDescription;
        private DateTime? runDate;
        private int hours;
        private int minutes;
        private bool isDefaultOnButton;

        public EditTimerViewModel(ModelHelpers modelHelpers)
        {
            this.modelHelpers = modelHelpers;
        }

        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime DisplayDate { get; set; }
        public bool TimeEditable { get; set; }
        public string OriginalJiraReference { get; set; }
        public string OriginalTempTimerDescription { get; set; }
        public DateTime? OriginalRunDate { get; set; }
        public int OriginalHours { get; set; }
        public int OriginalMinutes { get; set; }
        public Guid EditedTimerId { get; set; }

        public bool IsDefaultOnButton
        {
            get { return isDefaultOnButton; }
            set
            {
                isDefaultOnButton = value;
                RaisePropertyChanged();
            }
        }

        public bool DateEditable => !hasExportedTime && !HasModifiedJiraReference;
        public bool JiraReferenceEditable => !hasExportedTime && !HasModifiedRunDate;
        public bool HasModifiedJiraReference => (OriginalJiraReference != JiraReference) || (OriginalTempTimerDescription != TempTimerDescription);
        public bool HasModifiedRunDate => OriginalRunDate.Value.Date != RunDate.Value.Date;
        public bool HasModifiedTime => OriginalHours != Hours || OriginalMinutes != Minutes;


        public string JiraReference
        {
            get { return jiraReference; }
            set
            {
                jiraReference = value;
                RaisePropertyChanged();
            }
        }

        public string TempTimerDescription
        {
            get { return tempTimerDescription; }
            set
            {
                tempTimerDescription = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? RunDate
        {
            get { return runDate; }
            set
            {
                runDate = value;
                RaisePropertyChanged();
            }
        }

        public int? Hours
        {
            get { return hours; }
            set
            {
                hours = value ?? 0;
                RaisePropertyChanged();
            }
        }

        public int? Minutes
        {
            get { return minutes; }
            set
            {
                var newValue = value ?? 0;

                if (newValue < 0)
                {
                    if (Hours == 0)
                    {
                        minutes = 0;
                    }
                    else
                    {
                        minutes = 60 + newValue;
                        Hours--;
                    }
                }
                else if (value >= 60)
                {
                    if (Hours == 9)
                    {
                        minutes = 59;
                    }
                    else
                    {
                        Hours++;
                        minutes = newValue - 60;
                    }
                }
                else
                {
                    minutes = newValue;
                }
                RaisePropertyChanged();
                RaisePropertyChanged(()=> HasModifiedTime);
            }
        }

        public bool TempTimer
        {
            get { return tempTimer; }
            set
            {
                tempTimer = value;
                RaisePropertyChanged();
                TempTimerDescription = OriginalTempTimerDescription;
                JiraReference = OriginalJiraReference;
             }
        }

        public RelayCommand SaveCommand { get; private set; }

        public void SetNotDefaultButton()
        {
            IsDefaultOnButton = false;

        }

        public void AdjustTime(TimeSpan timeAdjustmentAmount, bool addTime)
        {
            var currentTime = new TimeSpan(Hours ?? 0, Minutes ?? 0, 0);

            currentTime = addTime ? currentTime.Add(timeAdjustmentAmount) : currentTime.Subtract(timeAdjustmentAmount);

            Hours = currentTime.Hours > 9 ? 9 : currentTime.Hours;
            Minutes = currentTime.Minutes;
        }

        public async void Save()
        {
            if (HasModifiedRunDate)
            {
                if (!RunDate.HasValue)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Missing Date", "You Must Enter A Start Date");
                    return;
                }

                if (RunDate.Value < MinDate || RunDate.Value > MaxDate)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Date", $"You Must Enter A Start Date Between {MinDate.ToShortDateString()} And {MaxDate.ToShortDateString()}");
                    return;
                }

                try
                {
                    EditedTimerId = modelHelpers.Gallifrey.JiraTimerCollection.ChangeTimerDate(EditedTimerId, RunDate.Value);
                }
                catch (DuplicateTimerException ex)
                {
                    var handlerd = await MergeTimers(ex);
                    if (!handlerd)
                    {
                        return;
                    }
                }
            }
            else if (HasModifiedJiraReference)
            {
                if (TempTimer)
                {
                    var currentTimer = modelHelpers.Gallifrey.JiraTimerCollection.GetTimer(EditedTimerId);
                    if (!currentTimer.TempTimer)
                    {
                        if (!modelHelpers.Gallifrey.Settings.InternalSettings.IsPremium)
                        {
                            var tempTimersCount = modelHelpers.Gallifrey.JiraTimerCollection.GetAllTempTimers().Count();
                            if (tempTimersCount >= 2)
                            {
                                modelHelpers.ShowGetPremiumMessage("Without Gallifrey Premium You Are Limited To A Maximum Of 2 Temp Timers");
                                return;
                            }
                        }
                    }
                    EditedTimerId = modelHelpers.Gallifrey.JiraTimerCollection.ChangeTempTimerDescription(EditedTimerId, TempTimerDescription);
                }
                else
                {
                    Issue jiraIssue;
                    try
                    {
                        jiraIssue = modelHelpers.Gallifrey.JiraConnection.GetJiraIssue(JiraReference);
                    }
                    catch (NoResultsFoundException)
                    {
                        await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Jira", "Unable To Locate The Jira");
                        return;
                    }

                    var result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Correct Jira?", $"Jira found!\n\nRef: {jiraIssue.key}\nName: {jiraIssue.fields.summary}\n\nIs that correct?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                    if (result == MessageDialogResult.Negative)
                    {
                        return;
                    }

                    try
                    {
                        EditedTimerId = modelHelpers.Gallifrey.JiraTimerCollection.RenameTimer(EditedTimerId, jiraIssue);
                    }
                    catch (DuplicateTimerException ex)
                    {
                        var handlerd = await MergeTimers(ex);
                        if (!handlerd)
                        {
                            return;
                        }
                    }
                }
            }

            if (HasModifiedTime)
            {
                var originalTime = new TimeSpan(OriginalHours, OriginalMinutes, 0);
                var newTime = new TimeSpan(Hours ?? 0, Minutes ?? 0, 0);
                var difference = newTime.Subtract(originalTime);
                var addTime = difference.TotalSeconds > 0;

                modelHelpers.Gallifrey.JiraTimerCollection.AdjustTime(EditedTimerId, Math.Abs(difference.Hours), Math.Abs(difference.Minutes), addTime);
            }

            modelHelpers.RefreshModel();
            modelHelpers.SetSelectedTimer(EditedTimerId);
            //TODO: close edit timer
            modelHelpers.CloseAllFlyouts();
        }

        private async Task<bool> MergeTimers(DuplicateTimerException ex)
        {
            var duplicateTimerMerge = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Timer Already Exists", "This Timer Already Exists On That With That Name On That Date, Would You Like To Merge Them?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

            if (duplicateTimerMerge == MessageDialogResult.Negative)
            {
                return false;
            }

            modelHelpers.Gallifrey.JiraTimerCollection.AdjustTime(ex.TimerId, OriginalHours, OriginalMinutes, true);
            modelHelpers.Gallifrey.JiraTimerCollection.RemoveTimer(EditedTimerId);
            EditedTimerId = ex.TimerId;
            return true;
        }
    }
}
