namespace TSLog.Links.MobileAppForBoxScan.Models;

/// <summary>
/// Модель HTTP ответа для входа в приложение.
/// </summary>
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
