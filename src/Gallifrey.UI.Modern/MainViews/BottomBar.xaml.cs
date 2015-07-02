using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Gallifrey.UI.Modern.MainViews
{
    /// <summary>
    /// Interaction logic for BottomBar.xaml
    /// </summary>
    public partial class BottomBar : UserControl
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
