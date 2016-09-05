namespace Gallifrey.Harvest.Model
{
    public class DayEntry
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public string spent_at { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string project_id { get; set; }
        public string task_id { get; set; }
        public string project { get; set; }
        public string task { get; set; }
        public string client { get; set; }
        public string notes { get; set; }
        public double hours { get; set; }
    }
}