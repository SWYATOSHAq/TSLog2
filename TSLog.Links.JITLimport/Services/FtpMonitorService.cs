using FluentFTP;
using System;
using System.Collections.Generic;
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

    public async Task DownloadFileAsync(string fileName, string localPath)
    {
        try
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(_ftpServer + _remotePath + "/" + fileName);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_ftpUsername, _ftpPassword);

            using (FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync())
            using (Stream responseStream = response.GetResponseStream())
            using (FileStream fileStream = File.Create(localPath + fileName))
            {
                await responseStream.CopyToAsync(fileStream);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading file {fileName}: {ex.Message}");
            // Handle or log the exception as needed
        }
    }
}
