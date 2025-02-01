using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;
/// <summary>
/// Общая функциональность для работы с UI.
/// </summary>
class UICommon
{
    /// <summary>
    /// Выход из приложения.
    /// </summary>
    /// <param name="page">Текущая "страница".</param>
    /// <param name="client">HTTP клиент.</param>
    /// <param name="token">Токен безопасности.</param>
    public static async void ExitApplication(ContentPage page, HttpClient client, string token)
    {
        try
        {
            var loginRequest = new { Token = token };
            var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.PostAsync("logoff", content);
            if (response.IsSuccessStatusCode)
                await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
            else
                await page.DisplayAlert("Ошибка", "Не удалось выполнить выход.", "OK");
        }
        catch (HttpRequestException exception)
        {
            await page.DisplayAlert("HTTP Error", $"Request error: {exception.Message}", "OK");
        }
        catch (Exception exception)
        {
            await page.DisplayAlert("Ошибка", $"Не удалось выполнить выход: {exception.Message}", "OK");
        }
    }
}
