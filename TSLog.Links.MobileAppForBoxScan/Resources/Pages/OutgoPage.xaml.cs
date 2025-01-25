using System.ComponentModel;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;

public partial class OutgoPage : ContentPage, IQueryAttributable, INotifyPropertyChanged
{
    private readonly HttpClient _httpClient;
    private string _userName = string.Empty;
    private string _token = string.Empty;
    private bool _isNextPageButtonEnabled = false;

    new public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsNextPageButtonEnabled
    {
        get => _isNextPageButtonEnabled;
        set
        {
            _isNextPageButtonEnabled = value;
            OnPropertyChanged();
        }
    }

    public OutgoPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;

        // Attach the Completed event handler to the outgoIdEntry.
        outgoIdEntry.Completed += OutgoIdEntryCompleted;
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ClearEntries();
        outgoIdEntry.BackgroundColor = Colors.Transparent;
        tuIdEntry.BackgroundColor = Colors.Transparent;
        IsNextPageButtonEnabled = false;

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

    private async void OutgoIdEntryCompleted(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(_token))
            {
                await DisplayAlert("Ошибка", "Token is missing. Please log in again.", "OK");
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var outgoId = outgoIdEntry.Text;
            var response = await _httpClient.GetAsync($"check-outgo/{outgoId}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                outgoIdEntry.BackgroundColor = Colors.LightGreen;
                tuIdEntry.Focus();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
                outgoIdEntry.BackgroundColor = Colors.LightCoral;
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("Unauthorized", "Your session has expired. Please log in again.", "OK");
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
            else
            {
                // Handle unexpected status codes.
                outgoIdEntry.BackgroundColor = Colors.LightCoral;
                await DisplayAlert("Ошибка", "Заказ не найден. Попробуйте снова.", "OK");
            }
        }
        catch (HttpRequestException exception)
        {
            outgoIdEntry.BackgroundColor = Colors.LightCoral;
            await DisplayAlert("HTTP Error", $"Request error: {exception.Message}", "OK");
        }
        catch (Exception exception)
        {
            outgoIdEntry.BackgroundColor = Colors.LightCoral;
            await DisplayAlert("Ошибка", $"Не удалось выполнить проверку заказа: {exception.Message}", "OK");
        }

        UpdateNextPageButtonState();
    }

    private void OnTuIdEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateNextPageButtonState();
    }

    private void UpdateNextPageButtonState()
    {
        IsNextPageButtonEnabled = outgoIdEntry.BackgroundColor == Colors.LightGreen &&
            !string.IsNullOrWhiteSpace(tuIdEntry.Text);
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
        await Shell.Current.GoToAsync(nameof(SsccPage), navigationParameters);
    }

    new private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnExitClicked(object sender, EventArgs e)
    {
        UICommon.ExitApplication(this, _httpClient, _token);
    }
}
