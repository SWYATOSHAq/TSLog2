using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TSLog.Links.MobileAppForBoxScan.Models;

namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;

public partial class SsccPage : ContentPage, IQueryAttributable
{
    private readonly HttpClient _httpClient;
    private string _token = string.Empty;
    private string _outgoId = string.Empty;
    private string _tuId = string.Empty;
    private string _userName = string.Empty;
    private int _scanCounter;
    private bool _isProcessingScan;

    public SsccPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
        ssccEntry.Completed += OnSsccEntryCompleted;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Use the Dispatcher to ensure the UI is ready before setting the focus.
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
        {
            ssccEntry.Focus();
            return false; // Run the timer callback only once.
        });
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.TryGetValue("UserName", out object? userNameValue))
            _userName = userNameValue.ToString() ?? string.Empty;
        if (query.TryGetValue("Token", out object? tokenValue))
            _token = tokenValue.ToString() ?? string.Empty;
    }

    private async void OnSsccEntryCompleted(object? sender, EventArgs e)
    {
        if (_isProcessingScan)
            return;

        _isProcessingScan = true;
        var sscc = ssccEntry.Text;
        // TODO: если так, то на сервере нужно проверять, что бы входили только цифры.
        sscc = Regex.Replace(sscc, "[^0-9]", "");
        if (string.IsNullOrWhiteSpace(sscc) || sscc.Length != 20 || !long.TryParse(sscc, out _))
        {
            await DisplayAlert("Ошибка", "Введите корректный SSCC!", "OK");
            ssccEntry.Text = string.Empty;
            ssccEntry.Focus();
            _isProcessingScan = false;
            return;
        }

        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var request = new AddSsccRequest
            {
                OutgoId = _outgoId,
                TuId = _tuId,
                Sscc = sscc,
                UserName = _userName
            };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("add-sscc", content);
            if (response.IsSuccessStatusCode)
            {
                _scanCounter++;
                counterLabel.Text = _scanCounter.ToString();
                ssccEntry.Text = string.Empty;
                ssccEntry.Focus();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("Unauthorized", "Your session has expired. Please log in again.", "OK");
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось принять короб. Попробуйте снова.", "OK");
                ssccEntry.Text = string.Empty;
            }
        }
        catch (HttpRequestException exception)
        {
            await DisplayAlert("HTTP Error", $"Request error: {exception.Message}", "OK");
        }
        catch (Exception exception)
        {
            await DisplayAlert("Ошибка", $"Не удалось принять короб: {exception.Message}", "OK");
        }
        finally
        {
            _isProcessingScan = false;
        }
    }

    private async void OnRemoveSsccClicked(object? sender, EventArgs e)
    {
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
                _scanCounter--;
                counterLabel.Text = _scanCounter.ToString();
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("Unauthorized", "Your session has expired. Please log in again.", "OK");
                await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
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
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        UICommon.ExitApplication(this, _httpClient, _token);
    }
}
