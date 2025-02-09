namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;

/// <summary>
/// Страница ТЕ для сканирования SSСС.
/// </summary>
public partial class TuPage : ContentPage, IQueryAttributable
{
    private readonly HttpClient _httpClient;
    private string _outgoId = string.Empty;
    private string _tuId = string.Empty;
    private string _userName = string.Empty;
    private string _token = string.Empty;
    private int _scannedAndStoredBoxesCount = 0;
    private readonly ISet<string> _storedSsccs = new HashSet<string>();
    private readonly ISet<string> _scannedSsccs = new HashSet<string>();

    public TuPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
        versionLabel.Text = Common.Version;
    }
    private void ClearSsccs()
    {
        _storedSsccs.Clear();
        _scannedSsccs.Clear();
        scannedAndStoredBoxesCountLabel.Text = "0";
    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.TryGetValue("OutgoId", out object? outgoIdValue))
            _outgoId = outgoIdValue.ToString() ?? string.Empty;
        else
            throw new Exception("OutgoId not foud!");
        if (query.TryGetValue("TuId", out object? tuIdValue))
            _tuId = tuIdValue.ToString() ?? string.Empty;
        else
            throw new Exception("TuId not foud!");
        if (query.TryGetValue("ScannedSsccs", out object? scannedSsccsValue))
        {
            var temp = (string[])scannedSsccsValue;
            foreach (var sscc in temp)
                _scannedSsccs.Add(sscc);
        }
        if (query.TryGetValue("UserName", out object? userNameValue))
            _userName = userNameValue.ToString() ?? string.Empty;
        else
            throw new Exception("UserName not foud!");
        if (query.TryGetValue("Token", out object? tokenValue))
            _token = tokenValue.ToString() ?? string.Empty;
        else
            throw new Exception("Token not foud!");
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        outgoIdLabel.Text = _outgoId;
        tuIdLabel.Text = _tuId;

        var boxesCount = await Api.GetTuBoxesCount(_tuId, this, _httpClient, _token);
        if (boxesCount <= 0)
        {
            ClearSsccs();
            await Shell.Current.GoToAsync($"//{nameof(OutgoPage)}");
        }

        var storedSsccs = await Api.GetTuScannedBoxes(_tuId, this, _httpClient, _token);
        _scannedAndStoredBoxesCount = storedSsccs.Length + _scannedSsccs.Count;
        foreach (var sscc in storedSsccs)
            _storedSsccs.Add(sscc);

        totalBoxesCountLabel.Text = boxesCount.ToString();
        scannedAndStoredBoxesCountLabel.Text = _scannedAndStoredBoxesCount.ToString();
        ssccEntry.Text = string.Empty;
        ssccEntry.Focus();
    }
    private async void OnSsccEntryCompleted(object? sender, EventArgs e)
    {
        var text = ssccEntry.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            UICommon.FocusEntry(ssccEntry, Colors.Transparent);
            return;
        }

        Color color;
        if (Common.CheckSscc(text, out string sscc))
        {
            if (!_storedSsccs.Contains(text.Trim()) && !_scannedSsccs.Contains(text.Trim()))
            {
                _scannedSsccs.Add(sscc);
                _scannedAndStoredBoxesCount++;
                scannedAndStoredBoxesCountLabel.Text = _scannedAndStoredBoxesCount.ToString();
                color = Colors.LightGreen;
            }
            else 
            {
                color = Colors.Coral;
                await DisplayAlert("Ошибка", $"SSCC {text} уже просканирован.", "OK");
            }
        }
        else
        {
            color = Colors.Coral;
            await DisplayAlert("Ошибка", $"Не корректный SSCC {text}.", "OK");
        }
        UICommon.FocusEntry(ssccEntry, color);
    }
    private async void OnScannedSsccsClicked(object? sender, EventArgs e)
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "OutgoId", _outgoId },
            { "TuId", _tuId },
            { "ScannedSsccs", _scannedSsccs.ToArray() },
            { "Token", _token },
            { "UserName", _userName },
        };
        await Shell.Current.GoToAsync($"{nameof(TuScannedBoxesPage)}", navigationParameters);
    }
    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        if (!await Api.CheckOutgo(_outgoId, this, _httpClient, _token))
            return;
        if (!await Api.CheckOutgoTu(_outgoId, _tuId, this, _httpClient, _token))
            return;

        // TODO check boxes count.

        if (!await Api.StoreSsccs([.. _scannedSsccs], _outgoId, _tuId, _userName, this, _httpClient, _token))
            return;
        ClearSsccs();
        var navigationParameters = new Dictionary<string, object>
        {
            { "OutgoId", _outgoId },
            { "UserName", _userName },
            { "Token", _token }
        };
        await Shell.Current.GoToAsync($"//{nameof(OutgoPage)}", navigationParameters);
    }
    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        ClearSsccs();
        var navigationParameters = new Dictionary<string, object>
        {
            { "OutgoId", _outgoId },
            { "UserName", _userName },
            { "Token", _token }
        };
        await Shell.Current.GoToAsync($"//{nameof(OutgoPage)}", navigationParameters);
    }
    private void OnExitClicked(object? sender, EventArgs e)
    {
        UICommon.ExitApplication(this, _httpClient, _token);
    }
}
