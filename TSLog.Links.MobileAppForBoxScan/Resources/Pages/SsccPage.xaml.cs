namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;

public partial class SsccPage : ContentPage, IQueryAttributable
{
    private readonly HttpClient _httpClient;
    private string _outgoId = string.Empty;
    private string _tuId = string.Empty;
    private string _userName = string.Empty;
    private string _token = string.Empty;
    private int _scannedBoxesCount = 0;
    private readonly ISet<string> _ssccs = new HashSet<string>();

    public SsccPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
        ssccEntry.Completed += OnSsccEntryCompleted;
    }
    private void ClearSsccs()
    {
        _ssccs.Clear();
        scannedBoxesCountLabel.Text = "0";
    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.TryGetValue("OutgoId", out object? outgoIdValue))
            _outgoId = outgoIdValue.ToString() ?? string.Empty;
        if (query.TryGetValue("TuId", out object? tuIdValue))
            _tuId = tuIdValue.ToString() ?? string.Empty;
        if (query.TryGetValue("UserName", out object? userNameValue))
            _userName = userNameValue.ToString() ?? string.Empty;
        if (query.TryGetValue("Token", out object? tokenValue))
            _token = tokenValue.ToString() ?? string.Empty;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Use the Dispatcher to ensure the UI is ready before setting the focus.
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
        {
            ssccEntry.Focus();
            return false; // Run the timer callback only once.
        });

        outgoIdLabel.Text = _outgoId;
        tuIdLabel.Text = _tuId;

        var boxesCount = await Api.GetTuBoxesCount(_tuId, this, _httpClient, _token);
        if (boxesCount <= 0)
        {
            ClearSsccs();
            await Shell.Current.GoToAsync($"//{nameof(OutgoPage)}");
        }

         _scannedBoxesCount = await Api.GetScannedTuBoxesCount(_tuId, this, _httpClient, _token);
        if (_scannedBoxesCount < 0)
        {
            ClearSsccs();
            await Shell.Current.GoToAsync($"//{nameof(OutgoPage)}");
        }

        totalBoxesCountLabel.Text = boxesCount.ToString();
        scannedBoxesCountLabel.Text = _scannedBoxesCount.ToString();
        ssccEntry.Focus();

        // Just debug.
        ssccEntry.Text = "12345678901234567890";
    }
    private async void OnSsccEntryCompleted(object? sender, EventArgs e)
    {
        var text = ssccEntry.Text;
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (Common.CheckSscc(text, out string sscc) &&
            !_ssccs.Contains(text.Trim()))
        {
            _ssccs.Add(sscc);
            _scannedBoxesCount++;
            scannedBoxesCountLabel.Text = _scannedBoxesCount.ToString();
        }
        else
        {
            ssccEntry.Text = string.Empty;
            await DisplayAlert("Ошибка", $"Не корректный SSCC {text}.", "OK");
        }

        ssccEntry.Text = string.Empty;
        ssccEntry.Focus();
    }
    private async void OnScannedSsccsClicked(object? sender, EventArgs e)
    {
        /*
        try
        {
            if (_isProcessingScan)
                return;

            _isProcessingScan = true;
            var sscc = ssccEntry.Text;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            
            var response = await _httpClient.GetAsync($"remove-sscc/{sscc}");
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Успех", "SSCC короба успешно удалён.", "OK");
                _scannedBoxesCount--;
                counterLabel.Text = _scannedBoxesCount.ToString();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("Unauthorized", "Your session has expired. Please log in again.", "OK");
                await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
            }
            else
                await DisplayAlert("Ошибка", "Не удалось удалить SSCC короба. Попробуйте снова.", "OK");
        }
        catch (HttpRequestException httpRequestEx)
        {
            await DisplayAlert("HTTP Error", $"Request error: {httpRequestEx.Message}", "OK");
        }
        catch (Exception exception)
        {
            await DisplayAlert("Ошибка", $"Не удалось удалить SSCC короба: {exception.Message}", "OK");
        }
        */
    }
    private async void OnConfirmClicked(object? sender, EventArgs e)
    {
        if (!await Api.CheckOutgo(_outgoId, this, _httpClient, _token))
            return;
        if (!await Api.CheckOutgoTu(_outgoId, _tuId, this, _httpClient, _token))
            return;

        // TODO check boxes count.

        if (!await Api.StoreSsccs([.. _ssccs], _outgoId, _tuId, _userName, this, _httpClient, _token))
            return;
        ClearSsccs();
        await Shell.Current.GoToAsync($"//{nameof(OutgoPage)}");
    }
    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        ClearSsccs();
        await Shell.Current.GoToAsync($"//{nameof(OutgoPage)}");
    }
    private void OnExitClicked(object? sender, EventArgs e)
    {
        UICommon.ExitApplication(this, _httpClient, _token);
    }
}
