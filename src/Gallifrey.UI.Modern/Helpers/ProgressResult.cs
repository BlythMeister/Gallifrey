namespace Gallifrey.UI.Modern.Helpers
{
    public static class ProgressResult
    {
        public static ProgressResult<T> GetSuccess<T>(T retVal)
        {
            return new ProgressResult<T>(retVal, JiraHelperStatus.Success);
        }

        public static ProgressResult<T> GetCancelled<T>()
        {
            return new ProgressResult<T>(default(T), JiraHelperStatus.Cancelled);
        }

        public static ProgressResult<T> GetErrored<T>()
        {
            return new ProgressResult<T>(default(T), JiraHelperStatus.Errored);
        }

        public enum JiraHelperStatus
        {
            Success,
            Cancelled,
            Errored
        }
    }

    public class ProgressResult<T>
    {
        internal ProgressResult(T retVal, ProgressResult.JiraHelperStatus status)
        {
            RetVal = retVal;
            Status = status;
        }

        public T RetVal { get; }
        public ProgressResult.JiraHelperStatus Status { get; }
    }
}
