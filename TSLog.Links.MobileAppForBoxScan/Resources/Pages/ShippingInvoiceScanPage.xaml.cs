using System.Net.Http;

namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;

public partial class ShippingInvoiceScanPage : ContentPage
{
    private readonly HttpClient _httpClient;
    public ShippingInvoiceScanPage(HttpClient httpClient)
	{
		InitializeComponent();
        _httpClient = httpClient;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Use the Dispatcher to ensure the UI is ready before setting the focus
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
        {
            invoiceEntry.Focus();
            return false; // Run the timer callback only once
        });
    }

    private async void OnExitClicked(object sender, EventArgs e)
    {
        try
        {
            var response = await _httpClient.PostAsync("https://10.50.252.33:7278/logoff", null);
            if (response.IsSuccessStatusCode)
            {
                //await DisplayAlert("Успех", "Выход выполнен успешно", "OK");
                // Clear any stored user state and navigate to the login page
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