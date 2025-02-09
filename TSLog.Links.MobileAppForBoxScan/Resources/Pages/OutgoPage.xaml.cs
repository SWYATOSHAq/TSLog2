using System.ComponentModel;

namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;

/// <summary>
/// Страница для сканирования расхода (заказа) и ТЕ.
/// </summary>
public partial class OutgoPage : ContentPage, IQueryAttributable
{
    private readonly HttpClient _httpClient;
    private string _outgoId = string.Empty;
    private string _tuId = string.Empty;
    private string _userName = string.Empty;
    private string _token = string.Empty;

    private void SetControlsState()
    {
        tuPageButton.IsEnabled =
            !string.IsNullOrWhiteSpace(outgoIdEntry.Text) && 
            !string.IsNullOrWhiteSpace(tuIdEntry.Text);
        scannedTusButton.IsEnabled = !string.IsNullOrWhiteSpace(_outgoId);
    }
    public OutgoPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
        BindingContext = this;
        versionLabel.Text = Common.Version;
    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.TryGetValue("OutgoId", out object? outgoIdValue))
            _outgoId = outgoIdValue.ToString() ?? string.Empty;
        else
            _outgoId = string.Empty;
        if (query.TryGetValue("TuId", out object? tuIdValue) && !string.IsNullOrWhiteSpace(_outgoId))
            _tuId = tuIdValue.ToString() ?? string.Empty;
        else
            _tuId = string.Empty;
        if (query.TryGetValue("UserName", out object? userNameValue))
            _userName = userNameValue.ToString() ?? string.Empty;
        else
            throw new Exception("UserName not found!");
        if (query.TryGetValue("Token", out object? tokenValue))
            _token = tokenValue.ToString() ?? string.Empty;
        else
            throw new Exception("Token not found!");
    }
    private async Task<bool> ShowOutgoInfo()
    {
        if (string.IsNullOrWhiteSpace(_outgoId))
        {
            UICommon.FocusEntry(outgoIdEntry, Colors.Transparent);
            return false;
        }
        if (!await Api.CheckOutgo(_outgoId, this, _httpClient, _token))
        {
            UICommon.FocusEntry(outgoIdEntry, Colors.Coral);
            return false;
        }
        var tusCount = await Api.GetOutgoTusCount(_outgoId, this, _httpClient, _token);
        if (tusCount <= 0)
        {
            UICommon.FocusEntry(outgoIdEntry, Colors.Coral);
            return false;
        }
        var scannedTusCount = await Api.GetOutgoScannedTusCount(_outgoId, this, _httpClient, _token);
        if (scannedTusCount < 0)
        {
            UICommon.FocusEntry(outgoIdEntry, Colors.Coral);
            return false;
        }
        outgoTusCountLabel.Text = tusCount.ToString();
        outgoScannedTusCountLabel.Text = scannedTusCount.ToString();
        outgoIdEntry.BackgroundColor = Colors.LightGreen;
        return true;
    }
    private async Task<bool> ShowTuInfo()
    {
        if (string.IsNullOrWhiteSpace(_tuId))
        {
            UICommon.FocusEntry(tuIdEntry, Colors.Transparent);
            return false;
        }
        if (!await Api.CheckOutgoTu(_outgoId, _tuId, this, _httpClient, _token))
        {
            UICommon.FocusEntry(tuIdEntry, Colors.Coral);
            return false;
        }
        tuIdEntry.BackgroundColor = Colors.LightGreen;
        
        return true;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        outgoIdEntry.Text = _outgoId;
        tuIdEntry.Text = _tuId;
        outgoIdEntry.BackgroundColor = Colors.Transparent;
        tuIdEntry.BackgroundColor = Colors.Transparent;

        if (await ShowOutgoInfo())
        {
            if (!await ShowTuInfo())
                _tuId = string.Empty;
        }
        else
            _outgoId = string.Empty;

        SetControlsState();
    }
    private async void OnOutgoIdEntryCompleted(object? sender, EventArgs e)
    {
        _outgoId = outgoIdEntry.Text.Trim();
        _tuId = string.Empty;
        tuIdEntry.Text = _tuId;
        tuIdEntry.BackgroundColor = Colors.Transparent;
        var result = await ShowOutgoInfo();
        if (result)
            tuIdEntry.Focus();
        else
            _outgoId = string.Empty;
        SetControlsState();
    }
    private async void OnTuIdEntryCompleted(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_outgoId))
        {
            await DisplayAlert("Ошибка", "Сперва отсканируйте заказ!", "OK");

            _tuId = string.Empty;
            tuIdEntry.Text = _tuId;
            tuIdEntry.BackgroundColor = Colors.Transparent;

            outgoIdEntry.BackgroundColor = Colors.Transparent;
            outgoIdEntry.Focus();
            return;
        }

        _tuId = tuIdEntry.Text.Trim();
        var result = await ShowTuInfo();
        if (result)
            tuPageButton.Focus();
        else
            _tuId = string.Empty;
        SetControlsState();
    }
    private async void OnScannedTusClicked(object sender, EventArgs e)
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "OutgoId", _outgoId },
            { "Token", _token },
            { "UserName", _userName },
        };
        await Shell.Current.GoToAsync(nameof(OutgoScannedTusPage), navigationParameters);
    }
    private async void OnNextPageClicked(object sender, EventArgs e)
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "OutgoId", outgoIdEntry.Text },
            { "TuId", tuIdEntry.Text },
            { "Token", _token },
            { "UserName", _userName },
        };
        await Shell.Current.GoToAsync(nameof(TuPage), navigationParameters);
    }
    private void OnExitClicked(object sender, EventArgs e)
    {
        UICommon.ExitApplication(this, _httpClient, _token);
    }
}
