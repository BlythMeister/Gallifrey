using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
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
                    if (throwErrors)
                    {
                        throw;
                    }
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
                var jiraDownloadTask = new Task<T>(jiraAction, cancellationTokenSource.Token);

                JiraHelperResult<T> result;
                controller = await DialogCoordinator.Instance.ShowProgressAsync(dialogContext, "Please Wait", message, canCancel);
                var controllerCancel = Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        if (!controller.IsOpen)
                        {
                            break;
                        }

                        if (controller.IsCanceled)
                        {
                            break;
                        }
                        Thread.Sleep(100);
                    }
                }, cancellationTokenSource.Token);

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

                        if (throwErrors && jiraDownloadTask.Exception != null)
                        {
                            ExceptionDispatchInfo.Capture(jiraDownloadTask.Exception.InnerException).Throw();
                        }
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