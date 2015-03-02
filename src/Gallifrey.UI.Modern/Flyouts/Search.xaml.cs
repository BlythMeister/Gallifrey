using Gallifrey.UI.Modern.Models;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Search
    {
        private readonly MainViewModel viewModel;

        public Search(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
            InitializeComponent();
        }
    }
}
