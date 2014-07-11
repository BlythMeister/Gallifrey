namespace Gallifrey.Settings
{
    public interface IUiSettings
    {
        int Height { get; set; }
        int Width { get; set; }
        bool AlwaysOnTop { get; set; }
    }

    public class UiSettings : IUiSettings
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public bool AlwaysOnTop { get; set; }

        public UiSettings()
        {
            AlwaysOnTop = true;
        }
    }
}
