using Gallifrey.UI.Modern.Models;
using System.Windows.Input;

namespace Gallifrey.UI.Modern.MainViews
{
    public partial class ToastNotificationDisplayPart
    {
        public ToastNotificationDisplayPart(ToastNotification notification)
        {
            InitializeComponent();
            DataContext = notification;
            Notification = notification;
        }

        private void ToastNotificationDisplayPart_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Notification.Close();
        }
    }
}
