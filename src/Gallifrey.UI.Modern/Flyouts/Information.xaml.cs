using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using System.Xml.Linq;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for Information.xaml
    /// </summary>
    public partial class Information
    {
        private readonly MainViewModel viewModel;

        public Information(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
            InitializeComponent();
            DataContext = new InformationModel(viewModel.Gallifrey);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void ChangeLogButton(object sender, RoutedEventArgs e)
        {
            var changeLog = viewModel.Gallifrey.GetChangeLog(XDocument.Parse(Properties.Resources.ChangeLog));

            if (changeLog.Any())
            {
                viewModel.OpenFlyout(new ChangeLog(viewModel, changeLog));
            }
            else
            {
                DialogCoordinator.Instance.ShowMessageAsync(viewModel.DialogContext, "No Change Log", "There Is No Change Log To Show");
            }
        }
    }
}
