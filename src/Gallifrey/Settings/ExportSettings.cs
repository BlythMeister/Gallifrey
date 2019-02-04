using Newtonsoft.Json;
using System.ComponentModel;

namespace Gallifrey.Settings
{
    public interface IExportSettings
    {
        DefaultRemaining DefaultRemainingValue { get; set; }
        string ExportCommentPrefix { get; set; }
        string EmptyExportComment { get; set; }
        ExportPrompt ExportPrompt { get; set; }
        bool ExportPromptAll { get; set; }
        bool TrackingOnly { get; set; }
    }

    public class ExportSettings : IExportSettings
    {
        [DefaultValue(DefaultRemaining.Leave)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public DefaultRemaining DefaultRemainingValue { get; set; }

        [DefaultValue("Gallifrey")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string ExportCommentPrefix { get; set; }

        [DefaultValue("No Comment Entered")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public string EmptyExportComment { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public ExportPrompt ExportPrompt { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool ExportPromptAll { get; set; }

        [DefaultValue(false)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool TrackingOnly { get; set; }

        public ExportSettings()
        {
            DefaultRemainingValue = DefaultRemaining.Leave;
            ExportCommentPrefix = "Gallifrey";
            EmptyExportComment = "No Comment Entered";
            ExportPrompt = new ExportPrompt();
            ExportPromptAll = false;
            TrackingOnly = false;
        }
    }
}
