using System;
using System.Linq;
using Gallifrey.Serialization;

namespace Gallifrey.Versions
{
    public interface IPremiumChecker
    {
        bool CheckIfPremium(Guid installationId);
    }

    public class PremiumChecker : IPremiumChecker
    {
        public bool CheckIfPremium(Guid installationId)
        {
            try
            {
                string webContents;
                using (var wc = new System.Net.WebClient())
                    webContents = wc.DownloadString("http://releases.gallifreyapp.co.uk/download/PremiumInstanceIds");

                var descryptedContents = DataEncryption.Decrypt(webContents);
                var lines = descryptedContents.Split('\n');
                return lines.Any(x => GetInstallationId(x) == installationId.ToString().ToLower());
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetInstallationId(string fileLine)
        {
            var trimedLine = fileLine.Trim().ToLower();
            return trimedLine.Contains(" ") ? trimedLine.Substring(0, trimedLine.IndexOf(" ", StringComparison.Ordinal)).Trim() : trimedLine;
        }
    }
}