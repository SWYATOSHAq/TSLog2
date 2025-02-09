using System.Security;

namespace TSLog.Links.Connector;

interface IStorage : IDisposable
{
    public void SetDataSource(string dataSource)
    { }
    public void Connect(string user, SecureString password)
    { }
    public Document[] GetDocumentsToSend()
    {
        return [];
    }
    public void MarkSendDocument(Document document)
    { }
    public void Disconnect()
    { }
}
