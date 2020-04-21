using System.Windows.Media;

namespace Gallifrey.UI.Modern.Models
{
    public class AccentThemeModel
    {
        public string DisplayName => $"{AccentColour} ({BaseColour})";
        public string Name { get; set; }
        public string BaseColour => Name.Split('.')[0];
        public string AccentColour => Name.Split('.')[1];
        public Brush Colour { get; set; }
    }
}
