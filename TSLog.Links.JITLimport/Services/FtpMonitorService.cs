using AngleSharp.Io.Dom;
using FluentFTP;
using FluentFTP.Helpers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TSLog.Links.JITLimport.Interfaces;

namespace TSLog.Links.JITLimport.Services;

internal class FtpMonitorService : IFtpMonitorService
{
    private readonly string _ftpServer;
    private readonly string _ftpUsername;
    private readonly string _ftpPassword;
    private readonly string _remotePath;
    private readonly ILogger<FtpMonitorService> _logger;
    internal static readonly string ARCHIEVE_PATH = "/JTIL/arch_in/";

    public FtpMonitorService(string ftpServer, string ftpUsername, string ftpPassword, string remotePath, ILogger<FtpMonitorService> logger)
    {
        _ftpServer = ftpServer;
        _ftpUsername = ftpUsername;
        _ftpPassword = ftpPassword;
        _remotePath = remotePath;
        _logger = logger;
    }
    [Obsolete]
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
                if (item.Type == FtpObjectType.File && (item.Name.EndsWith(".xls") || item.Name.EndsWith(".xlsx")))
                {
                    fileList.Add(item.FullName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking for XLS files in {RemotePath}", _remotePath);
        }
        return fileList;
    }

    public IEnumerable<string> CheckForJsonFiles()
    {
        var fileList = new List<string>();
        try
        {
            using var ftpClient = new FtpClient(_ftpServer, _ftpUsername, _ftpPassword);

            ftpClient.Connect();

            var items = ftpClient.GetListing(_remotePath);

            foreach (var item in items)
            {
                if (item.Type == FtpObjectType.File && (item.Name.EndsWith(".json")))
                {
                    fileList.Add(item.FullName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking for json-files in {RemotePath}", _remotePath);
        }
        return fileList;
    }

    public async Task DownloadFilesAsync(IEnumerable<string> fileList, CancellationToken cancellationToken)
    {
        if (!fileList.Any())
        {
            _logger.LogInformation("No files to download.");
            return;
        }

        try
        {
            using (var ftp = new AsyncFtpClient(_ftpServer, _ftpUsername, _ftpPassword))
            {
                await ftp.Connect(cancellationToken);

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string downloadedFilesDir = Path.Combine(baseDir, "DownloadedJsonFiles");

                foreach (var file in fileList)
                {
                    try
                    {
                        var localFilePath = Path.Combine(downloadedFilesDir, Path.GetFileName(file));
                        _logger.LogInformation("Attempting to download file: {RemoteFilePath} to {LocalFilePath}", file, localFilePath);

                        if (File.Exists(localFilePath))
                        {
                            _logger.LogInformation("File already exists locally: {LocalFilePath}", localFilePath);
                            // If you want to overwrite, uncomment the next line and change Skip to Overwrite in the download call.
                            // File.Delete(localFilePath);
                        }

                        var result = await ftp.DownloadFile(localFilePath, file, FtpLocalExists.Skip, token: cancellationToken);
                        if (result.IsSuccess())
                        {
                            _logger.LogInformation("Downloaded file: {FileName}", file);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to download file: {FileName}", file);
                        }
                        _logger.LogInformation("Downloaded file: {FileName}", file);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to download file: {FileName}", file);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while downloading files from FTP server");
        }
    }

    public void MoveRemoteFilesToArchive(string archivePath)
    {
        try
        {
            using (var conn = new FtpClient(_ftpServer, _ftpUsername, _ftpPassword))
            {
                conn.Connect();
                var files = conn.GetListing(_remotePath)
                                .Where(item => item.Type == FtpObjectType.File &&
                                               (item.Name.EndsWith(".json")))
                                .ToList();

                foreach (var item in files)
                {
                    var sourceFilePath = item.FullName;
                    var destFilePath = Path.Combine(archivePath, item.Name).Replace("\\", "/");
                    conn.MoveFile(sourceFilePath, destFilePath, FtpRemoteExists.Overwrite);
                    _logger.LogInformation("File {FileName} moved to archive.", item.Name);
                }
            }            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while moving files to archive in {ArchivePath}", archivePath);
        }
    }

    public void DeleteParsedFiles(string directoryPath)
    {
        try
        {
            var files = Directory.GetFiles(directoryPath, "*.json");
            foreach (var file in files)
            {
                File.Delete(file);
                _logger.LogInformation("Deleted file: {FileName}", file);
            }
            _logger.LogInformation("All files in {DirectoryPath} have been successfully deleted.", directoryPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting files in {DirectoryPath}", directoryPath);
        }
    }
}