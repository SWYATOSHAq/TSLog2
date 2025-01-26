namespace TSLog.Links.MobileAppForBoxScan.Models;

public class LoginResponse
{
    /// <summary>
    /// Сообщение о результате.
    /// </summary>
    public required string Message { get; set; }
    /// <summary>
    /// Токен аутентификации.
    /// </summary>
    public required string Token { get; set; }
}
