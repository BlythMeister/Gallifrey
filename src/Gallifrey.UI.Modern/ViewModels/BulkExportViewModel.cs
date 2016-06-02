using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Gallifrey.JiraTimers;
using Gallifrey.UI.Modern.Helpers;
using Gallifrey.UI.Modern.Models;

namespace Gallifrey.UI.Modern.ViewModels
{
    public class BulkExportViewModel : ViewModelBase
    {
        private readonly ModelHelpers modelHelpers;
        private readonly List<JiraTimer> timers;

        public ObservableCollection<BulkExportModel> BulkExports { get; set; }

        public BulkExportViewModel(ModelHelpers modelHelpers, List<BulkExportModel> bulkExports, List<JiraTimer> timers)
        {
            this.modelHelpers = modelHelpers;
            this.timers = timers;
            BulkExports = new ObservableCollection<BulkExportModel>(bulkExports);
        }

        public void SetupContext()
        {

        }
    }
}
