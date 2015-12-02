namespace Gallifrey.Settings
{
    public interface IUiSettings
    {
        int Height { get; set; }
        int Width { get; set; }
        bool AlwaysOnTop { get; set; }
        string Theme { get; set; }
        string Accent { get; set; }
        bool SetDefaults();
    }

    public class UiSettings : IUiSettings
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public bool AlwaysOnTop { get; set; }
        public string Theme { get; set; }
        public string Accent { get; set; }
        
        public UiSettings()
        {
            AlwaysOnTop = true;
        }

        public bool SetDefaults()
        {
            var setANewDefault = false;

            if (string.IsNullOrWhiteSpace(Theme))
            {
                setANewDefault = true;
                Theme = "Dark";
            }

            if (string.IsNullOrWhiteSpace(Accent))
            {
                setANewDefault = true;
                Accent = "Blue";
            }

            return setANewDefault;
        }
    }
}
