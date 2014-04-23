using Gallifrey.Settings;

namespace Gallifrey.JiraIntergration
{
    public class JiraConnection
    {
        private readonly AppSettings appSettings;

        public JiraConnection(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }
    }
}
