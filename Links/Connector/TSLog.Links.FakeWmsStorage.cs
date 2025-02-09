namespace TSLog.Links.Connector;

class FakeWmsStorage : IWmsStorage
{
    private const string DummyPdfFileName = "Dummy.pdf";
    public string GetFile(Document document)
    {
        if (!File.Exists(DummyPdfFileName))
            throw new FileNotFoundException(DummyPdfFileName);
        return DummyPdfFileName;
    }
}
