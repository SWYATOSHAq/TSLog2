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
    public string GetIncomeHeader(string incomeId)
    {
        return string.Empty;
    }
    public string GetIncomeLines(string incomeId)
    {
        return string.Empty;
    }
    public string GetOutgoHeader(string outgoId)
    {
        return string.Empty;
    }
    public string GetOutgoLines(string outgoId)
    {
        return string.Empty;
    }
    public void MarkSendDocument(Document document)
    { }
    public void Disconnect()
    { }
}
