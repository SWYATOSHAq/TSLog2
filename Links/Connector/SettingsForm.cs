namespace TSLog.Links.Connector;

public partial class SettingsForm : Form
{
    public SettingsForm()
    {
        InitializeComponent();

        DbDataSourceTextBox.Text = Settings.Default.DbDataSource;
        DbLoginTextBox.Text = Settings.Default.DbLogin;
        DbPasswordTextBox.Text = Settings.Default.DbPassword;
    }
    public string DbDataSource
    {
        get
        {
            return DbDataSourceTextBox.Text.Trim().ToLower();
        }
    }
    public string DbLogin
    {
        get
        {
            return DbLoginTextBox.Text.Trim().ToLower();
        }
    }
    public string DbPassword
    {
        get
        {
            return DbPasswordTextBox.Text.Trim();
        }
    }
    private void SetControlsState()
    {
        SaveButton.Enabled = DbDataSourceTextBox.Text.Trim().Length > 0 &&
            DbLoginTextBox.Text.Trim().Length > 0 &&
            DbPasswordTextBox.Text.Trim().Length > 0 &&
            
            IntervalTextBox.Text.Trim().Length > 0 && int.Parse(IntervalTextBox.Text.Trim()) > 0;
    }

    private void SaveButton_Click(object sender, EventArgs e)
    {
        //
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        //
    }

    private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyData == Keys.Escape)
            CancelButton.PerformClick();
    }

    private void TextBox_TextChanged(object sender, EventArgs e)
    {
        SetControlsState();
    }

    private void SettingsForm_Shown(object sender, EventArgs e)
    {
        SetControlsState();
    }

    private void IntervalTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            e.Handled = true;
    }
}
