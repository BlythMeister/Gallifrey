using System.Diagnostics;
using System.Linq;
using System.Windows;
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

        private void EmailButton(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("mailto:contact@gallifreyapp.co.uk?subject=Gallifrey App Contact"));
            e.Handled = true;
        }

        private void TwitterButton(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://twitter.com/GallifreyApp"));
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
                DialogCoordinator.Instance.ShowMessageAsync(viewModel, "No Change Log", "There Is No Change Log To Show");
            }
        }

        private void PayPalButton(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=G3MWL8E6UG4RS"));
            e.Handled = true;
        }

        private void GitHubButton(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/BlythMeister/Gallifrey"));
            e.Handled = true;
        }
    }
}
