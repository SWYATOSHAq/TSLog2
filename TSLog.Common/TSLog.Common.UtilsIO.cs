using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.Common
{
    public static class UtilsIO
    {
        private static readonly string WMS_FOLDER = @"Z:\Temp\test_not_WMS_IN";
        public static void WriteFileToWMSINFolder(string fileContent, string transactionNumber)
        {
            var fileName = $"WMS_{DateTime.Now:yyyyMMdd HHmmss}_{transactionNumber}.EIN";

            string pathString = System.IO.Path.Combine(WMS_FOLDER, fileName);

            System.IO.File.WriteAllText(pathString, fileContent);

        }

    }
}
