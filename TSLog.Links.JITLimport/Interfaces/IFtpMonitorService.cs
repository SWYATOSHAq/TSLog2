namespace TSLog.Links.JITLimport.Interfaces;

public interface IFtpMonitorService
{
    IEnumerable<string> CheckForXlsFiles();
    Task DownloadFilesAsync(IEnumerable<string> fileList, CancellationToken cancellationToken);
    void MoveRemoteFilesToArchive(string archivePath);
    void DeleteParsedFiles(string directoryPath);
}