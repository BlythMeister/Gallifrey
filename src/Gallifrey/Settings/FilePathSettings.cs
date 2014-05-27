using System;
using System.IO;

namespace Gallifrey.Settings
{
    internal static class FilePathSettings
    {
        internal static string DataSavePath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gallifrey"); }
        }
    }
}
