using System;
using System.Threading;
using System.Threading.Tasks;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Modern.Helpers
{
    public class JiraHelper
    {
        private readonly DialogContext dialogContext;

        public JiraHelper(DialogContext dialogContext)
        {
            this.dialogContext = dialogContext;
        }

        public Task<JiraHelperResult<bool>> Do(Action jiraAction, string message, bool canCancel, bool throwErrors)
        {
            var jiraFunc = new Func<bool>(() =>
            {
                try
                {
                    jiraAction.Invoke();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });
            return Do(jiraFunc, message, canCancel, throwErrors);
        }

        public async Task<JiraHelperResult<T>> Do<T>(Func<T> jiraAction, string message, bool canCancel, bool throwErrors)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            ProgressDialogController controller = null;

            try
            {
                JiraHelperResult<T> result;
                controller = await DialogCoordinator.Instance.ShowProgressAsync(dialogContext, "Please Wait", message, canCancel);
                var controllerCancel = Task.Factory.StartNew(() =>
                {
                    while (!controller.IsCanceled)
                    {

                    }
                });

                var jiraDownloadTask = new Task<T>(jiraAction, cancellationTokenSource.Token);
                jiraDownloadTask.Start();
                if (await Task.WhenAny(jiraDownloadTask, controllerCancel) == controllerCancel)
                {
                    cancellationTokenSource.Cancel();
                    result = JiraHelperResult<T>.GetCancelled();
                }
                else
                {
                    if (jiraDownloadTask.Status == TaskStatus.RanToCompletion)
                    {
                        result = JiraHelperResult<T>.GetSuccess(jiraDownloadTask.Result);
                    }
                    else
                    {
                        result = JiraHelperResult<T>.GetErrored();
                    }
                }

                return result;
            }
            catch (Exception)
            {
                if (throwErrors)
                {
                    throw;
                }

                return JiraHelperResult<T>.GetErrored();
            }
            finally
            {
                cancellationTokenSource.Cancel();

                if (controller != null)
                {
                    await controller.CloseAsync();
                }
            }
        }
    }
}