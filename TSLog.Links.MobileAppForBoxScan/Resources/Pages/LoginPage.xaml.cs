using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TSLog.Links.MobileAppForBoxScan.Models;

namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages
{
    /// <summary>
    /// Страница входа в приложение.
    /// </summary>
    public partial class LoginPage : ContentPage
    {
        private readonly HttpClient _httpClient;

        public LoginPage(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
            versionLabel.Text = Common.Version;
        }

        private void OnTextChenged(object sender, TextChangedEventArgs e)
        {
            SetControlsState();
        }
        private void OnUserNameEntryCompleted(object? sender, EventArgs e)
        {
            passwordEntry.Focus();
        }
        private void OnPasswordEntryCompleted(object? sender, EventArgs e)
        {
            loginButton.Focus();
        }
        private void SetControlsState()
        {
            loginButton.IsEnabled = !string.IsNullOrWhiteSpace(userNameEntry.Text) &&
                !string.IsNullOrWhiteSpace(passwordEntry.Text);
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            ClearEntries();
            FocusUserNameEntry();
        }

        private void ClearEntries()
        {
            userNameEntry.Text = string.Empty;
            passwordEntry.Text = string.Empty;
        }

        private void FocusUserNameEntry()
        {
            // Use the Dispatcher to ensure the UI is ready before setting the focus.
            Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                userNameEntry.Focus();
                return false; // Run the timer callback only once.
            });
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            loginButton.Text = "⌛ Выполняется вход...";
            var userName = userNameEntry.Text;
            var password = passwordEntry.Text;
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Ошибка", "Введите логин и пароль", "OK");
                loginButton.Text = "Войти";
                return;
            }

            var loginRequest = new { UserName = userName, Password = password };
            var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PostAsync("login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (responseContent == null || string.IsNullOrWhiteSpace(responseContent.Token))
                    {
                        await DisplayAlert("Ошибка", $"Не удалось выполнить вход.", "OK");
                        return;
                    }

                    var parameters = new Dictionary<string, object>
                    {
                        { "UserName", userName },
                        { "Token", responseContent.Token }
                    };
                    await Shell.Current.GoToAsync($"///{nameof(OutgoPage)}", parameters);
                }
                else
                    await DisplayAlert("Ошибка", "Неверный логин или пароль", "OK");
            }
            catch (HttpRequestException exception)
            {
                await DisplayAlert("HTTP Error", $"Request error: {exception.Message}", "OK");
            }
            catch (Exception exception)
            {
                await DisplayAlert("Ошибка", $"Не удалось выполнить вход: {exception.Message}", "OK");
            }
            finally
            {
                passwordEntry.Text = string.Empty;
                loginButton.Text = "Войти";
            }
        }
        private async void OnQuitClicked(object sender, EventArgs e)
        {
            Application.Current.Quit();
        }
    }
}
