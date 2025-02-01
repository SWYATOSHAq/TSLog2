using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TSLog.Links.MobileAppForBoxScan.Models;
using TSLog.Links.MobileAppForBoxScan.Resources.Pages;

namespace TSLog.Links.MobileAppForBoxScan
{
    /// <summary>
    /// "Обёртка" для вызова API функций.
    /// </summary>
    public class Api
    {
        private static async Task<bool> CheckToken(string token, ContentPage page)
        {
            ArgumentNullException.ThrowIfNull(page);
            if (string.IsNullOrWhiteSpace(token))
            {
                await page.DisplayAlert("Ошибка", "Токен отсутствует. Пожалуйста, войдите снова.", "OK");
                await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
                return false;
            }
            return true;
        }
        private static async void HandleUnauthorized(ContentPage page)
        {
            ArgumentNullException.ThrowIfNull(page);
            await page.DisplayAlert("Ошибка",
                "Ваша сессия истекла. Пожалуйста, войдите снова.", "OK");
            await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
        }
        /// <summary>
        /// Проверяет расход (заказ) на корректность - наличие ТЕ у расхода и не нулевое количество коробов.
        /// </summary>
        /// <param name="outgoId">Идентификатор расхода (заказа).</param>
        /// <param name="page">Страница (UI).</param>
        /// <param name="client">HTTP клиент.</param>
        /// <param name="token">Токен авторизации.</param>
        /// <returns>Признак "корректного" расхода (с ТЕ и с ненулевым количеством коробов).</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<bool> CheckOutgo(string outgoId,
            ContentPage page, HttpClient client, string token)
        {
            if (string.IsNullOrWhiteSpace(outgoId))
                throw new ArgumentNullException(nameof(outgoId));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));
            try
            {
                if (!await CheckToken(token, page))
                    return false;

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync($"check-outgo/{outgoId}");
                if (response.StatusCode == HttpStatusCode.OK)
                    return true;
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    await page.DisplayAlert("Ошибка", "Заказ не найден.", "OK");
                    return false;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HandleUnauthorized(page);
                    return false;
                }
                else
                {
                    await page.DisplayAlert("Ошибка", "Заказ не найден.", "OK");
                    return false;
                }

            }
            catch (HttpRequestException exception)
            {
                await page.DisplayAlert("HTTP ошибка", $"Ошибка запроса: {exception.Message}.", "OK");
            }
            catch (Exception exception)
            {
                await page.DisplayAlert("Ошибка", $"Ошибка:{exception.Message}.", "OK");
            }
            return false;
        }
        /// <summary>
        /// Проверяет ТЕ расхода (заказа) на корректность - наличие коробов на ТЕ.
        /// </summary>
        /// <param name="outgoId">Идентификатор расхода (заказа).</param>
        /// <param name="tuId">Идентификатор ТЕ.</param>
        /// <param name="page">Страница (UI).</param>
        /// <param name="client">HTTP клиент.</param>
        /// <param name="token">Токен авторизации.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<bool> CheckOutgoTu(string outgoId, string tuId,
            ContentPage page, HttpClient client, string token)
        {
            if (string.IsNullOrWhiteSpace(outgoId))
                throw new ArgumentNullException(nameof(outgoId));
            if (string.IsNullOrWhiteSpace(tuId))
                throw new ArgumentNullException(nameof(tuId));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(client);
            try
            {
                if (!await CheckToken(token, page))
                    return false;

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync($"check-outgo-tu/{outgoId}/{tuId}");
                if (response.StatusCode == HttpStatusCode.OK)
                    return true;
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    await page.DisplayAlert("Ошибка", "ТЕ в заказе не найдена.", "OK");
                    return false;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HandleUnauthorized(page);
                    return false;
                }
                else
                {
                    await page.DisplayAlert("Ошибка", "ТЕ в заказе не найдена.", "OK");
                    return false;
                }

            }
            catch (HttpRequestException exception)
            {
                await page.DisplayAlert("HTTP ошибка", $"Ошибка запроса: {exception.Message}.", "OK");
            }
            catch (Exception exception)
            {
                await page.DisplayAlert("Ошибка", $"Ошибка:{exception.Message}.", "OK");
            }
            return false;
        }
        /// <summary>
        /// Возврашает количество ТЕ, которые связаны с расходом (заказом).
        /// </summary>
        /// <param name="outgoId">Идентификатор расхода (заказа).</param>
        /// <param name="page">Страница (UI).</param>
        /// <param name="client">HTTP клиент.</param>
        /// <param name="token">Токен авторизации.</param>
        /// <returns>Количество ТЕ, в случае ошибки возвращает 0.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<int> GetOutgoTusCount(string outgoId,
            ContentPage page, HttpClient client, string token)
        {
            if (string.IsNullOrWhiteSpace(outgoId))
                throw new ArgumentNullException(nameof(outgoId));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(client);
            try
            {
                if (!await CheckToken(token, page))
                    return 0;

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync($"outgo-tus-count/{outgoId}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<OutgoTusCountResponse>();
                    if (responseContent != null && responseContent.TusCount > 0)
                        return responseContent.TusCount;
                    else
                    {
                        await page.DisplayAlert("Ошибка", "В заказе отсутствуют ТЕ.", "OK");
                        return 0;
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HandleUnauthorized(page);
                    return 0;
                }
                else
                    await page.DisplayAlert("Ошибка", "Ошибка.", "OK");
            }
            catch (HttpRequestException exception)
            {
                await page.DisplayAlert("HTTP ошибка", $"Ошибка запроса: {exception.Message}.", "OK");
            }
            catch (Exception exception)
            {
                await page.DisplayAlert("Ошибка", $"Ошибка:{exception.Message}.", "OK");
            }
            return 0;
        }
        /// <summary>
        /// Возвращает количество коробов на ТЕ.
        /// </summary>
        /// <param name="tuId">Идентификатор ТЕ.</param>
        /// <param name="page">Страница (UI).</param>
        /// <param name="client">HTTP клиент.</param>
        /// <param name="token">Токен авторизации.</param>
        /// <returns>Количество коробов на ТЕ, 0 в случае ошмбки.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<int> GetTuBoxesCount(string tuId,
            ContentPage page, HttpClient client, string token)
        {
            if (string.IsNullOrWhiteSpace(tuId))
                throw new ArgumentNullException(nameof(tuId));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(client);
            try
            {
                if (!await CheckToken(token, page))
                    return 0;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync($"tu-boxes-count/{tuId}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<TuBoxesCountResponse>();
                    if (responseContent != null && responseContent.BoxesCount > 0)
                        return responseContent.BoxesCount;
                    else
                    {
                        await page.DisplayAlert("Ошибка", "На ТЕ нет коробок.", "OK");
                        return 0;
                    }
                }
                else
                {
                    await page.DisplayAlert("Ошибка", "На ТЕ нет коробок.", "OK");
                    return 0;
                }
            }
            catch (HttpRequestException exception)
            {
                await page.DisplayAlert("HTTP ошибка", $"Ошибка запроса: {exception.Message}.", "OK");
            }
            catch (Exception exception)
            {
                await page.DisplayAlert("Ошибка", $"Ошибка:{exception.Message}.", "OK");
            }
            return 0;
        }
        /// <summary>
        /// Возвращает количество ОТСКАНИРОВАННЫХ коробов на ТЕ.
        /// </summary>
        /// <param name="tuId">Идентификатор ТЕ.</param>
        /// <param name="page">Страница (UI).</param>
        /// <param name="client">HTTP клиент.</param>
        /// <param name="token">Токен авторизации.</param>
        /// <returns>Количество отсканированных коробов на ТЕ. 0 - допустимое значение, -1 в случае ошибки.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<int> GetScannedTuBoxesCount(string tuId,
            ContentPage page, HttpClient client, string token)
        {
            if (string.IsNullOrWhiteSpace(tuId))
                throw new ArgumentNullException(nameof(tuId));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(client);
            try
            {
                if (!await CheckToken(token, page))
                    return -1;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync($"tu-scanned-boxes-count/{tuId}");
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<TuScannedBoxesCountResponse>();
                    if (responseContent != null)
                        return responseContent.BoxesCount;
                    else
                    {
                        await page.DisplayAlert(
                            "Ошибка", "Не могу определить количество отсканированных коробов на ТЕ.",
                            "OK");
                        return -1;
                    }
                }
                else
                {
                    await page.DisplayAlert(
                        "Ошибка", "Не могу определить количество отсканированных коробов на ТЕ.",
                        "OK");
                    return -1;
                }
            }
            catch (HttpRequestException exception)
            {
                await page.DisplayAlert("HTTP ошибка", $"Ошибка запроса: {exception.Message}.", "OK");
            }
            catch (Exception exception)
            {
                await page.DisplayAlert("Ошибка", $"Ошибка:{exception.Message}.", "OK");
            }
            return -1;
        }
        /// <summary>
        /// Сохраняет набор (может быть несколько, это важно!) SSCC.
        /// </summary>
        /// <param name="ssccs">Массив SSCC.</param>
        /// <param name="outgoId">Идентификатор расхода (заказа).</param>
        /// <param name="tuId">Идентификатор ТЕ.</param>
        /// <param name="userName">Имя пользователя.</param>
        /// <param name="page">Страница (UI).</param>
        /// <param name="client">HTTP клиент.</param>
        /// <param name="token">Токен авторизации.</param>
        /// <returns>Возвращает признак успешного сохранения.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<bool> StoreSsccs(string[] ssccs, string outgoId, string tuId, string userName,
            ContentPage page, HttpClient client, string token)
        {
            if (ssccs == null || ssccs.Length <= 0)
                throw new ArgumentException($"{nameof(ssccs)} is null or empty.");
            if (string.IsNullOrWhiteSpace(outgoId))
                throw new ArgumentNullException(nameof(outgoId));
            if (string.IsNullOrWhiteSpace(tuId))
                throw new ArgumentNullException(nameof(tuId));
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentNullException(nameof(userName));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(client);
            try
            {
                if (!await Api.CheckToken(token, page))
                    return false;

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var request = new AddSsccsRequest
                {
                    OutgoId = outgoId,
                    TuId = tuId,
                    Ssccs = ssccs,
                    UserName = userName
                };
                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("add-ssccs", content);
                if (response.StatusCode == HttpStatusCode.OK)
                    return true;
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Api.HandleUnauthorized(page);
                    return false;
                }
                else
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    if (responseContent != null)
                        await page.DisplayAlert("Ошибка", responseContent.Detail, "OK");
                    else
                        await page.DisplayAlert("Ошибка", "Не удалось принять короба. Попробуйте снова.", "OK");
                    return false;
                }
            }
            catch (HttpRequestException exception)
            {
                await page.DisplayAlert("HTTP Ошибка", $"Ошибка HTTP: {exception.Message}", "OK");
            }
            catch (Exception exception)
            {
                await page.DisplayAlert("Ошибка", $"Ошибка: {exception.Message}", "OK");
            }
            return false;
        }


        public static async Task<bool> RemoveOutgoSsccs(string outgoId,
            ContentPage page, HttpClient client, string token)
        {
            if (string.IsNullOrWhiteSpace(outgoId))
                throw new ArgumentNullException(nameof(outgoId));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(client);
            try
            {
                if (!await Api.CheckToken(token, page))
                    return false;

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.DeleteAsync("/remove-outgo-ssccs/{outgoId}");
                if (response.StatusCode == HttpStatusCode.OK)
                    return true;
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Api.HandleUnauthorized(page);
                    return false;
                }
                else
                {
                    var responseContent = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    if (responseContent != null)
                        await page.DisplayAlert("Ошибка", responseContent.Detail, "OK");
                    else
                        await page.DisplayAlert("Ошибка", "Не удалось удалить короба. Попробуйте снова.", "OK");
                    return false;
                }
            }
            catch (HttpRequestException exception)
            {
                await page.DisplayAlert("HTTP Ошибка", $"Ошибка HTTP: {exception.Message}", "OK");
            }
            catch (Exception exception)
            {
                await page.DisplayAlert("Ошибка", $"Ошибка: {exception.Message}", "OK");
            }
            return false;
        }
    }
}
