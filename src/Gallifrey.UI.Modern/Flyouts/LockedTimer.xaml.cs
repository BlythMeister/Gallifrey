using Gallifrey.UI.Modern.Models;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for LockedTimer.xaml
    /// </summary>
    public partial class LockedTimer
    {
        private readonly MainViewModel viewModel;

        public LockedTimer(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
            InitializeComponent();
        }
    }
}
