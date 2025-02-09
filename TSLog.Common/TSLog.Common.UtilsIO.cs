using System;
using System.IO;

namespace TSLog.Common;

/// <summary>
/// Общая функциональность для UI.
/// </summary>
public static class IOUtils
{
    private const string WmsFolder = @"Z:\Temp\test_not_WMS_IN";

    public static void WriteFileToWmsINFolder(string fileContent, string transactionNumber)
    {
        var fileName = $"WMS_{DateTime.Now:yyyyMMdd HHmmss}_{transactionNumber}.EIN";
        string pathString = Path.Combine(WmsFolder, fileName);

        File.WriteAllText(pathString, fileContent);
    }
}
