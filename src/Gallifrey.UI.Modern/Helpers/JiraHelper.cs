using System;
using System.Threading;
using System.Threading.Tasks;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Helpers
{
    public static class JiraHelper
    {
        public static async Task<JiraHelperResult<T>> Do<T>(Func<T> jiraAction, MainViewModel viewModel, string message, bool canCancel = false)
        {
            JiraHelperResult<T> result;
            var cancellationTokenSource = new CancellationTokenSource();
            var jiraDownloadTask = Task.Factory.StartNew(jiraAction, cancellationTokenSource.Token);

            var controller = await DialogCoordinator.Instance.ShowProgressAsync(viewModel, "Please Wait", message, canCancel);
            var controllerCancel = Task.Factory.StartNew(() =>
            {
                while (!controller.IsCanceled)
                {

                }
            });

            if (await Task.WhenAny(jiraDownloadTask, controllerCancel) == controllerCancel)
            {
                cancellationTokenSource.Cancel();
                result = JiraHelperResult<T>.GetCancelled();
            }
            else
            {
                result = JiraHelperResult<T>.GetSuccess(jiraDownloadTask.Result);
            }

            await controller.CloseAsync();

            return result;
        }
    }
}