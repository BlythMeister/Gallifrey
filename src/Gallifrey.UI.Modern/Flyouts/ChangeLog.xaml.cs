using System.Collections.Generic;
using Gallifrey.ChangeLog;
using Gallifrey.UI.Modern.Models;

namespace Gallifrey.UI.Modern.Flyouts
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
