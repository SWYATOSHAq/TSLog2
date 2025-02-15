namespace TSLog.Links.BLCtobacco.Interfaces;

internal interface IFileFtpService
{
    void CreateInMessage();
    void CreateOutMessage();
    Task UploadFileToServerAsync();
}
