using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TSLog.Links.WebApiForBoxScan.Models;

namespace TSLog.Links.WebApiForBoxScan;

public record LoginRequest(string UserName, string Password);
public record AddSsccRequest(string OutgoId, string TuId, string Sscc, string UserName);
public record AddSsccsRequest(string OutgoId, string TuId, string[] Ssccs, string UserName);

public class Program
{
    private static readonly string SecretKey = "S3cr3tK3yF0rJWTTokenGeneration2024!";
    private static readonly string Domain = "srv-apsk-0001.apsk.ru";
    private static string GenerateJwtToken(string userName)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(SecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new(ClaimTypes.Name, userName)
            ]),
            Expires = DateTime.UtcNow.AddHours(12),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    private static IResult Login(LoginRequest loginRequest, SessionService sessionService)
    {
        bool isAuthenticated = SessionService.AuthenticateUser(loginRequest.UserName, loginRequest.Password, Domain);
        if (isAuthenticated)
        {
            sessionService.LogInUser(loginRequest.UserName); // Mark user as logged in.
            var token = GenerateJwtToken(loginRequest.UserName);
            return Results.Ok(new { Message = "Authenticated.", Token = token });
        }
        return Results.Unauthorized();
    }
    [Authorize]
    private static IResult Logout(HttpContext httpContext, SessionService sessionService)
    {
        if (httpContext.User.Identity is ClaimsIdentity identity)
        {
            var username = identity.FindFirst(ClaimTypes.Name)?.Value;
            if (username != null)
            {
                sessionService.LogOutUser(username);
                return Results.Ok(new { Message = "Logged off" });
            }
        }
        return Results.BadRequest(new { Message = "Invalid request" });
    }

    private static void DefineEndpoints(WebApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.MapGet("/check-outgo/{outgoId}", Service.CheckOutgo).RequireAuthorization();
        application.MapGet("/check-outgo-tu/{outgoId}/{tuId}", Service.CheckOutgoTu).RequireAuthorization();
        application.MapGet("/outgo-tus-count/{outgoId}", Service.GetOutgoTusCount).RequireAuthorization();
        application.MapGet("/outgo-boxes-count/{outgoId}", Service.GetOutgoBoxesCount).RequireAuthorization();
        application.MapGet("/tu-boxes-count/{tuId}", Service.GetTuBoxesCount).RequireAuthorization();
        application.MapGet("/outgo-scanned-tus-count/{outgoId}", Service.GetOutgoScannedTusCount).RequireAuthorization();
        application.MapGet("/outgo-scanned-tus/{outgoId}", Service.GetOutgoScannedTus).RequireAuthorization();
        application.MapGet("/outgo-scanned-boxes-count/{outgoId}", Service.GetOutgoScannedBoxesCount).RequireAuthorization();
        application.MapGet("/outgo-scanned-boxes/{outgoId}", Service.GetOutgoScannedBoxes).RequireAuthorization();
        application.MapGet("/tu-scanned-boxes-count/{tuId}", Service.GetTuScannedBoxesCount).RequireAuthorization();
        application.MapGet("/tu-scanned-boxes/{tuId}", Service.GetTuScannedBoxes).RequireAuthorization();
        application.MapPost("/add-sscc", Service.AddSscc).RequireAuthorization();
        application.MapPost("/add-ssccs", Service.AddSsccs).RequireAuthorization();
        application.MapDelete("/remove-outgo-ssccs/{outgoId}", Service.RemoveOutgoSsccs).RequireAuthorization();
        application.MapDelete("/remove-tu-ssccs/{tuId}", Service.RemoveTuSsccs).RequireAuthorization();
        application.MapDelete("/remove-sscc/{sscc}", Service.RemoveSscc).RequireAuthorization();
    }
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey))
            };
        });

        builder.Services.AddAuthorization();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
        });

        var isDebug = builder.Configuration.GetValue<bool>("IsDebug");

        // Register Oracle connection settings dynamically based on the IsDebug flag.
        if (isDebug)
            builder.Services.Configure<OracleConnectionSettings>(
                builder.Configuration.GetSection("OracleConnectionDebugSettings"));
        else
            builder.Services.Configure<OracleConnectionSettings>(
                builder.Configuration.GetSection("OracleConnectionSettings"));

        builder.Services.AddSingleton<OracleDbWorker>();
        builder.Services.AddSingleton<SessionService>();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var users = builder.Configuration.GetSection("Users").Get<User[]>() ?? 
            throw new Exception("Users is null!");
        SessionService.ClearUsers();
        SessionService.InitUsers(users);

        var application = builder.Build();

        // Configure the HTTP request pipeline.
        if (application.Environment.IsDevelopment())
        {
            application.UseSwagger();
            application.UseSwaggerUI();
        }

        application.UseHttpsRedirection();
        application.UseCors("AllowAll");
        application.UseAuthentication();
        application.UseAuthorization();

        application.MapPost("/login", Login);
        application.MapPost("/logoff", Logout);

        DefineEndpoints(application);

        application.Run();
    }
}
