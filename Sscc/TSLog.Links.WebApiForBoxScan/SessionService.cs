using System.Collections.Concurrent;

namespace TSLog.Links.WebApiForBoxScan;

/// <summary>
/// Класс для обслуживания аутентификации, авторизации на API.
/// </summary>
public class SessionService
{
    private readonly ConcurrentDictionary<string, bool> _loggedOutUsers = new();
    private static readonly Dictionary<string, string> _users = [];
    /// <summary>
    /// Очищает список пользователей.
    /// </summary>
    public static void ClearUsers()
    {
        _users.Clear();
    }
    /// <summary>
    /// Получает и кэширует список пользователей.
    /// </summary>
    /// <param name="users">Массив пользователей.</param>
    /// <exception cref="Exception"></exception>
    public static void InitUsers(User[] users)
    {
        foreach (var user in users)
        {
            var name = user.Name;
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("User name is empty!");
            var password = user.Password;
            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("User password is empty!");

            name = name.Trim().ToLower();
            if (_users.ContainsKey(name))
                throw new Exception("User {name} already exists!");
            password = password.Trim();

            _users.Add(name, password);
        }
    }
    /// <summary>
    /// Аутентификация пользователя. Мы просто из настроек тянем список пользователей с паролями.
    /// Сдалано "на коленке", хоть кака-то безопасность что бы была.
    /// </summary>
    /// <param name="userName">Имя пользователя.</param>
    /// <param name="password">Пароль.</param>
    /// <param name="domain">Домен.</param>
    /// <returns>Признак что пользователь вошёл.</returns>
    public static bool AuthenticateUser(string userName, string password, string domain)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return false;
        if (string.IsNullOrWhiteSpace(password))
            return false;
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        userName = userName.Trim().ToLower();
        if (!_users.TryGetValue(userName, out string? value))
            return false;

        password = password.Trim();
        if (value != password)
            return false;

        return true;
    }
    public void LogInUser(string userName)
    {
        _loggedOutUsers[userName] = false;
    }
    public void LogOutUser(string userName)
    {
        _loggedOutUsers[userName] = true;
    }
    public bool IsUserLoggedOut(string userName)
    {
        return _loggedOutUsers.TryGetValue(userName, out bool loggedOut) && loggedOut;
    }
    public static string NormalizeUserName(string? userName)
    {
        ArgumentNullException.ThrowIfNull(userName);
        
        // Remove domain part if present.
        var parts = userName.Split('\\');
        return parts.Length > 1 ? parts[1] : parts[0];
    }
}
