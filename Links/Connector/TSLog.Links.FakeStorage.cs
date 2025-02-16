namespace TSLog.Links.Connector;

class FakeStorage : IStorage
{
    public string CommonSchema { protected get; set; } = string.Empty;
    public string LinksSchema { protected get; set; } = string.Empty;
    public string PartnersSchema { protected get; set; } = string.Empty;

    public void Dispose()
    { }
}
