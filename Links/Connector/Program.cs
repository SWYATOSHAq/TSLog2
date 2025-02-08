namespace TSLog.Links.Connector;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        IStorage storage = null;
        IWmsStorage wmsStorage = null;
        IDiadocWorker diadocWorker = null;
        var worker = new Worker(storage, wmsStorage, diadocWorker);
        var isDebug = Settings.Default.IsDebug;
        if (isDebug)
        {
            storage = new FakeStorage();
            wmsStorage = new FakeWmsStorage();
            diadocWorker = new FakeDiadocWorker();
        }
        else
        {
            var dataSource = string.Empty;
            storage = new Storage(dataSource);
            wmsStorage = new WmsStorage();


            string clientId = string.Empty; 
            string apiUrl = string.Empty;
            string login = string.Empty;
            string password = string.Empty;
            string boxId = string.Empty;
            string certificatePath = string.Empty;
            diadocWorker = new DiadocWorker(clientId, apiUrl, login, password, boxId, certificatePath);
        }

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(worker));
    }
}