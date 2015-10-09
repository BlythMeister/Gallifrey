namespace Gallifrey.UI.Modern.Helpers
{
    public class JiraHelperResult<T>
    {
        private JiraHelperResult(T retVal, bool cancelled)
        {
            RetVal = retVal;
            Cancelled = cancelled;
        }

        public T RetVal { get; private set; }
        public bool Cancelled { get; private set; }

        public static JiraHelperResult<T> GetCancelled()
        {
            return new JiraHelperResult<T>(default(T), true);
        }

        public static JiraHelperResult<T> GetSuccess(T retVal)
        {
            return new JiraHelperResult<T>(retVal, false);
        }
    }
}