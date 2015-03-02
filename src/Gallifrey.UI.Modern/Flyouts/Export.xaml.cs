using Gallifrey.UI.Modern.Models;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class Export
    {
        private readonly MainViewModel viewModel;

        public Export(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
            InitializeComponent();
        }
    }
}
