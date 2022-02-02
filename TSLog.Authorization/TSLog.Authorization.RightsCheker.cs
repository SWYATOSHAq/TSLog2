using System.Security.Claims;

namespace TSLog.Authorization
{
    public class RightsCheker
    {
        public bool CheckRight(string right, ClaimsPrincipal user)
        {
            //SecurityTokenService
            //ClaimTypes

            var a = user.IsInRole("redmine_users");
            return a;
        }
    }
}