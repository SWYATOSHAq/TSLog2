using System.ComponentModel;

namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;

/// <summary>
/// Страница для сканирования расхода (заказа) и ТЕ.
/// </summary>
public partial class OutgoPage : ContentPage, IQueryAttributable, INotifyPropertyChanged
{
    private readonly HttpClient _httpClient;
    private string _outgoId = string.Empty;
    private string _tuId = string.Empty;
    private string _userName = string.Empty;
    private string _token = string.Empty;

    new public event PropertyChangedEventHandler? PropertyChanged;

    private void SetControlsState()
    {
        tuPageButton.IsEnabled =
            !string.IsNullOrWhiteSpace(_outgoId) && !string.IsNullOrWhiteSpace(_tuId);
    }
    public OutgoPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ClearEntries();
        outgoIdEntry.BackgroundColor = Colors.Transparent;
        tuIdEntry.BackgroundColor = Colors.Transparent;

        // Use the Dispatcher to ensure the UI is ready before setting the focus.
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
        {
            outgoIdEntry.Focus();
            return false; // Run the timer callback only once.
        });
    }

    private void ClearEntries()
    {
        outgoIdEntry.Text = string.Empty;
        tuIdEntry.Text = string.Empty;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.TryGetValue("UserName", out object? userNameValue)) 
            _userName = userNameValue.ToString() ?? string.Empty;
        if (query.TryGetValue("Token", out object? tokenValue))
            _token = tokenValue.ToString() ?? string.Empty;
    }

    private async void OnOutgoIdEntryCompleted(object? sender, EventArgs e)
    {
        _outgoId = string.Empty;
        _tuId = string.Empty;
        if (!await Api.CheckOutgo(outgoIdEntry.Text, this, _httpClient, _token))
        {
            outgoIdEntry.BackgroundColor = Colors.Coral;
            return;
        }

        var tusCount = await Api.GetOutgoTusCount(outgoIdEntry.Text, this, _httpClient, _token);
        if (tusCount <= 0)
        {
            outgoIdEntry.BackgroundColor = Colors.Coral;
            return;
        }
        _outgoId = outgoIdEntry.Text.Trim();
        SetControlsState();

        outgoTusCountLabel.Text = tusCount.ToString();
        outgoIdEntry.BackgroundColor = Colors.LightGreen;
        tuIdEntry.Text = string.Empty;
        tuIdEntry.Focus();
    }

    private async void OnTuIdEntryCompleted(object? sender, EventArgs e)
    {
        _tuId = string.Empty;
        if (!await Api.CheckOutgoTu(outgoIdEntry.Text, tuIdEntry.Text, this, _httpClient, _token))
        {
            outgoIdEntry.BackgroundColor = Colors.Coral;
            return;
        }

        _tuId = tuIdEntry.Text.Trim();
        tuIdEntry.BackgroundColor = Colors.LightGreen;
        SetControlsState();
        tuPageButton.Focus();
    }

    private async void OnScannedTusClicked(object sender, EventArgs e)
    { }

    private async void OnNextPageClicked(object sender, EventArgs e)
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "OutgoId", outgoIdEntry.Text },
            { "TuId", tuIdEntry.Text },
            { "Token", _token },
            { "UserName", _userName },
        };
        await Shell.Current.GoToAsync(nameof(SsccPage), navigationParameters);
    }

    private void OnExitClicked(object sender, EventArgs e)
    {
        UICommon.ExitApplication(this, _httpClient, _token);
    }
}
