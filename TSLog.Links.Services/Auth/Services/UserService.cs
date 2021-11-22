using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

//using Auth.Entities;
using Auth.Helpers;
using Auth.Models;
using TSLog.Links;
using TSLog.BO;

namespace Auth.Services
{
    public interface IUserService
    {
        //AuthenticateResponse Authenticate(AuthenticateRequest model);
        //User GetById(int id);
    }

    public class UserService : IUserService
    {
        //// users hardcoded for simplicity, store in a db with hashed passwords in production applications
        //private List<User> _users = new List<User>
        //{
        //    new User { Id = 1, PartnerId = "Test", PartnerName = "User", Name = "admin", Password = "1" }

        //    // TODO: load from db
        //};

        private readonly AppSettings _appSettings;
        ////private readonly AppSettings _appSettings;

        //public UserService(IOptions<AppSettings> appSettings)
        //{
        //    _appSettings = appSettings.Value;
        //}

        //public AuthenticateResponse Authenticate(AuthenticateRequest model)
        //{
        //    var user = _users.SingleOrDefault(x => x.Name == model.Username && x.Password == model.Password);

        //    // TODO: realize
        //    //DB.loadLegalEntityByHashedPass(model.Username, model.Password);

        //    // return null if user not found
        //    if (user == null) return null;

        //    // authentication successful so generate jwt token
        //    var token = generateJwtToken(user);

        //    return new AuthenticateResponse(user, token);
        //}


        //public User GetById(int id)
        //{
        //    return _users.FirstOrDefault(x => x.Id == id);
        //    // TODO: realize
        //}

        //// helper methods

        private string generateJwtToken(LegalEntity user)
        {
            // generate token that is valid for 365 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(365), // token is given for 1 year
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}