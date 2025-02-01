namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;

/// <summary>
/// Страница для сканирования SSСС.
/// </summary>
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
        else
            throw new Exception("OutgoId not foud!");
        if (query.TryGetValue("TuId", out object? tuIdValue))
            _tuId = tuIdValue.ToString() ?? string.Empty;
        else
            throw new Exception("TuId not foud!");
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
        ssccEntry.Text = string.Empty;
        ssccEntry.Focus();
    }
    private async void OnSsccEntryCompleted(object? sender, EventArgs e)
    {
        var text = ssccEntry.Text;
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (Common.CheckSscc(text, out string sscc))
        {
            if (!_ssccs.Contains(text.Trim()))
            {
                _ssccs.Add(sscc);
                _scannedBoxesCount++;
                scannedBoxesCountLabel.Text = _scannedBoxesCount.ToString();
            }
            else 
            {
                ssccEntry.Text = string.Empty;
                await DisplayAlert("Ошибка", $"SSCC {text} уже просканирован.", "OK");
            }
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
        var navigationParameters = new Dictionary<string, object>
        {
            { "OutgoId", _outgoId },
            { "TuId", _tuId },
            { "Token", _token },
            { "UserName", _userName },
        };
        await Shell.Current.GoToAsync($"{nameof(ScannedTuBoxesPage)}", navigationParameters);
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
