using Gallifrey.ChangeLog;
using Gallifrey.UI.Modern.Models;
using System.Collections.Generic;

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
