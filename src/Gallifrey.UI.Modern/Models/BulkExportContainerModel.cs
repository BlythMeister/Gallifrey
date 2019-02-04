using System.Collections.ObjectModel;

namespace Gallifrey.UI.Modern.Models
{
    public class BulkExportContainerModel
    {
        public ObservableCollection<BulkExportModel> BulkExports { get; set; }

        public BulkExportContainerModel()
        {
            BulkExports = new ObservableCollection<BulkExportModel>();
        }
    }
}
