
using TSLog.Links;

namespace Auth.Models
{
    public class AuthenticateResponse
    {
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string Name { get; set; }
        public string Token { get; set; }


        public AuthenticateResponse(User user, string token)
        {
            PartnerId = user.PartnerId;
            PartnerName = user.PartnerName;
            Name = user.Name;
            Token = token;
        }
    }
}