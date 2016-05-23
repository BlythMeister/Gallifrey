using System.Collections.Generic;
using Gallifrey.ChangeLog;
using Gallifrey.UI.Models;

namespace Gallifrey.UI.Flyouts
{
    public partial class ChangeLog
    {
        public ChangeLog(IEnumerable<ChangeLogVersion> changeLog)
        {
            InitializeComponent();
            DataContext = new ChangeLogModel(changeLog);
        }
    }
}
