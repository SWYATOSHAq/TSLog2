namespace TSLog.Links.Connector;

interface IDiadocWorker
{
    public void Init()
    { }
    public string GetBoxIdForDocument(Document document)
    {
        return string.Empty;
    }
    public void SendDocument(string fileName, string boxId)
    { }
    public void Test()
    { }
}
