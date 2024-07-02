using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.DirectoryServices.AccountManagement;
using System.Text;
using System.Text.Json;
using TSLog.Links.WebApiForBoxScan.Models;
using System.Security.Claims;

namespace TSLog.Links.WebApiForBoxScan;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        string secretKey = "S3cr3tK3yF0rJWTTokenGeneration2024!";

        //// Add services to the container.
        //builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Negotiate.NegotiateDefaults.AuthenticationScheme)
        //    .AddNegotiate();
        //builder.Services.AddAuthorization();
        //builder.Services.AddCors(options =>
        //{
        //    options.AddPolicy("AllowAll",
        //        builder =>
        //        {
        //            builder.AllowAnyOrigin()
        //                   .AllowAnyMethod()
        //                   .AllowAnyHeader();
        //        });
        //});

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,// falsw
                ValidateAudience = false,// falsw
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                //ValidIssuer = "yourdomain.com",
                //ValidAudience = "yourdomain.com",
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

        // Determine whether to use debug settings
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


        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

        //app.MapGet("/login", [Authorize] (HttpContext context) =>
        //{
        //    var user = context.User;
        //    if (user.Identity != null && user.Identity.IsAuthenticated)
        //    {
        //        return Results.Ok(new { Username = user.Identity.Name });
        //    }
        //    return Results.Unauthorized();
        //});



        // Define the login endpoint
        //app.MapPost("/login", (LoginRequest loginRequest, SessionService sessionService) =>
        //{
        //    bool isAuthenticated = AuthenticateUser(loginRequest.Username, loginRequest.Password);
        //    if (isAuthenticated)
        //    {
        //        sessionService.LogInUser(loginRequest.Username); // Mark user as logged in
        //        return Results.Ok(new { Message = "Authenticated", Username = loginRequest.Username });
        //    }
        //    return Results.Unauthorized();
        //});

        //app.MapPost("/logoff", async (HttpContext httpContext, SessionService sessionService) =>
        //{
        //    using var reader = new StreamReader(httpContext.Request.Body);
        //    var body = await reader.ReadToEndAsync();
        //    var loginRequest = JsonSerializer.Deserialize<LoginRequest>(body);

        //    if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username))
        //    {
        //        return Results.BadRequest(new { Message = "Invalid request" });
        //    }

        //    sessionService.LogOutUser(loginRequest.Username); // Mark user as logged out
        //    return Results.Ok(new { Message = "Logged off" });
        //});

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
            //string domain = "apsk.ru";// Replace with your actual domain
            bool isAuthenticated = AuthenticateUser(loginRequest.Username, loginRequest.Password, domain);
            if (isAuthenticated)
            {
                sessionService.LogInUser(loginRequest.Username); // Mark user as logged in
                var token = GenerateJwtToken(loginRequest.Username, secretKey);
                return Results.Ok(new { Message = "Authenticated", Token = token });
            }
            return Results.Unauthorized();
        });

        //app.MapPost("/logoff", (LoginRequest loginRequest, SessionService sessionService) =>
        //{
        //    sessionService.LogOutUser(loginRequest.Username); // Mark user as logged out
        //    return Results.Ok(new { Message = "Logged off" });
        //});

        app.MapGet("/validate-order/{orderId}", async (string orderId, OracleConnectionService dbService,
            HttpContext httpContext, SessionService sessionService) =>
        {
            var normalizedUsername = sessionService.NormalizeUsername(httpContext.User.Identity.Name);
            if (sessionService.IsUserLoggedOut(normalizedUsername))
            {
                return Results.Json(new { Message = "User is logged out" }, statusCode: 401);
            }

            try
            {
                bool orderExists = await dbService.ValidateOrderAsync(orderId);
                if (orderExists)
                {
                    return Results.Ok(new { Message = "Order found", OrderId = orderId });
                }
                else
                {
                    return Results.NotFound(new { Message = "Order not found", OrderId = orderId });
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }).RequireAuthorization();

        // Define an endpoint to accept order and box SSC� codes
        app.MapPost("/accept-boxes", async (BoxSSCCRequest request, OracleConnectionService dbService, HttpContext httpContext, SessionService sessionService) =>
        {
            var normalizedUsername = sessionService.NormalizeUsername(httpContext.User.Identity.Name);
            if (sessionService.IsUserLoggedOut(normalizedUsername))
            {
                return Results.Json(new { Message = "User is logged out" }, statusCode: 401);
            }

            try
            {
                await dbService.AcceptBoxSSCCasync(request.OrderId, request.BoxSSCCCode, request.teNum, normalizedUsername);
                return Results.Ok(new { Message = "Boxes accepted", OrderId = request.OrderId });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }).RequireAuthorization();

        app.MapDelete("/delete-box-sscc", async ([FromBody] DeleteBoxSSCCRequest request, OracleConnectionService dbService, HttpContext httpContext, SessionService sessionService) =>
        {
            var normalizedUsername = sessionService.NormalizeUsername(httpContext.User.Identity.Name);
            if (sessionService.IsUserLoggedOut(normalizedUsername))
            {
                return Results.Json(new { Message = "User is logged out" }, statusCode: 401);
            }

            try
            {
                await dbService.DeleteBoxSSCCforTEAsync(request.teNum, request.OrderId);
                return Results.Ok(new { Message = "Boxes SSCC deleted", OrderId = request.OrderId });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }).RequireAuthorization();



        app.Run();
    }



    // Method to authenticate the user against Active Directory
    static bool AuthenticateUser(string username, string password, string domain)
    {
        //TODO: нужно настроить корректную аутентификацию, через Active Directory.

        //try
        //{
        //    using (var context = new PrincipalContext(ContextType.Domain, domain))
        //    {
        //        Console.WriteLine($"ContextType: {context.ContextType}, Domain: {domain}");
        //        return context.ValidateCredentials(username, password);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    // Log or handle exceptions as needed
        //    Console.WriteLine($"Error authenticating user: {ex.Message}");
        //    return false;
        //}

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
            //Issuer = "yourdomain.com",
            //Audience = "yourdomain.com",
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

