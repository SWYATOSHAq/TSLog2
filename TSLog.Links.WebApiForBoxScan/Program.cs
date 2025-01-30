using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TSLog.Links.WebApiForBoxScan.Models;

namespace TSLog.Links.WebApiForBoxScan;

public class Program
{
    private static readonly string SecretKey = "S3cr3tK3yF0rJWTTokenGeneration2024!";
    private static readonly string Domain = "srv-apsk-0001.apsk.ru";

    private static bool CheckUser(HttpContext httpContext, SessionService sessionService, out IResult result)
    {
        if (httpContext.User == null || httpContext.User.Identity == null ||
            string.IsNullOrWhiteSpace(httpContext.User.Identity.Name))
        {
            result = Results.Problem("User is missing.");
            return false;
        }
            
        var normalizedUsername = SessionService.NormalizeUsername(httpContext.User.Identity.Name);
        if (sessionService.IsUserLoggedOut(normalizedUsername))
        {
            result = Results.Json(new { Message = "User is logged out." }, statusCode: 401);
            return false;
        }

        result = null;
        return true;
    }

    private static IResult Login(LoginRequest loginRequest, SessionService sessionService)
    {
        bool isAuthenticated = AuthenticateUser(loginRequest.UserName, loginRequest.Password, Domain);
        if (isAuthenticated)
        {
            sessionService.LogInUser(loginRequest.UserName); // Mark user as logged in.
            var token = GenerateJwtToken(loginRequest.UserName);
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

    private static async Task<IResult> CheckOutgo(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            bool outgoExists = await dbService.CheckOutgoAsync(outgoId);
            if (outgoExists)
                return Results.Ok(new { Message = "Outgo found.", outgoId });
            else
                return Results.NotFound(new { Message = "Outgo not found.", outgoId });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> CheckOutgoTu(string outgoId, string tuId, 
        OracleDbWorker dbService, HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            bool outgoExists = await dbService.CheckOutgoTuAsync(outgoId, tuId);
            if (outgoExists)
                return Results.Ok(new { Message = "TU for outgo found.", outgoId, tuId });
            else
                return Results.NotFound(new { Message = "TU for outgo not found.", outgoId, tuId });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> GetOutgoTusCount(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            int tusCount = await dbService.GetOutgoTusCountAsync(outgoId);
            return Results.Ok(new { Message = "Outgo found.", outgoId, tusCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> GetOutgoBoxesCount(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            int boxesCount = await dbService.GetOutgoBoxesCountAsync(outgoId);
            return Results.Ok(new { Message = "Outgo found.", outgoId, boxesCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> GetTuBoxesCount(string tuId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            int boxesCount = await dbService.GetTuBoxesCountAsync(tuId);
            return Results.Ok(new { Message = "TU found.", tuId, boxesCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> GetOutgoScannedTusCount(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            int tusCount = await dbService.GetOutgoScannedTusCountAsync(outgoId);
            return Results.Ok(new { Message = "Outgo found.", outgoId, tusCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> GetOutgoScannedBoxesCount(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            int boxesCount = await dbService.GetOutgoScannedBoxesCountAsync(outgoId);
            return Results.Ok(new { Message = "Outgo found.", outgoId, boxesCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> GetTuScannedBoxesCount(string tuId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            int boxesCount = await dbService.GetTuScannedBoxesCountAsync(tuId);
            return Results.Ok(new { Message = "TU found.", tuId, boxesCount });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> AddSscc(AddSsccRequest request,
        OracleDbWorker dbService, HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            var normalizedUsername = SessionService.NormalizeUsername(httpContext.User.Identity.Name);
            await dbService.AddSsccAsync(request.OutgoId, request.TuId, request.Sscc, normalizedUsername);
            return Results.Ok(new
            {
                Message = "Box added.",
                outgoId = request.OutgoId,
                tuId = request.TuId,
                boxSscc = request.Sscc
            });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> AddSsccs(AddSsccsRequest request,
       OracleDbWorker dbService, HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            var normalizedUsername = SessionService.NormalizeUsername(httpContext.User.Identity.Name);
            await dbService.AddSsccsAsync(request.OutgoId, request.TuId, request.Ssccs, normalizedUsername);
            return Results.Ok(new
            {
                Message = "Boxes added.",
                outgoId = request.OutgoId,
                tuId = request.TuId,
                ssccsCount = request.Ssccs.Length
            });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> RemoveOutgoSsccs(string outgoId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            await dbService.RemoveOutgoSsccsAsync(outgoId);
            return Results.Ok(new { Message = "Outgo SSCCs removed.", outgoId });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> RemoveTuSsccs(string tuId, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            await dbService.RemoveTuSsccsAsync(tuId);
            return Results.Ok(new { Message = "TU SSCCs removed.", tuId });
        }
        catch (Exception exception)
        {
            return Results.Problem(exception.Message);
        }
    }

    private static async Task<IResult> RemoveSscc(string sscc, OracleDbWorker dbService,
        HttpContext httpContext, SessionService sessionService)
    {
        if (!CheckUser(httpContext, sessionService, out var result))
            return result;

        try
        {
            await dbService.RemoveSsccAsync(sscc);
            return Results.Ok(new { Message = "SSCC removed.", sscc });
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
            builder.Services.Configure<OracleConnectionSettings>(builder.Configuration.GetSection("OracleConnectionDebugSettings"));
        else
            builder.Services.Configure<OracleConnectionSettings>(builder.Configuration.GetSection("OracleConnectionSettings"));

        builder.Services.AddSingleton<OracleDbWorker>();
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
        app.MapGet("/check-outgo/{outgoId}", CheckOutgo).RequireAuthorization();
        app.MapGet("/check-outgo-tu/{outgoId}/{tuId}", CheckOutgoTu).RequireAuthorization();
        app.MapGet("/outgo-tus-count/{outgoId}", GetOutgoTusCount).RequireAuthorization();
        app.MapGet("/outgo-boxes-count/{outgoId}", GetOutgoBoxesCount).RequireAuthorization();
        app.MapGet("/tu-boxes-count/{tuId}", GetTuBoxesCount).RequireAuthorization();
        app.MapGet("/outgo-scanned-tus-count/{outgoId}", GetOutgoScannedTusCount).RequireAuthorization();
        app.MapGet("/outgo-scanned-boxes-count/{outgoId}", GetOutgoScannedBoxesCount).RequireAuthorization();
        app.MapGet("/tu-scanned-boxes-count/{tuId}", GetTuScannedBoxesCount).RequireAuthorization();
        app.MapPost("/add-sscc", AddSscc).RequireAuthorization();
        app.MapPost("/add-ssccs", AddSsccs).RequireAuthorization();
        app.MapDelete("/remove-outgo-ssccs/{outgoId}", RemoveOutgoSsccs).RequireAuthorization();
        app.MapDelete("/remove-tu-ssccs/{tuId}", RemoveTuSsccs).RequireAuthorization();
        app.MapDelete("/remove-sscc/{sscc}", RemoveSscc).RequireAuthorization();

        app.Run();
    }

    // Method to authenticate the user against Active Directory.
    private static bool AuthenticateUser(string userName, string password, string domain)
    {
        // TODO: нужно настроить корректную аутентификацию, через Active Directory.
        return !string.IsNullOrWhiteSpace(userName) &&
            !string.IsNullOrWhiteSpace(password) &&
            !string.IsNullOrWhiteSpace(domain);
    }

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
}

public record LoginRequest(string UserName, string Password);
public record AddSsccRequest(string OutgoId, string TuId, string Sscc, string UserName);
public record AddSsccsRequest(string OutgoId, string TuId, string[] Ssccs, string UserName);
