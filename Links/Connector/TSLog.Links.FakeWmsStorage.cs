namespace TSLog.Links.Connector;

class FakeWmsStorage : IWmsStorage
{
    private const string DummyPdfFileName = "Dummy.pdf";
    public string IncomesPath { set => throw new NotImplementedException(); }
    public string OutgoesPath { set => throw new NotImplementedException(); }

    public string GetFile(Document document)
    {
        if (!File.Exists(DummyPdfFileName))
            throw new FileNotFoundException(DummyPdfFileName);
        return DummyPdfFileName;
    }
}
