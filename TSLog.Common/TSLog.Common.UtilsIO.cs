using System;
using System.IO;

namespace TSLog.Common
{
    public static class UtilsIO
    {
        private static readonly string WMS_FOLDER = @"Z:\Temp\test_not_WMS_IN";

        public static void WriteFileToWMSINFolder(string fileContent, string transactionNumber)
        {
            var fileName = $"WMS_{DateTime.Now:yyyyMMdd HHmmss}_{transactionNumber}.EIN";

            string pathString = Path.Combine(WMS_FOLDER, fileName);

            File.WriteAllText(pathString, fileContent);
        }
    }
}