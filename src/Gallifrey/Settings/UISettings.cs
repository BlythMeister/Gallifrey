using System.ComponentModel;
using Newtonsoft.Json;

namespace Gallifrey.Settings
{
    public interface IUiSettings
    {
        int Height { get; set; }
        int Width { get; set; }
        string Theme { get; set; }
        string Accent { get; set; }
    }

    public class UiSettings : IUiSettings
    {
        public int Height { get; set; }
        public int Width { get; set; }

        [DefaultValue("Dark")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Theme { get; set; }

        [DefaultValue("Blue")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string Accent { get; set; }
        
        public UiSettings()
        {
            Theme = "Dark";
            Accent = "Blue";
        }
    }
}
