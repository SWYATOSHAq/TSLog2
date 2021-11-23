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
using TSLog.Links.Db;

namespace Auth.Services
{
    public interface IUserService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model);
        LegalEntity GetById(string id);
        LegalEntity[] GetAll();
    }

    public class UserService : IUserService
    {

        private readonly AppSettings _appSettings;
        private ILinksUsers _databaseUsers;
        //private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _databaseUsers = DbLinksStorage.instance; // TODO: idk how to properly pass object to UserService
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _databaseUsers.Authenticate(model);


            // return null if user not found
            if (user == null || user == LegalEntity.Null) return null;

            // authentication successful so generate jwt token
            var token = generateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }

        public LegalEntity[] GetAll()
        {
            return _databaseUsers.GetAllUsers(); // TODO just for debug
        }

        public LegalEntity GetById(string id)
        {
            return _databaseUsers.GetLegalEntity(id);
        }

        // helper methods

        private string generateJwtToken(LegalEntity user)
        {
            // generate token that is valid for 365 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(1), // token is given for 1 day
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}