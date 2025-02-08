namespace TSLog.Links.Connector;

interface IStorage
{
    public Document[] GetDocumentsToSend()
    {
        return [];
    }
    public void MarkSendDocument(Document document)
    { }
}
