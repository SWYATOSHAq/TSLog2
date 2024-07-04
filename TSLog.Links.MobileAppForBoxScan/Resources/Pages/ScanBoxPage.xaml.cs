using System.Text.Json;
using System.Text;
using System.Net.Http.Json;
using System.Net;
using System.Text.RegularExpressions;
using TSLog.Links.MobileAppForBoxScan.Models;

namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;

public partial class ScanBoxPage : ContentPage, IQueryAttributable
{
    private readonly HttpClient _httpClient;
    private string _token;
    private string _invoiceId;
    private string _teNum;
    private int _scanCounter;
    private bool _isProcessingScan;

    public ScanBoxPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
        scanBoxEntry.Completed += OnScanBoxEntryCompleted;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Use the Dispatcher to ensure the UI is ready before setting the focus.
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
        {
            scanBoxEntry.Focus();
            return false; // Run the timer callback only once.
        });
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("InvoiceId"))
        {
            _invoiceId = query["InvoiceId"].ToString();
            invoiceIdLabel.Text = $"Номер заказа: {_invoiceId}";
        }

        if (query.ContainsKey("TeNum"))
        {
            _teNum = query["TeNum"].ToString();
            teNumLabel.Text = $"Номер TE: {_teNum}";
        }

        if (query.ContainsKey("Token"))
        {
            _token = query["Token"].ToString();
        }
    }

    private async void OnScanBoxEntryCompleted(object sender, EventArgs e)
    {
        if (_isProcessingScan)
        {
            return;
        }

        _isProcessingScan = true;
        string ssccCode = scanBoxEntry.Text;

        // Clean the input to remove any symbols except numbers.
        ssccCode = Regex.Replace(ssccCode, "[^0-9]", "");

        if (string.IsNullOrWhiteSpace(ssccCode) || ssccCode.Length > 20 || !long.TryParse(ssccCode, out _))
        {
            await DisplayAlert("Ошибка", "Введите корректный SSCC-код!", "OK");
            scanBoxEntry.Text = string.Empty;
            scanBoxEntry.Focus();
            _isProcessingScan = false;
            return;
        }

        var request = new BoxSSCCRequest
        {
            OrderId = _invoiceId,
            BoxSSCCCode = ssccCode,
            teNum = _teNum,
            UserName = _token // Assuming UserName is taken from token for this request.
        };

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("accept-boxes", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<AcceptBoxesResponse>();
                if (responseContent != null && responseContent.Message == "Boxes accepted")
                {
                    // Increment the counter
                    _scanCounter++;
                    counterLabel.Text = _scanCounter.ToString();
                    scanBoxEntry.Text = string.Empty;
                    scanBoxEntry.Focus();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось принять короб. Попробуйте снова.", "OK");
                    scanBoxEntry.Text = string.Empty;
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("Unauthorized", "Your session has expired. Please log in again.", "OK");
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось принять короб. Попробуйте снова.", "OK");
                scanBoxEntry.Text = string.Empty;
            }
        }
        catch (HttpRequestException httpRequestEx)
        {
            await DisplayAlert("HTTP Error", $"Request error: {httpRequestEx.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось принять короб: {ex.Message}", "OK");
        }
        finally
        {
            _isProcessingScan = false;
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        try
        {
            var request = new DeleteBoxSSCCRequest
            {
                teNum = _teNum,
                OrderId = _invoiceId
            };

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "delete-box-sscc")
            {
                Content = content
            };
            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<DeleteBoxSSCCResponse>();
                if (responseContent != null && responseContent.Message == "Boxes SSCC deleted")
                {
                    await DisplayAlert("Успех", "SSCC коробов успешно удалёны.", "OK");
                    _scanCounter = 0;
                    counterLabel.Text = _scanCounter.ToString();
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось удалить короб SSCC. Попробуйте снова.", "OK");
                }
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("Unauthorized", "Your session has expired. Please log in again.", "OK");
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось удалить короб SSCC. Попробуйте снова.", "OK");
            }
        }
        catch (HttpRequestException httpRequestEx)
        {
            await DisplayAlert("HTTP Error", $"Request error: {httpRequestEx.Message}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось удалить короб SSCC: {ex.Message}", "OK");
        }
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
}