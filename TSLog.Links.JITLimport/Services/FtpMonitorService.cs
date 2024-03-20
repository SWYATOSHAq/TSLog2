using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TSLog.Links.JITLimport.Services;

internal class FtpMonitorService
{
    private readonly string _ftpServer;
    private readonly string _ftpUsername;
    private readonly string _ftpPassword;
    private readonly string _remotePath;
    internal static readonly string ARCHIEVE_PATH = "/JTIL/arch_in/";

    public FtpMonitorService(string ftpServer, string ftpUsername, string ftpPassword, string remotePath)
    {
        _ftpServer = ftpServer;
        _ftpUsername = ftpUsername;
        _ftpPassword = ftpPassword;
        _remotePath = remotePath;
    }

    public IEnumerable<string> CheckForXlsFiles()
    {
        var fileList = new List<string>();
        try
        {
            using var ftpClient = new FtpClient(_ftpServer, _ftpUsername, _ftpPassword);

            ftpClient.Connect();

            var items = ftpClient.GetListing(_remotePath);

            foreach (var item in items)
            {
                if (item.Type == FtpObjectType.File && item.Name.EndsWith(".xls"))
                {
                    fileList.Add(item.FullName);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
        }
        return fileList;
    }

    public async Task DownloadFilesAsync(IEnumerable<string> fileList)
    {
        var token = new CancellationToken();
        using (var ftp = new AsyncFtpClient(_ftpServer, _ftpUsername, _ftpPassword))
        {
            await ftp.Connect(token);

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string downloadedFilesDir = Path.Combine(baseDir, "DownloadedXlsFiles");

            // Download many files, skip if they already exist on disk.
            await ftp.DownloadFiles(downloadedFilesDir, fileList, FtpLocalExists.Skip, token: token);
        }
    }

    public void MoveRemoteFilesToArchive(string archivePath) 
    {
        using (var conn = new FtpClient(_ftpServer, _ftpUsername, _ftpPassword))
        {
            conn.Connect();
            var files = conn.GetListing(_remotePath);
            foreach (var item in files)
            {
                // Construct the full source path.
                var sourceFilePath = item.FullName;
                // Construct the destination path with the same file name in the archive directory.
                var destFilePath = Path.Combine(archivePath, item.Name).Replace("\\", "/");
                conn.MoveFile(sourceFilePath, destFilePath, FtpRemoteExists.Skip);
            }            
        }
    }
}