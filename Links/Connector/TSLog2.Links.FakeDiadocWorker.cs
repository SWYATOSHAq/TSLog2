namespace TSLog.Links.Connector;

class FakeDiadocWorker: IDiadocWorker
{
    public void Init(string clientId, string apiUrl, string login, string password, string boxId, string certificatePath)
    { }
    public string GetBoxIdForDocument(Document document)
    {
        return string.Empty;
    }
    public void SendDocument(string fileName, string boxId)
    { }
}
