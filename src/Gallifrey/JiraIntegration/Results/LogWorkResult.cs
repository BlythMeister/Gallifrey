namespace Gallifrey.JiraIntegration
{
    public class LogWorkResult
    {
        public bool Result { get; private set; }
        public string ErrorMessage { get; private set; }

        public LogWorkResult(bool result, string errorMessage)
        {
            Result = result;
            ErrorMessage = errorMessage;
        }
    }
}
