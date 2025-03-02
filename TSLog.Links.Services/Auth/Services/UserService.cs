using Auth.Helpers;
using Auth.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TSLog.LegalEntities.BO;
using TSLog.Links;
using TSLog.Links.Data;

namespace Auth.Services;

public interface IUserService
{
    AuthenticateResponse Authenticate(AuthenticateRequest model);
    LegalEntity GetById(string id);
    LegalEntity[] GetAll();
}

public class UserService : IUserService
{
    private readonly AppSettings _appSettings;
    private readonly ILinksUsers _databaseUsers;

    public UserService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
        _databaseUsers = LinksOracleStorage.Instance; // TODO: idk how to properly pass object to UserService.
    }

    public AuthenticateResponse Authenticate(AuthenticateRequest model)
    {
        var user = _databaseUsers.Authenticate(model);

        // Return null if user not found.
        if (user == null || user == LegalEntity.Null) return null;

        // Authentication successful so generate jwt token.
        var token = GenerateJwtToken(user);

        return new AuthenticateResponse(user, token);
    }

    public LegalEntity[] GetAll()
    {
        return _databaseUsers.GetAllUsers(); // TODOL: just for debug.
    }

    public LegalEntity GetById(string id)
    {
        return _databaseUsers.GetLegalEntity(id);
    }

    // Helper methods.
    private string GenerateJwtToken(LegalEntity user)
    {
        // Generate token that is valid for 365 days.
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
            Expires = DateTime.UtcNow.AddDays(1), // Token is given for 1 day.
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
