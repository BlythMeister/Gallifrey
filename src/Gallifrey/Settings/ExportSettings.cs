namespace Gallifrey.Settings
{
    public interface IExportSettings
    {
        DefaultRemaining DefaultRemainingValue { get; set; }
        string ExportCommentPrefix { get; set; }
        string EmptyExportComment { get; set; }
    }

    public class ExportSettings : IExportSettings
    {
        public DefaultRemaining DefaultRemainingValue { get; set; }
        public string ExportCommentPrefix { get; set; }
        public string EmptyExportComment { get; set; }

        public ExportSettings()
        {
            DefaultRemainingValue = DefaultRemaining.Set;
            ExportCommentPrefix = "Gallifrey";
            EmptyExportComment = "No Comment Entered";
        }
    }
}