using TSLog.Links.BLCtobacco.Interfaces;

namespace TSLog.Links.BLCtobacco.Services;

public class HttpClientService : IHttpClientService
{
    private readonly HttpClient _httpClient;

    public HttpClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> SendGetRequestAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task SendPostRequestAsync(string url, object data)
    {
        var jsonContent = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(data),
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(url, jsonContent);
        response.EnsureSuccessStatusCode();
    }
}
