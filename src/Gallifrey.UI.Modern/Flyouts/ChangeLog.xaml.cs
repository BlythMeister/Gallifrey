using System;
using System.Collections.Generic;
using Gallifrey.ChangeLog;
using Gallifrey.UI.Modern.Models;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for ChangeLog.xaml
    /// </summary>
    public partial class ChangeLog
    {
        private readonly MainViewModel viewModel;
        
        public ChangeLog(MainViewModel viewModel, IEnumerable<ChangeLogVersion> changeLog)
        {
            this.viewModel = viewModel;
            InitializeComponent();
            DataContext = new ChangeLogModel(changeLog);
        }
    }
}
