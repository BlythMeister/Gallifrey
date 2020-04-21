using Gallifrey.Serialization;
using Gallifrey.Settings;
using System;
using System.Diagnostics;
using System.Linq;

namespace Gallifrey.Versions
{
    public class PremiumChecker
    {
        public bool CheckIfPremium(ISettingsCollection settingsCollection)
        {
            if (Debugger.IsAttached)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(ConfigKeys.PremiumEncryptionPassPhrase))
            {
                return false;
            }

            try
            {
                using (var wc = new System.Net.WebClient())
                {
                    var webContents = wc.DownloadString("https://releases.gallifreyapp.co.uk/download/PremiumInstanceIds.dat");
                    var decryptedContents = DataEncryption.Decrypt(webContents, ConfigKeys.PremiumEncryptionPassPhrase);
                    var lines = decryptedContents.Split('\n');
                    return lines.Select(GetPremiumHash).Any(premiumHash => premiumHash == $"user-{settingsCollection.UserHash}" || premiumHash == $"site-{settingsCollection.SiteHash}");
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetPremiumHash(string fileLine)
        {
            var trimmedLine = fileLine.Trim().ToLower();
            return trimmedLine.Contains(" ") ? trimmedLine.Substring(0, trimmedLine.IndexOf(" ", StringComparison.Ordinal)).Trim() : trimmedLine;
        }
    }
}
