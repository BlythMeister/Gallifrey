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
using Gallifrey.UI.Modern.ViewModels;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    public partial class AddTimer
    {
        private readonly ModelHelpers modelHelpers;
        private readonly DateTime? startDate;
        private AddTimerViewModel DataModel => (AddTimerViewModel)DataContext;
        
        public AddTimer(ModelHelpers modelHelpers, string jiraRef = null, DateTime? startDate = null, bool? enableDateChange = null, List<IdleTimer> idleTimers = null, bool? startNow = null)
        {
            this.modelHelpers = modelHelpers;
            this.startDate = startDate;
            InitializeComponent();

            DataContext = new AddTimerViewModel(modelHelpers.Gallifrey, jiraRef, startDate, enableDateChange, idleTimers, startNow,modelHelpers);
        }

        //TODO: need to do a bit more MVVM to make this work.
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
        
    }
}
