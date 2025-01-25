using System.Collections.Concurrent;

namespace TSLog.Links.WebApiForBoxScan;

public class SessionService
{
    private readonly ConcurrentDictionary<string, bool> _loggedOutUsers = new();

    public void LogOutUser(string userName)
    {
        _loggedOutUsers[userName] = true;
    }

    public bool IsUserLoggedOut(string userName)
    {
        return _loggedOutUsers.TryGetValue(userName, out bool loggedOut) && loggedOut;
    }

    public void LogInUser(string userName)
    {
        _loggedOutUsers[userName] = false;
    }

    internal static string NormalizeUsername(string userName)
    {
        // Remove domain part if present.
        var parts = userName.Split('\\');
        return parts.Length > 1 ? parts[1] : parts[0];
    }
}
