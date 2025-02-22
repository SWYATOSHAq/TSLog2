namespace TSLog.Links.Connector;

interface IWmsStorage
{
    public string IncomesPath { set; }
    public string OutgoesPath { set; }
    public string GetFile(Document document)
    {
        return string.Empty;
    }
}
