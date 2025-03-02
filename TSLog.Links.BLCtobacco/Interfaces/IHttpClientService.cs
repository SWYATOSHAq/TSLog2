namespace TSLog.Links.BLCtobacco.Interfaces;
public interface IHttpClientService
{
    Task<string> SendGetRequestAsync(string url);
    Task SendPostRequestAsync(string url, object data);
}
