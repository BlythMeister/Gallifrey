using System;
using System.IO;

namespace Gallifrey.Settings
{
    public static class FilePathSettings
    {
        public static string DataSavePath
        {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gallifrey"); }
        }
    }
}
