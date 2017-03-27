using System;
using System.Linq;
using Gallifrey.Serialization;

namespace Gallifrey.Versions
{
    public interface IPremiumChecker
    {
        bool CheckIfPremium(string installtionHash);
    }

    public class PremiumChecker : IPremiumChecker
    {
        public bool CheckIfPremium(string installtionHash)
        {
            try
            {
                string webContents;
                using (var wc = new System.Net.WebClient())
                    webContents = wc.DownloadString("http://releases.gallifreyapp.co.uk/download/PremiumInstanceIds");

                var descryptedContents = DataEncryption.Decrypt(webContents);
                var lines = descryptedContents.Split('\n');
                return lines.Select(GetPremiumHash).Any(premiumHash => premiumHash == installtionHash);
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