using System.Collections.Generic;

namespace Gallifrey.Harvest.Model
{
    public class DailyRecord
    {
        public string for_day { get; set; }
        public List<DayEntry> day_entries { get; set; }
        public List<Project> projects { get; set; }
    }
}