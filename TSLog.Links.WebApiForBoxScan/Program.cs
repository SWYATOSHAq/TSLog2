using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TSLog.Links.WebApiForBoxScan.Models;

namespace TSLog.Links.WebApiForBoxScan;

public class Program
{
    private static async Task<IResult> ValidateOrder(string orderId, OracleConnectionService dbService,
        HttpContext httpContext, SessionService sessionService)
    {
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
            return Results.Problem("User is missing");

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
            return Results.Problem("User is missing");

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
            return Results.Problem("User is missing");

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

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var secretKey = "S3cr3tK3yF0rJWTTokenGeneration2024!";

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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
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

        app.MapPost("/logoff", [Authorize] async (HttpContext httpContext, SessionService sessionService) =>
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
        });
        app.MapPost("/login", (LoginRequest loginRequest, SessionService sessionService) =>
        {
            string domain = "srv-apsk-0001.apsk.ru";
            bool isAuthenticated = AuthenticateUser(loginRequest.Username, loginRequest.Password, domain);
            if (isAuthenticated)
            {
                sessionService.LogInUser(loginRequest.Username); // Mark user as logged in
                var token = GenerateJwtToken(loginRequest.Username, secretKey);
                return Results.Ok(new { Message = "Authenticated", Token = token });
            }
            return Results.Unauthorized();
        });

        app.MapGet("/validate-order/{orderId}", ValidateOrder).RequireAuthorization();
        app.MapGet("/outgo-tus-count/{outgoId}", GetOutgoTusCount).RequireAuthorization();
        app.MapGet("/outgo-scanned-tus-count/{outgoId}", GetOutgoScannedTusCount).RequireAuthorization();
        app.MapGet("/outgo-scanned-boxes-count/{outgoId}", GetOutgoScannedBoxesCount).RequireAuthorization();

        // Define an endpoint to accept order and box SSCC codes.
        app.MapPost("/accept-boxes", async (BoxSSCCRequest request, OracleConnectionService dbService, HttpContext httpContext, SessionService sessionService) =>
        {
            var normalizedUsername = SessionService.NormalizeUsername(httpContext.User.Identity.Name);
            if (sessionService.IsUserLoggedOut(normalizedUsername))
            {
                return Results.Json(new { Message = "User is logged out" }, statusCode: 401);
            }

            try
            {
                await dbService.AcceptBoxSSCCasync(request.OrderId, request.BoxSSCCCode, request.teNum, normalizedUsername);
                return Results.Ok(new { Message = "Boxes accepted", OrderId = request.OrderId });
            }
            catch (Exception exception)
            {
                return Results.Problem(exception.Message);
            }
        }).RequireAuthorization();

        app.MapDelete("/delete-box-sscc", async ([FromBody] DeleteBoxSSCCRequest request, OracleConnectionService dbService, HttpContext httpContext, SessionService sessionService) =>
        {
            var normalizedUsername = SessionService.NormalizeUsername(httpContext.User.Identity.Name);
            if (sessionService.IsUserLoggedOut(normalizedUsername))
            {
                return Results.Json(new { Message = "User is logged out" }, statusCode: 401);
            }

            try
            {
                await dbService.DeleteBoxSSCCforTEAsync(request.teNum, request.OrderId);
                return Results.Ok(new { Message = "Boxes SSCC deleted", OrderId = request.OrderId });
            }
            catch (Exception exception)
            {
                return Results.Problem(exception.Message);
            }
        }).RequireAuthorization();

        app.Run();
    }

    // Method to authenticate the user against Active Directory.
    static bool AuthenticateUser(string username, string password, string domain)
    {
        // TODO: нужно настроить корректную аутентификацию, через Active Directory.
        return true;
    }

    static string GenerateJwtToken(string username, string secretKey)
    {
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, username)
            }),
            Expires = DateTime.UtcNow.AddHours(12),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
// Define the login request model.
public record LoginRequest(string Username, string Password);

// Define the order register request model.
public record BoxSSCCRequest(string OrderId, string BoxSSCCCode, string teNum, string UserName);

public record DeleteBoxSSCCRequest(string teNum, string OrderId);
