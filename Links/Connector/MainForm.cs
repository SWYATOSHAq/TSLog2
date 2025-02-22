namespace TSLog.Links.Connector;

/// <summary>
/// Главное окно приложения.
/// </summary>
partial class MainForm : Form
{
    private readonly Worker _worker;
    private Task? _task = null;
    public MainForm(Worker worker, bool forceStart = false)
    {
        InitializeComponent();
        _worker = worker ?? throw new ArgumentNullException(nameof(worker));
        if (forceStart)
            Start();
    }
    private void Start()
    {
        var progress = new Progress<int>(ReportProgress);
        _worker.Reporter = progress;

        _task = Task.Run(_worker.Start);
        Thread.Sleep(64);
        SetControlsState();
    }
    private void ReportProgress(int count)
    {
        ProgressLabel.Text = $"Обработано документов - {count}";
    }
    private async Task Stop()
    {
        _worker.Stop();
        if (_task != null)
        {
            await _task;
            _task = null;
        }
        ProgressLabel.Text = "Прогресс выполнения...";
    }
    private void SetControlsState()
    {
        StartButton.Enabled = !_worker.IsRunning;
        StopButton.Enabled = _worker.IsRunning;
        SetingsButton.Enabled = !_worker.IsRunning;
    }
    private void StartButton_Click(object sender, EventArgs e)
    {
        Start();
    }

    private async void StopButton_Click(object sender, EventArgs e)
    {
        await Stop();
        SetControlsState();
    }

    private void SetingsButton_Click(object sender, EventArgs e)
    {
        var settingsForm = new SettingsForm(Settings.Default);
        if (settingsForm.ShowDialog() == DialogResult.OK)
        {
            Settings.Default.DbDataSource = settingsForm.DbDataSource;
            Settings.Default.DbLogin = settingsForm.DbLogin;
            Settings.Default.DbPassword = settingsForm.DbPassword;

            Settings.Default.SleepTime = settingsForm.SleepTime;
            Settings.Default.IncomesPath = settingsForm.IncomesPath;
            Settings.Default.OutgoesPath = settingsForm.OutgoesPath;

            Settings.Default.Save();

            _worker.Disconnect();
            _worker.SetDbDataSource(Settings.Default.DbDataSource);
            var securePassword = TSLog.Common.Utils.StringToSecureString(Settings.Default.DbLogin);
            _worker.Connect(Settings.Default.DbLogin, securePassword);

            _worker.SleepTime = Settings.Default.SleepTime;
            _worker.SetIncomesPath(Settings.Default.IncomesPath);
            _worker.SetOutgoesPath(Settings.Default.OutgoesPath);
        }
    }

    private async void ExitButton_Click(object sender, EventArgs e)
    {
        await Stop();
        Close();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        SetControlsState();
    }

    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyData == Keys.Escape)
            ExitButton.PerformClick();
    }
}
