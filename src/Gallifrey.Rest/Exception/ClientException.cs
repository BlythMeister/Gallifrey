namespace Gallifrey.Rest.Exception
{
    public class ClientException : System.Exception
    {
        public ClientException(string message)
            : base(message)
        {

        }

        public ClientException(string message, System.Exception innerException)
            : base(message, innerException)
        {

        }
    }
}