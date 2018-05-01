using Gallifrey.Serialization;
using Gallifrey.Settings;
using System;
using System.Linq;

namespace Gallifrey.Versions
{
    public interface IPremiumChecker
    {
        bool CheckIfPremium(ISettingsCollection settingsCollection);
    }

    public class PremiumChecker : IPremiumChecker
    {
        public bool CheckIfPremium(ISettingsCollection settingsCollection)
        {
            try
            {
                using (var wc = new System.Net.WebClient())
                {
                    var webContents = wc.DownloadString("https://releases.gallifreyapp.co.uk/download/PremiumInstanceIds");
                    var descryptedContents = DataEncryption.Decrypt(webContents);
                    var lines = descryptedContents.Split('\n');
                    return lines.Select(GetPremiumHash).Any(premiumHash => premiumHash == settingsCollection.InstallationHash || premiumHash == $"user-{settingsCollection.UserHash}" || premiumHash == $"site-{settingsCollection.SiteHash}");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetPremiumHash(string fileLine)
        {
            var trimedLine = fileLine.Trim().ToLower();
            return trimedLine.Contains(" ") ? trimedLine.Substring(0, trimedLine.IndexOf(" ", StringComparison.Ordinal)).Trim() : trimedLine;
        }
    }
}