using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TSLog.Links.WebApiForBoxScan.Models;

namespace TSLog.Links.WebApiForBoxScan;

public class Program
{
    private static readonly string SecretKey = "S3cr3tK3yF0rJWTTokenGeneration2024!";

    private static IResult Login(LoginRequest loginRequest, SessionService sessionService)
    {
        
        string domain = "srv-apsk-0001.apsk.ru";
        bool isAuthenticated = AuthenticateUser(loginRequest.UserName, loginRequest.Password, domain);
        if (isAuthenticated)
        {
            sessionService.LogInUser(loginRequest.UserName); // Mark user as logged in.
            var token = GenerateJwtToken(loginRequest.UserName, SecretKey);
            return Results.Ok(new { Message = "Authenticated", Token = token });
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
    private static async Task<IResult> ValidateOrder(string orderId, OracleConnectionService dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (httpContext.User == null || httpContext.User.Identity == null ||
            string.IsNullOrWhiteSpace(httpContext.User.Identity.Name))
            return Results.Problem("User is missing.");

        var normalizedUsername = SessionService.NormalizeUsername(httpContext.User.Identity.Name);
        if (sessionService.IsUserLoggedOut(normalizedUsername))
            return Results.Json(new { Message = "User is logged out" }, statusCode: 401);

        try
        {
            bool orderExists = await dbService.ValidateOrderAsync(orderId);
            if (orderExists)
                return Results.Ok(new { Message = "Order found", OrderId = orderId });
            else
                return Results.NotFound(new { Message = "Order not found", OrderId = orderId });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> GetOutgoTusCount(string outgoId, OracleConnectionService dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (httpContext.User == null || httpContext.User.Identity == null ||
            string.IsNullOrWhiteSpace(httpContext.User.Identity.Name))
            return Results.Problem("User is missing.");

        var userName = SessionService.NormalizeUsername(httpContext.User.Identity.Name);
        if (sessionService.IsUserLoggedOut(userName))
            return Results.Json(new { Message = "User is logged out" }, statusCode: 401);

        try
        {
            int tusCount = await dbService.GetOutgoTusCount(outgoId);
            return Results.Ok(new { Message = "Order found", OutgoId = outgoId, TusCount = tusCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> GetOutgoScannedTusCount(string outgoId, OracleConnectionService dbService,
    HttpContext httpContext, SessionService sessionService)
    {
        if (httpContext.User == null || httpContext.User.Identity == null ||
            string.IsNullOrWhiteSpace(httpContext.User.Identity.Name))
            return Results.Problem("User is missing.");

        var userName = SessionService.NormalizeUsername(httpContext.User.Identity.Name);
        if (sessionService.IsUserLoggedOut(userName))
            return Results.Json(new { Message = "User is logged out" }, statusCode: 401);

        try
        {
            int tusCount = await dbService.GetOutgoScannedTusCount(outgoId);
            return Results.Ok(new { Message = "Order found", OutgoId = outgoId, TusCount = tusCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> GetOutgoScannedBoxesCount(string outgoId, OracleConnectionService dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (httpContext.User == null || httpContext.User.Identity == null ||
            string.IsNullOrWhiteSpace(httpContext.User.Identity.Name))
            return Results.Problem("User is missing.");

        var userName = SessionService.NormalizeUsername(httpContext.User.Identity.Name);
        if (sessionService.IsUserLoggedOut(userName))
            return Results.Json(new { Message = "User is logged out" }, statusCode: 401);

        try
        {
            int tusCount = await dbService.GetOutgoScannedBoxesCount(outgoId);
            return Results.Ok(new { Message = "Order found", OutgoId = outgoId, TusCount = tusCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> AddOutgoScannedBox(BoxSSCCRequest request, OracleConnectionService dbService, 
        HttpContext httpContext, SessionService sessionService)
    {
        if (httpContext.User == null || httpContext.User.Identity == null ||
            string.IsNullOrWhiteSpace(httpContext.User.Identity.Name))
            return Results.Problem("User is missing.");

        var normalizedUsername = SessionService.NormalizeUsername(httpContext.User.Identity.Name);
        if (sessionService.IsUserLoggedOut(normalizedUsername))
        {
            return Results.Json(new { Message = "User is logged out." }, statusCode: 401);
        }

        try
        {
            await dbService.AcceptBoxSSCCasync(request.OrderId, request.BoxSSCCCode, request.TeNum, normalizedUsername);
            return Results.Ok(new { Message = "Boxes accepted", OrderId = request.OrderId });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> RemoveOutgoScannedBox([FromBody] DeleteBoxSSCCRequest request, OracleConnectionService dbService, HttpContext httpContext, SessionService sessionService)
    {
        if (httpContext.User == null || httpContext.User.Identity == null ||
            string.IsNullOrWhiteSpace(httpContext.User.Identity.Name))
            return Results.Problem("User is missing.");

        var normalizedUsername = SessionService.NormalizeUsername(httpContext.User.Identity.Name);
        if (sessionService.IsUserLoggedOut(normalizedUsername))
        {
            return Results.Json(new { Message = "User is logged out" }, statusCode: 401);
        }

        try
        {
            await dbService.DeleteBoxSSCCforTEAsync(request.TeNum, request.OrderId);
            return Results.Ok(new { Message = "Boxes SSCC deleted", OrderId = request.OrderId });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
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
        {
            builder.Services.Configure<OracleConnectionSettings>(builder.Configuration.GetSection("OracleConnectionDebugSettings"));
        }
        else
        {
            builder.Services.Configure<OracleConnectionSettings>(builder.Configuration.GetSection("OracleConnectionSettings"));
        }

        builder.Services.AddSingleton<OracleConnectionService>();
        builder.Services.AddSingleton<SessionService>();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapPost("/login", Login);
        app.MapPost("/logoff", Logout);

        // Define an endpoint to accept order and box SSCC codes.
        app.MapGet("/validate-order/{orderId}", ValidateOrder).RequireAuthorization();
        app.MapGet("/outgo-tus-count/{outgoId}", GetOutgoTusCount).RequireAuthorization();
        app.MapGet("/outgo-scanned-tus-count/{outgoId}", GetOutgoScannedTusCount).RequireAuthorization();
        app.MapGet("/outgo-scanned-boxes-count/{outgoId}", GetOutgoScannedBoxesCount).RequireAuthorization();
        app.MapPost("/accept-boxes", AddOutgoScannedBox).RequireAuthorization();
        app.MapDelete("/delete-box-sscc", RemoveOutgoScannedBox).RequireAuthorization();

        app.Run();
    }

    // Method to authenticate the user against Active Directory.
    static bool AuthenticateUser(string userName, string password, string domain)
    {
        // TODO: нужно настроить корректную аутентификацию, через Active Directory.
        return true;
    }

    static string GenerateJwtToken(string userName, string secretKey)
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
}
// Define the login request model.
public record LoginRequest(string UserName, string Password);

// Define the order register request model.
public record BoxSSCCRequest(string OrderId, string BoxSSCCCode, string TeNum, string UserName);

public record DeleteBoxSSCCRequest(string TeNum, string OrderId);
