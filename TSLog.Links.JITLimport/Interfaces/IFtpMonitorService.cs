namespace TSLog.Links.JITLimport.Interfaces
{
    internal interface IFtpMonitorService
    {
        IEnumerable<string> CheckForXlsFiles();
        Task DownloadFilesAsync(IEnumerable<string> fileList);
        void MoveRemoteFilesToArchive(string archivePath);
    }
}