using System.Collections.Concurrent;

namespace TSLog.Links.WebApiForBoxScan;

public class SessionService
{
    private readonly ConcurrentDictionary<string, bool> _loggedOutUsers = new();

    public void LogOutUser(string username)
    {
        _loggedOutUsers[username] = true;
    }

    public bool IsUserLoggedOut(string username)
    {
        return _loggedOutUsers.TryGetValue(username, out bool loggedOut) && loggedOut;
    }

    public void LogInUser(string username)
    {
        _loggedOutUsers[username] = false;
    }

    internal static string NormalizeUsername(string username)
    {
        // Remove domain part if present.
        var parts = username.Split('\\');
        return parts.Length > 1 ? parts[1] : parts[0];
    }
}
