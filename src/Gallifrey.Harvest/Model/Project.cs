using System.Collections.Generic;

namespace Gallifrey.Harvest.Model
{
    public class Project
    {
        public int id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public List<Task> tasks { get; set; }
        public string client { get; set; }
        public int client_id { get; set; }
    }
}