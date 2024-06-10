using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;

namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages
{
    public partial class MainPage : ContentPage
    {
        private readonly HttpClient _httpClient;

        public MainPage()
        {
            InitializeComponent();

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Use the Dispatcher to ensure the UI is ready before setting the focus
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                usernameEntry.Focus();
                return false; // Run the timer callback only once
            });
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            loginButton.Text = "⌛ Выполняется вход...";

            var username = usernameEntry.Text;
            var password = passwordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Ошибка", "Введите логин и пароль", "OK");
                loginButton.Text = "Войти";
                return;
            }

            // Ping the host to check connection
            //var pingResult = await PingHost("10.50.252.33");

            //if (!pingResult)
            //{
            //    await DisplayAlert("Ошибка", "Не удалось подключиться к хосту", "OK");
            //    loginButton.Text = "Войти";
            //    return;
            //}

            var loginRequest = new { Username = username, Password = password };
            var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("https://10.50.252.33:7278/login", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // Handle successful login, for example navigate to another page
                    //await DisplayAlert("Успех", "Вход выполнен успешно", "OK");
                    //await Shell.Current.GoToAsync("ShippingInvoiceScanPage");
                    await Shell.Current.GoToAsync($"///{nameof(ShippingInvoiceScanPage)}");
                    //await Navigation.PushAsync(new ShippingInvoiceScanPage());
                }
                else
                {
                    await DisplayAlert("Ошибка", "Неверный логин или пароль", "OK");
                }
            }
            catch (HttpRequestException httpRequestEx)
            {
                await DisplayAlert("HTTP Error", $"Request error: {httpRequestEx.Message}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось выполнить вход: {ex.Message}", "OK");
            }
            finally
            {
                loginButton.Text = "Войти";
            }
        }

        //private async Task<bool> PingHost(string host)
        //{
        //    try
        //    {
        //        using var ping = new Ping();
        //        var reply = await ping.SendPingAsync(host);

        //        return reply.Status == IPStatus.Success;
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("Ping Error", $"Ping failed: {ex.Message}", "OK");
        //        return false;
        //    }
        //}
    }

}
