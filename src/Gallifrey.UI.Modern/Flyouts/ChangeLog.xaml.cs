using Gallifrey.UI.Modern.Models;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for ChangeLog.xaml
    /// </summary>
    public partial class ChangeLog
    {
        private readonly MainViewModel viewModel;

        public ChangeLog(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
            InitializeComponent();
        }
    }
}
