using TSLog.Common;

namespace TSLog.Links.Connector;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        IStorage storage;
        IWmsStorage wmsStorage;
        IDiadocWorker diadocWorker;
        var forceStart = false;
        var isDebug = Settings.Default.IsDebug;
        if (isDebug)
        {
            storage = new FakeStorage();
            wmsStorage = new FakeWmsStorage();
            diadocWorker = new FakeDiadocWorker();
        }
        else
        {
            var dbDataSource = Settings.Default.DbDataSource;
            storage = new Storage(dbDataSource)
            {
                CommonSchema = "ecco",
                LinksSchema = "ecco",
                PartnersSchema = "ecco"
            };
            storage.Connect(Settings.Default.DbLogin, Utils.StringToSecureString(Settings.Default.DbPassword));

            Environment.SetEnvironmentVariable("ITEXT_BOUNCY_CASTLE_FACTORY_NAME", "bouncy-castle");
            wmsStorage = new WmsStorage(storage);

            // TODO: убрать после нормальной отладки подключения к DiaDoc API.
            /*
            var clientId = Settings.Default.DiadocClientId;
            var apiUrl = Settings.Default.DiadocApiUrl;
            var login = Settings.Default.DiadocLogin;
            var password = Settings.Default.DiadocPassword;
            var boxId = Settings.Default.DiadocBoxId;
            var certificatePath = Settings.Default.DiadocCertificatePath;
            diadocWorker = new DiadocWorker(clientId, apiUrl, login, password, boxId, certificatePath);
            */
            diadocWorker = new FakeDiadocWorker();

            if (args.Contains("--force-start"))
                forceStart = true;
        }
        var worker = new Worker(storage, wmsStorage, diadocWorker);

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        Application.Run(new MainForm(worker, forceStart));

        storage.Dispose();
    }
}
