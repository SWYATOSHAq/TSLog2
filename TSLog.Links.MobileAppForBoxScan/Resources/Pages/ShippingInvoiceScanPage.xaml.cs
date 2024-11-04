using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using TSLog.Links.MobileAppForBoxScan.Models;

namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;

public partial class ShippingInvoiceScanPage : ContentPage, IQueryAttributable, INotifyPropertyChanged
{
    private readonly HttpClient _httpClient;
    private string _username;
    private string _password;
    private string _token;
    private bool _isNextPageButtonEnabled;

    public event PropertyChangedEventHandler PropertyChanged;

    public bool IsNextPageButtonEnabled
    {
        get => _isNextPageButtonEnabled;
        set
        {
            _isNextPageButtonEnabled = value;
            OnPropertyChanged();
        }
    }

    public ShippingInvoiceScanPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
  
        // Attach the Completed event handler to the invoiceEntry.
        invoiceEntry.Completed += InvoiceEntry_Completed;
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ClearEntries();
        invoiceEntry.BackgroundColor = Colors.Transparent;
        teNumEntry.BackgroundColor = Colors.Transparent;
        IsNextPageButtonEnabled = false;

        // Use the Dispatcher to ensure the UI is ready before setting the focus.
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
        {
            invoiceEntry.Focus();
            return false; // Run the timer callback only once.
        });
    }

    private void ClearEntries()
    {
        invoiceEntry.Text = string.Empty;
        teNumEntry.Text = string.Empty;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("Username"))
        {
            _username = query["Username"].ToString();
        }

        if (query.ContainsKey("Password"))
        {
            _password = query["Password"].ToString();
        }

        if (query.ContainsKey("Token"))
        {
            _token = query["Token"].ToString();
        }
    }

    private async void InvoiceEntry_Completed(object sender, EventArgs e)
    {
        string orderId = invoiceEntry.Text;

        try
        {
            if (string.IsNullOrEmpty(_token))
            {
                await DisplayAlert("Ошибка", "Token is missing. Please log in again.", "OK");
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            var response = await _httpClient.GetAsync($"validate-order/{orderId}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<ValidateOrderResponse>();
                if (responseContent != null && responseContent.Message == "Order found")
                {
                    // Set green border for validated order.
                    invoiceEntry.BackgroundColor = Colors.LightGreen;
                    teNumEntry.Focus();
                }
                else
                {
                    // Set red border for invalid order.
                    invoiceEntry.BackgroundColor = Colors.LightCoral;
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("Unauthorized", "Your session has expired. Please log in again.", "OK");
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
            else
            {
                // Handle unexpected status codes.
                invoiceEntry.BackgroundColor = Colors.LightCoral;
                await DisplayAlert("Ошибка", "Заказ не найден. Попробуйте снова.", "OK");
            }
        }
        catch (HttpRequestException httpRequestEx)
        {
            invoiceEntry.BackgroundColor = Colors.LightCoral;
            await DisplayAlert("HTTP Error", $"Request error: {httpRequestEx.Message}", "OK");
        }
        catch (Exception ex)
        {
            invoiceEntry.BackgroundColor = Colors.LightCoral;
            await DisplayAlert("Ошибка", $"Не удалось выполнить проверку заказа: {ex.Message}", "OK");
        }

        UpdateNextPageButtonState();
    }

    private void OnTeNumEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateNextPageButtonState();
    }

    private void UpdateNextPageButtonState()
    {
        IsNextPageButtonEnabled = invoiceEntry.BackgroundColor == Colors.LightGreen &&
            !string.IsNullOrWhiteSpace(teNumEntry.Text);
    }

    private async void OnExitClicked(object sender, EventArgs e)
    {
        try
        {
            var loginRequest = new { Token = _token };
            var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            var response = await _httpClient.PostAsync("logoff", content);

            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось выполнить выход", "OK");
            }
        }
        catch (HttpRequestException httpRequestEx)
        {
            await DisplayAlert("HTTP Error", $"Request error: {httpRequestEx.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось выполнить выход: {ex.Message}", "OK");
        }
    }

    private async void OnNextPageClicked(object sender, EventArgs e)
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "InvoiceId", invoiceEntry.Text },
            { "TeNum", teNumEntry.Text },
            { "Token", _token }
        };
        await Shell.Current.GoToAsync(nameof(ScanBoxPage), navigationParameters);
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
