namespace TSLog.Links.WebApiForBoxScan;
/// <summary>
/// Пользователь API (нужно для аутентификации).
/// </summary>
public class User
{
    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Пароль.
    /// </summary>
    public required string Password { get; set; }
}
