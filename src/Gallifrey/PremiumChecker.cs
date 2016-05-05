using System;
using System.Linq;
using Gallifrey.Serialization;

namespace Gallifrey
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
            return fileLine.Contains("~") ? fileLine.Substring(0, fileLine.IndexOf("~")).Trim().ToLower() : fileLine.Trim().ToLower();
        }
    }
}