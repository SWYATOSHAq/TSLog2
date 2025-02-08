using System.IO;

namespace TSLog.Links.Connector;

class Worker
{
    public bool IsRunning { get; private set; } = false;
    /// <summary>
    /// In miliseconds.
    /// </summary>
    public int SleepTime { private get; set; } = 4 * 60;
    private IStorage _storage;
    private IWmsStorage _wmsStorage;
    private IDiadocWorker _diadocWorker;
    public Worker(IStorage storage, IWmsStorage wmsStorage, IDiadocWorker diadocWorker)
    {
        _storage = storage;
        _wmsStorage = wmsStorage;
        _diadocWorker = diadocWorker;
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
    private void SendDocument()
    {
        var documents = _storage.GetDocumentsToSend();
        foreach (var document in documents)
            SendDocument(document);
    }
    public void Run()
    {
        IsRunning = true;
        while (IsRunning)
        {
            SendDocument();
            Thread.Sleep(SleepTime);
        }
    }
}
