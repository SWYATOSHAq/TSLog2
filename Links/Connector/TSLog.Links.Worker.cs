using System.Security;

namespace TSLog.Links.Connector;

class Worker(IStorage storage, IWmsStorage wmsStorage, IDiadocWorker diadocWorker)
{
    public delegate void ReportProgress(int count);
    public IProgress<int>? Reporter { private get; set; } = null;
    public bool IsRunning { get; private set; } = false;
    private const int DefaultSleepTime = 4 * 60 * 1024;
    /// <summary>
    /// In miliseconds.
    /// </summary>
    public int SleepTime { private get; set; } = DefaultSleepTime;
    private const int SleepStep = 64; 

    private readonly IStorage _storage = storage;
    private readonly IWmsStorage _wmsStorage = wmsStorage;
    private readonly IDiadocWorker _diadocWorker = diadocWorker;
    private int _count = 0;

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
    public void SetIncomesPath(string path)
    {
        _wmsStorage.IncomesPath = path;
    }
    public void SetOutgoesPath(string path)
    {
        _wmsStorage.OutgoesPath = path;
    }
    public string GetFileFromWms(Document document)
    {
        var fileName = _wmsStorage.GetFile(document);
        return fileName;
    }

    public void SendDocument(Document document)
    {
        var fileName = GetFileFromWms(document);
        var boxId = _diadocWorker.GetBoxIdForDocument(document);
        _diadocWorker.SendDocument(fileName, boxId);
        _storage.MarkSendDocument(document);
        
        // TODO: в будущем удалять, оставить на этапе внедрения.
        //File.Delete(fileName);
    }
    private int SendDocuments()
    {
        var documents = _storage.GetDocumentsToSend();
        foreach (var document in documents)
        {
            if (!IsRunning)
                break;
            SendDocument(document);
            _count++;
            Reporter?.Report(_count);
        }
        return documents.Length;
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
        _count = 0;
        IsRunning = true;
        while (IsRunning)
        {
            int documentCount;
            do
            {
                documentCount = SendDocuments();
            }
            while (IsRunning && documentCount > 0);
            RunLoop();
            Application.DoEvents();
        }
    }
    public void Stop()
    {
        _count = 0;
        IsRunning = false;
    }
}
