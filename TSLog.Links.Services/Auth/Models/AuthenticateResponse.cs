
using TSLog.BO;
using TSLog.Links;

namespace Auth.Models
{
    public class AuthenticateResponse
    {
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string Token { get; set; }


        public AuthenticateResponse(LegalEntity user, string token)
        {
            PartnerId = user.Code;
            PartnerName = user.Name;
            Token = token;
        }
    }
}