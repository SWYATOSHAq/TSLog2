namespace TSLog.Links.Connector;

partial class MainForm : Form
{
    private readonly Worker _worker;
    private Task? _task = null;
    public MainForm(Worker worker)
    {
        InitializeComponent();
        _worker = worker ?? throw new ArgumentNullException(nameof(worker));
    }
    private async Task Stop()
    {
        _worker.Stop();
        if (_task != null)
        {
            await _task;
            _task = null;
        }
    }
    private void SetControlsState()
    {
        StartButton.Enabled = !_worker.IsRunning;
        StopButton.Enabled = _worker.IsRunning;
        SetingsButton.Enabled = !_worker.IsRunning;
    }
    private void StartButton_Click(object sender, EventArgs e)
    {
        _task = Task.Run(_worker.Start);
        Thread.Sleep(64);
        SetControlsState();
    }

    private async void StopButton_Click(object sender, EventArgs e)
    {
        await Stop();
        SetControlsState();
    }

    private void SetingsButton_Click(object sender, EventArgs e)
    {
        var settingsForm = new SettingsForm();
        if (settingsForm.ShowDialog() == DialogResult.OK)
        {
            Settings.Default.DbDataSource = settingsForm.DbDataSource;
            Settings.Default.DbLogin = settingsForm.DbLogin;
            Settings.Default.DbPassword = settingsForm.DbPassword;
            Settings.Default.Save();

            _worker.Disconnect();
            _worker.SetDbDataSource(Settings.Default.DbDataSource);
            var securePassword = TSLog.Common.Utils.StringToSecureString(Settings.Default.DbLogin);
            _worker.Connect(Settings.Default.DbLogin, securePassword);
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
