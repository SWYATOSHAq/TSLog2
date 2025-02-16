using System.Security;
using TSLog.Incomes.BO;
using TSLog.Outgoes.BO;

namespace TSLog.Links.Connector;

interface IStorage : IDisposable
{
    public string CommonSchema { set; }
    public string LinksSchema { set; }
    public string PartnersSchema { set; }
    public void SetDataSource(string dataSource)
    { }
    public void Connect(string user, SecureString password)
    { }
    public Document[] GetDocumentsToSend()
    {
        return [];
    }
    public Income GetIncome(string incomeId)
    {
        throw new NotImplementedException();
    }
    public Outgo GetOutgo(string outgoId)
    {
        throw new NotImplementedException();
    }
    public void MarkSendDocument(Document document)
    { }
    public void Disconnect()
    { }
}
