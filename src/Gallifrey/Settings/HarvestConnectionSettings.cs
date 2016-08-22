namespace Gallifrey.Settings
{
    public interface IHarvestConnectionSettings
    {
        string HarvestUrl { get; set; }
        string HarvestJiraUsername { get; set; }
        string HarvestJiraPassword { get; set; }
    }

    public class HarvestConnectionSettings : IHarvestConnectionSettings
    {
        public string HarvestUrl { get; set; }
        public string HarvestJiraUsername { get; set; }
        public string HarvestJiraPassword { get; set; }
    }
}