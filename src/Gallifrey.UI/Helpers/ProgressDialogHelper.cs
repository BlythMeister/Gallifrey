using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;

namespace Gallifrey.UI.Helpers
{
    public class ProgressDialogHelper
    {
        private readonly DialogContext dialogContext;

        public ProgressDialogHelper(DialogContext dialogContext)
        {
            this.dialogContext = dialogContext;
        }

        public Task<ProgressResult<bool>> Do(Action action, string message, bool canCancel, bool throwErrors)
        {
            return Do(controller =>
            {
                action();
                return true;
            }, message, canCancel, throwErrors);
        }

        public Task<ProgressResult<bool>> Do(Action<ProgressDialogController> action, string message, bool canCancel, bool throwErrors)
        {
            return Do(controller =>
            {
                action(controller);
                return true;
            }, message, canCancel, throwErrors);
        }

        public async Task<ProgressResult<T>> Do<T>(Func<T> function, string message, bool canCancel, bool throwErrors)
        {
            return await Do(controller => function(), message, canCancel, throwErrors);
        }

        public async Task<ProgressResult<T>> Do<T>(Func<ProgressDialogController, T> function, string message, bool canCancel, bool throwErrors)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var controller = await DialogCoordinator.Instance.ShowProgressAsync(dialogContext, "Please Wait", message, canCancel);
            controller.SetIndeterminate();

            var controllerCancel = Task.Factory.StartNew(delegate
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

            try
            {
                var functionTask = Task.Factory.StartNew(() => function.Invoke(controller), cancellationTokenSource.Token);

                ProgressResult<T> result;
                if (await Task.WhenAny(functionTask, controllerCancel) == controllerCancel)
                {
                    cancellationTokenSource.Cancel();
                    result = ProgressResult.GetCancelled<T>();
                }
                else
                {
                    if (functionTask.Status == TaskStatus.RanToCompletion)
                    {
                        result = ProgressResult.GetSuccess(functionTask.Result);
                    }
                    else
                    {
                        result = ProgressResult.GetErrored<T>();

                        if (throwErrors && functionTask.Exception != null)
                        {
                            ExceptionDispatchInfo.Capture(functionTask.Exception.InnerException).Throw();
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

                return ProgressResult.GetErrored<T>();
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