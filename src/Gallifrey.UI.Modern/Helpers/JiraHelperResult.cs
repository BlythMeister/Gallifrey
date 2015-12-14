namespace Gallifrey.UI.Modern.Helpers
{
    public class JiraHelperResult<T>
    {
        private JiraHelperResult(T retVal, JiraHelperStatus status)
        {
            RetVal = retVal;
            Status = status;
        }

        public T RetVal { get; private set; }
        public JiraHelperStatus Status { get; private set; }

        public static JiraHelperResult<T> GetCancelled()
        {
            return new JiraHelperResult<T>(default(T), JiraHelperStatus.Cancelled);
        }

        public static JiraHelperResult<T> GetSuccess(T retVal)
        {
            return new JiraHelperResult<T>(retVal, JiraHelperStatus.Success);
        }

        public static JiraHelperResult<T> GetErrored()
        {
            return new JiraHelperResult<T>(default(T), JiraHelperStatus.Errored);
        }

        public enum JiraHelperStatus
        {
            Success,
            Cancelled,
            Errored
        }
    }
}