using Gallifrey.UI.Modern.Models;
using System.Windows.Input;
using ToastNotifications.Core;

namespace Gallifrey.UI.Modern.MainViews
{
    public partial class ToastNotificationDisplayPart : NotificationDisplayPart
    {
        public ToastNotificationDisplayPart(ToastNotification notification)
        {
            InitializeComponent();
            DataContext = notification;
        }

        private void ToastNotificationDisplayPart_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            ((ToastNotification)DataContext).Close();
        }
    }
}
