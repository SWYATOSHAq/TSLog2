namespace TSLog.Links.Connector;

class FakeWmsStorage : IWmsStorage
{
    public string GetFile(Document document)
    {
        return string.Empty;
    }
}
