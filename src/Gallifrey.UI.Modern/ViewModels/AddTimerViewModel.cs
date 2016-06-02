using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Exceptions.JiraTimers;
using Gallifrey.IdleTimers;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Flyouts;
using Gallifrey.UI.Modern.Helpers;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.ViewModels
{
    public class AddTimerViewModel : ViewModelBase
    {
        private bool tempTimer;
        private int startMinutes;
        private int startHours;
        private string _jiraReference;
        private string _tempTimerDescription;
        private bool _jiraReferenceEditable;
        private DateTime _minDate;
        private DateTime _maxDate;
        private DateTime? _startDate;
        private DateTime _displayDate;
        private bool _dateEditable;
        private bool _startNow;
        private bool _startNowEditable;
        private bool _assignToMe;
        private bool _inProgress;
        private readonly ModelHelpers modelHelpers;

        public string JiraReference
        {
            get { return _jiraReference; }
            set
            {
                _jiraReference = value;
                RaisePropertyChanged();
            }
        }

        public string TempTimerDescription
        {
            get { return _tempTimerDescription; }
            set
            {
                _tempTimerDescription = value;
                RaisePropertyChanged();
            }
        }

        public bool JiraReferenceEditable
        {
            get { return _jiraReferenceEditable; }
            set
            {
                _jiraReferenceEditable = value;
                RaisePropertyChanged();
            }
        }

        public DateTime MinDate
        {
            get { return _minDate; }
            set
            {
                _minDate = value;
                RaisePropertyChanged();
            }
        }

        public DateTime MaxDate
        {
            get { return _maxDate; }
            set
            {
                _maxDate = value;
                RaisePropertyChanged();
            }
        }

        public DateTime? StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                RaisePropertyChanged();
                StartDateChanged();
            }
        }

        public DateTime DisplayDate
        {
            get { return _displayDate; }
            set
            {
                _displayDate = value;
                RaisePropertyChanged();
            }
        }

        public bool DateEditable
        {
            get { return _dateEditable; }
            set
            {
                _dateEditable = value;
                RaisePropertyChanged();
            }
        }

        public bool StartNow
        {
            get { return _startNow; }
            set
            {
                _startNow = value;
                RaisePropertyChanged();
            }
        }

        public bool StartNowEditable
        {
            get { return _startNowEditable; }
            set
            {
                _startNowEditable = value;
                RaisePropertyChanged();
            }
        }

        public bool AssignToMe
        {
            get { return _assignToMe; }
            set
            {
                _assignToMe = value;
                RaisePropertyChanged();
            }
        }

        public bool InProgress
        {
            get { return _inProgress; }
            set
            {
                _inProgress = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<IdleTimer> IdleTimers { get; set; }

        public RelayCommand AddCommand { get; private set; }
        
        public bool TimeEditable => IdleTimers == null || IdleTimers.Count == 0;

        public bool TempTimer
        {
            get { return tempTimer; }
            set
            {
                tempTimer = value;
                RaisePropertyChanged();
            }
        }

        public AddTimerViewModel(IBackend gallifrey, string jiraRef, DateTime? startDate, bool? enableDateChange, List<IdleTimer> idleTimers, bool? startNow,ModelHelpers modelHelpers)
        {
            this.modelHelpers = modelHelpers;
            var dateToday = DateTime.Now;

            AddCommand = new RelayCommand(AddTimer);

            JiraReference = jiraRef;
            JiraReferenceEditable = string.IsNullOrWhiteSpace(jiraRef);

            if (gallifrey.Settings.AppSettings.KeepTimersForDays > 0)
            {
                MinDate = dateToday.AddDays(gallifrey.Settings.AppSettings.KeepTimersForDays * -1);
                MaxDate = dateToday.AddDays(gallifrey.Settings.AppSettings.KeepTimersForDays);
            }
            else
            {
                MinDate = dateToday.AddDays(-300);
                MaxDate = dateToday.AddDays(300);
            }

            if (!startDate.HasValue) startDate = dateToday;

            if (startDate.Value < MinDate || startDate.Value > MaxDate)
            {
                DisplayDate = dateToday;
                StartDate = dateToday;
            }
            else
            {
                DisplayDate = startDate.Value;
                StartDate = startDate.Value;
            }

            DateEditable = !enableDateChange.HasValue || enableDateChange.Value;
            StartNow = startNow.HasValue && startNow.Value;

            if (idleTimers != null && idleTimers.Any())
            {
                var preloadTime = new TimeSpan();
                preloadTime = idleTimers.Aggregate(preloadTime, (current, idleTimer) => current.Add(idleTimer.IdleTimeValue));
                StartHours = preloadTime.Hours > 9 ? 9 : preloadTime.Hours;
                StartMinutes = preloadTime.Minutes;
                IdleTimers = new ObservableCollection<IdleTimer>();

                foreach (var idleTimer in idleTimers)
                {
                    IdleTimers.Add(idleTimer);
                }
            }
        }

        private void StartDateChanged()
        {
            if (StartDate.HasValue && StartDate.Value.Date != DateTime.Now.Date)
            {
                SetStartNowEnabled(false);
            }
            else
            {
                SetStartNowEnabled(true);
            }
        }

        private async void AddTimer()
        {
            if (!StartDate.HasValue)
            {
                await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Missing Date", "You Must Enter A Start Date");
                return;
            }

            if (StartDate.Value < MinDate || StartDate.Value > MaxDate)
            {
                await
                    DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Date",
                        $"You Must Enter A Start Date Between {MinDate.ToShortDateString()} And {MaxDate.ToShortDateString()}");
                return;
            }
            TimeSpan seedTime;
            if (TimeEditable)
            {
                seedTime = new TimeSpan(StartHours ?? 0, StartMinutes ?? 0, 0);
            }
            else
            {
                seedTime = new TimeSpan();
            }
            Issue jiraIssue = null;

            if (!TempTimer)
            {
                try
                {
                    jiraIssue = modelHelpers.Gallifrey.JiraConnection.GetJiraIssue(JiraReference);
                }
                catch (NoResultsFoundException)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Invalid Jira", "Unable To Locate The Jira");
                    return;
                }

                if (JiraReferenceEditable)
                {
                    var result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Correct Jira?", $"Jira found!\n\nRef: {jiraIssue.key}\nName: {jiraIssue.fields.summary}\n\nIs that correct?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                    if (result == MessageDialogResult.Negative)
                    {
                        return;
                    }
                }
            }
            try
            {
                if (TempTimer)
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
                    NewTimerId = modelHelpers.Gallifrey.JiraTimerCollection.AddTempTimer(TempTimerDescription, StartDate.Value, seedTime, StartNow);
                }
                else
                {
                    NewTimerId = modelHelpers.Gallifrey.JiraTimerCollection.AddTimer(jiraIssue, StartDate.Value, seedTime, StartNow);
                }
                AddedTimer = true;
                if (!TimeEditable)
                {
                    modelHelpers.Gallifrey.JiraTimerCollection.AddIdleTimer(NewTimerId, IdleTimers.ToList());
                }
            }
            catch (DuplicateTimerException ex)
            {
                var doneSomething = false;
                if (seedTime.TotalMinutes > 0 || !TimeEditable)
                {
                    var result = await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Duplicate Timer", "The Timer Already Exists, Would You Like To Add The Time?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings { AffirmativeButtonText = "Yes", NegativeButtonText = "No", DefaultButtonFocus = MessageDialogResult.Affirmative });

                    if (result == MessageDialogResult.Affirmative)
                    {
                        if (TimeEditable)
                        {
                            modelHelpers.Gallifrey.JiraTimerCollection.AdjustTime(ex.TimerId, seedTime.Hours, seedTime.Minutes, true);
                        }
                        else
                        {
                            modelHelpers.Gallifrey.JiraTimerCollection.AddIdleTimer(ex.TimerId, IdleTimers.ToList());
                        }

                        doneSomething = true;
                    }
                    else
                    {
                        return;
                    }
                }

                if (StartNow)
                {
                    modelHelpers.Gallifrey.JiraTimerCollection.StartTimer(ex.TimerId);
                    doneSomething = true;
                }

                if (!doneSomething)
                {
                    await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Duplicate Timer", "This Timer Already Exists!");
                    return;
                }

                NewTimerId = ex.TimerId;
            }

            if (!TempTimer)
            {
                if (AssignToMe)
                {
                    try
                    {
                        modelHelpers.Gallifrey.JiraConnection.AssignToCurrentUser(JiraReference);
                    }
                    catch (JiraConnectionException)
                    {
                        await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Assign Jira Error", "Unable To Locate Assign Jira To Current User");
                    }
                }

                if (InProgress)
                {
                    try
                    {
                        modelHelpers.Gallifrey.JiraConnection.SetInProgress(JiraReference);
                    }
                    catch (StateChangedException)
                    {
                        await DialogCoordinator.Instance.ShowMessageAsync(modelHelpers.DialogContext, "Error Changing Status", "Unable To Set Issue As In Progress");
                    }
                }
            }

            AddedTimer = true;
            modelHelpers.HideAllFlyouts();
            modelHelpers.RefreshModel();
        }

        public bool AddedTimer { get; set; }

        public Guid NewTimerId { get; set; }


        public int? StartHours
        {
            get { return startHours; }
            set
            {
                startHours = value ?? 0;
                RaisePropertyChanged();
            }
        }

        public int? StartMinutes
        {
            get { return startMinutes; }
            set
            {
                var newValue = value ?? 0;

                if (newValue < 0)
                {
                    if (StartHours == 0)
                    {
                        startMinutes = 0;
                    }
                    else
                    {
                        startMinutes = 60 + newValue;
                        StartHours--;
                    }
                }
                else if (value >= 60)
                {
                    if (StartHours == 9)
                    {
                        startMinutes = 59;
                    }
                    else
                    {
                        StartHours++;
                        startMinutes = newValue - 60;
                    }
                }
                else
                {
                    startMinutes = newValue;
                }
            }
        }

        public void SetStartNowEnabled(bool enabled)
        {
            if (enabled)
            {
                StartNowEditable = true;
            }
            else
            {
                StartNow = false;
                StartNowEditable = false;
            }
            
        }

        public void SetJiraReference(string jiraRef)
        {
            JiraReference = jiraRef;
            JiraReferenceEditable = false;
        }
    }
}