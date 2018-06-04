using System;
using System.Threading.Tasks;
using System.Windows;

namespace Gallifrey.UI.Modern.Helpers
{
    public static class ClipboardHelper
    {
        private static async Task SetClipboard(string text, int attempts)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception)
            {
                if (attempts > 10)
                {
                    throw;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
                await SetClipboard(text, attempts + 1);
            }
        }

        private static async Task<string> GetClipboard(int attempts)
        {
            try
            {
                return Clipboard.GetText().Trim();
            }
            catch (Exception)
            {
                if (attempts > 10)
                {
                    throw;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
                return await GetClipboard(attempts + 1);
            }
        }

        public static async Task SetClipboard(string text)
        {
            await SetClipboard(text, 0);
        }

        public static async Task<string> GetClipboard()
        {
            return await GetClipboard(0);
        }
    }
}