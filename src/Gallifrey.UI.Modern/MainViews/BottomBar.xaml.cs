using System.Diagnostics;
using System.Windows.Navigation;

namespace Gallifrey.UI.Modern.MainViews
{
    public partial class BottomBar
    {
        public BottomBar()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
