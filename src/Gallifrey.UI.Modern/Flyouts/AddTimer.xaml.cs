using Gallifrey.UI.Modern.Models;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for AddTimer.xaml
    /// </summary>
    public partial class AddTimer
    {
        private readonly MainViewModel viewModel;

        public AddTimer(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
            InitializeComponent();
        }
    }
}
