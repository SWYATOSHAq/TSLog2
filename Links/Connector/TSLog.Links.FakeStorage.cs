namespace TSLog.Links.Connector;

class FakeStorage : IStorage
{
    public Document[] GetDocumentsToSend()
    {
        return [];
    }
    public void MarkSendDocument(Document document)
    { }
}
