using System.Security;

namespace TSLog.Links.Connector;

class Worker(IStorage storage, IWmsStorage wmsStorage, IDiadocWorker diadocWorker)
{
    public bool IsRunning { get; private set; } = false;
    /// <summary>
    /// In miliseconds.
    /// </summary>
    public int SleepTime { private get; set; } = 4 * 60 * 1024;
    private const int SleepStep = 64; 

    private readonly IStorage _storage = storage;
    private readonly IWmsStorage _wmsStorage = wmsStorage;
    private readonly IDiadocWorker _diadocWorker = diadocWorker;

    public void SetDbDataSource(string dbDataSource)
    {
        if (!string.IsNullOrWhiteSpace(dbDataSource))
            throw new ArgumentException($"{nameof(dbDataSource)} is empty.");
        _storage.SetDataSource(dbDataSource);
    }
    public void Connect(string dbLogin, SecureString dbPassword)
    {
        _storage.Connect(dbLogin, dbPassword);
    }
    public void Disconnect()
    {
        _storage.Disconnect();
    }

    public string GetFileFromWms(Document document)
    {
        var fileName = _wmsStorage.GetFile(document);
        //var fileName = $"{document.Kind}_{document.Id}";
        // TODO: check file name for correct path.

        return fileName;
    }

    public void SendDocument(Document document)
    {
        var fileName = GetFileFromWms(document);
        var boxId = _diadocWorker.GetBoxIdForDocument(document);
        _diadocWorker.SendDocument(fileName, boxId);
        _storage.MarkSendDocument(document);
        File.Delete(fileName);
    }
    private void SendDocuments()
    {
        var documents = _storage.GetDocumentsToSend();
        foreach (var document in documents)
            SendDocument(document);
    }
    private void RunLoop()
    {
        var counter = 0;
        while (counter < SleepTime)
        {
            if (!IsRunning)
                return;
            Thread.Sleep(SleepStep);
            counter += SleepStep;
        }
    }
    public void Start()
    {
        IsRunning = true;
        while (IsRunning)
        {
            SendDocuments();
            RunLoop();
            Application.DoEvents();
        }
    }
    public void Stop()
    {
        IsRunning = false;
    }
}
