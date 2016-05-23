using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Gallifrey.Settings;
using Gallifrey.Versions;
using MahApps.Metro;
using Microsoft.Win32;

namespace Gallifrey.UI.Modern.Models
{
    public class SettingModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        readonly RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        //AppSettings
        public bool AlertWhenIdle { get; set; }
        public int AlertMinutes { get; set; }
        public int KeepTimersForDays { get; set; }
        public bool AutoUpdate { get; set; }
        public bool AllowTracking { get; set; }
        public int TargetHoursPerDay { get; set; }
        public int TargetMinutesPerDay { get; set; }
        public string StartOfWeek { get; set; }
        public List<WorkingDay> WorkingDays { get; set; }

        //UI Settings
        public AccentThemeModel Theme { get; set; }
        public AccentThemeModel Accent { get; set; }
        public bool StartOnBoot { get; set; }

        //Jira Settings
        public string JiraUrl { get; set; }
        public string JiraUsername { get; set; }
        public string JiraPassword { get; set; }

        //Export Settings
        public bool ExportAll { get; set; }
        public List<ExportPrompt> ExportPrompts { get; set; }
        public List<RemainingAdjustmentValue> RemainingAdjustmentValues { get; set; }
        public RemainingAdjustmentValue SelectedRemainingAdjustmentValue { get; set; }
        public string CommentPrefix { get; set; }
        public string DefaultComment { get; set; }

        //Static Data
        public List<AccentThemeModel> AvaliableThemes { get; set; }
        public List<AccentThemeModel> AvaliableAccents { get; set; }

        //Data Change Flags
        public bool JiraSettingsChanged { get; set; }

        public SettingModel(ISettingsCollection settings, IVersionControl versionControl)
        {
            //Static Data
            AvaliableThemes = ThemeManager.AppThemes.Select(x => new AccentThemeModel { Name = x.Name.Replace("Base", ""), Colour = x.Resources["WhiteColorBrush"] as Brush, BorderColour = x.Resources["BlackColorBrush"] as Brush }).ToList();
            AvaliableAccents = ThemeManager.Accents.Select(x => new AccentThemeModel { Name = x.Name, Colour = x.Resources["AccentColorBrush"] as Brush, BorderColour = x.Resources["AccentColorBrush"] as Brush }).ToList();

            //AppSettings
            AlertWhenIdle = settings.AppSettings.AlertWhenNotRunning;
            AlertMinutes = (settings.AppSettings.AlertTimeMilliseconds / 1000 / 60);
            KeepTimersForDays = settings.AppSettings.KeepTimersForDays;
            AutoUpdate = settings.AppSettings.AutoUpdate;
            AllowTracking = settings.AppSettings.UsageTracking;
            TargetHoursPerDay = settings.AppSettings.TargetLogPerDay.Hours;
            TargetMinutesPerDay = settings.AppSettings.TargetLogPerDay.Minutes;
            StartOfWeek = settings.AppSettings.StartOfWeek.ToString();

            WorkingDays = new List<WorkingDay>();
            foreach (DayOfWeek dayOfWeek in Enum.GetValues(typeof(DayOfWeek)))
            {
                var isChecked = settings.AppSettings.ExportDays.Contains(dayOfWeek);
                WorkingDays.Add(new WorkingDay(isChecked, dayOfWeek));
            }

            //UI Settings
            Theme = AvaliableThemes.FirstOrDefault(x => x.Name == settings.UiSettings.Theme) ?? AvaliableThemes.First();
            Accent = AvaliableAccents.FirstOrDefault(x => x.Name == settings.UiSettings.Accent) ?? AvaliableAccents.First();
            StartOnBoot = versionControl.IsAutomatedDeploy && registryKey.GetValue(versionControl.AppName) != null;

            //Jira Settings
            JiraUrl = settings.JiraConnectionSettings.JiraUrl;
            JiraUsername = settings.JiraConnectionSettings.JiraUsername;
            JiraPassword = settings.JiraConnectionSettings.JiraPassword;

            //Export Settings
            ExportAll = settings.ExportSettings.ExportPromptAll;
            ExportPrompts = new List<ExportPrompt>
            {
                new ExportPrompt("Idle", settings.ExportSettings.ExportPrompt.OnAddIdle, "Add Idle Time"),
                new ExportPrompt("Manual", settings.ExportSettings.ExportPrompt.OnManualAdjust, "Manual Timer Adjustment"),
                new ExportPrompt("Stop", settings.ExportSettings.ExportPrompt.OnStop, "Stop Timer"),
                new ExportPrompt("Pre", settings.ExportSettings.ExportPrompt.OnCreatePreloaded, "Add Pre-Loaded Timer"),
            };

            RemainingAdjustmentValues = new List<RemainingAdjustmentValue>
            {
                new RemainingAdjustmentValue(DefaultRemaining.Auto, "Adjust Automatically"),
                new RemainingAdjustmentValue(DefaultRemaining.Leave, "Leave Remaining"),
                new RemainingAdjustmentValue(DefaultRemaining.Set, "Set Value")
            };
            SelectedRemainingAdjustmentValue = RemainingAdjustmentValues.First(x => x.Remaining == settings.ExportSettings.DefaultRemainingValue);
            CommentPrefix = settings.ExportSettings.ExportCommentPrefix;
            DefaultComment = settings.ExportSettings.EmptyExportComment;
        }

        public void UpdateSettings(ISettingsCollection settings, IVersionControl versionControl)
        {
            //AppSettings
            settings.AppSettings.AlertWhenNotRunning = AlertWhenIdle;
            settings.AppSettings.AlertTimeMilliseconds = AlertMinutes * 60 * 1000;
            settings.AppSettings.KeepTimersForDays = KeepTimersForDays;
            settings.AppSettings.AutoUpdate = AutoUpdate;
            settings.AppSettings.UsageTracking = AllowTracking;
            settings.AppSettings.TargetLogPerDay = new TimeSpan(TargetHoursPerDay, TargetMinutesPerDay, 0);
            settings.AppSettings.ExportDays = WorkingDays.Where(x => x.IsChecked).Select(x => x.DayOfWeek).ToList();
            settings.AppSettings.StartOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), StartOfWeek, true);

            //UI Settings
            settings.UiSettings.Theme = Theme.Name;
            settings.UiSettings.Accent = Accent.Name;
            if (versionControl.IsAutomatedDeploy)
            {
                if (StartOnBoot)
                {
                    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Gallifrey", $"{versionControl.AppName}.appref-ms");
                    registryKey.SetValue(versionControl.AppName, path);
                }
                else
                {
                    registryKey.DeleteValue(versionControl.AppName, false);
                }
            }

            //Jira Settings
            if (settings.JiraConnectionSettings.JiraUrl != JiraUrl ||
                settings.JiraConnectionSettings.JiraUsername != JiraUsername ||
                settings.JiraConnectionSettings.JiraPassword != JiraPassword)
            {
                JiraSettingsChanged = true;
            }
            else if (string.IsNullOrWhiteSpace(JiraUrl) || string.IsNullOrWhiteSpace(JiraUsername) || string.IsNullOrWhiteSpace(JiraPassword))
            {
                JiraSettingsChanged = true;
            }

            settings.JiraConnectionSettings.JiraUrl = JiraUrl;
            settings.JiraConnectionSettings.JiraUsername = JiraUsername;
            settings.JiraConnectionSettings.JiraPassword = JiraPassword;

            //Export Settings
            settings.ExportSettings.ExportPrompt.OnAddIdle = ExportPrompts.First(x => x.Key == "Idle").IsChecked;
            settings.ExportSettings.ExportPrompt.OnManualAdjust = ExportPrompts.First(x => x.Key == "Manual").IsChecked;
            settings.ExportSettings.ExportPrompt.OnStop = ExportPrompts.First(x => x.Key == "Stop").IsChecked;
            settings.ExportSettings.ExportPrompt.OnCreatePreloaded = ExportPrompts.First(x => x.Key == "Pre").IsChecked;
            settings.ExportSettings.ExportPromptAll = ExportAll;
            settings.ExportSettings.DefaultRemainingValue = SelectedRemainingAdjustmentValue.Remaining;
            settings.ExportSettings.ExportCommentPrefix = CommentPrefix;
            settings.ExportSettings.EmptyExportComment = DefaultComment;
        }

        public class WorkingDay
        {
            public bool IsChecked { get; set; }
            public DayOfWeek DayOfWeek { get; set; }
            public string DisplayName => DayOfWeek.ToString();

            public WorkingDay(bool isChecked, DayOfWeek dayOfWeek)
            {
                IsChecked = isChecked;
                DayOfWeek = dayOfWeek;
            }
        }

        public class ExportPrompt
        {
            public string Key { get; set; }
            public bool IsChecked { get; set; }
            public string DisplayName { get; set; }

            public ExportPrompt(string key, bool isChecked, string displayName)
            {
                Key = key;
                IsChecked = isChecked;
                DisplayName = displayName;
            }
        }

        public class RemainingAdjustmentValue
        {
            public DefaultRemaining Remaining { get; set; }
            public string DisplayName { get; set; }

            public RemainingAdjustmentValue(DefaultRemaining remaining, string displayName)
            {
                Remaining = remaining;
                DisplayName = displayName;
            }

            public override string ToString()
            {
                return DisplayName;
            }
        }

        public void EnableTracking()
        {
            AllowTracking = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AllowTracking"));
        }
    }
}