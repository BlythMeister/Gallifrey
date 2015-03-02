using Gallifrey.UI.Modern.Models;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for EditTimer.xaml
    /// </summary>
    public partial class EditTimer
    {
        private readonly MainViewModel viewModel;

        public EditTimer(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
            InitializeComponent();
        }
    }
}
