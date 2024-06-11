using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using System.DirectoryServices.AccountManagement;

namespace TSLog.Links.WebApiForBoxScan;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Negotiate.NegotiateDefaults.AuthenticationScheme)
            .AddNegotiate();
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
        app.MapPost("/login", (LoginRequest loginRequest) =>
        {
            bool isAuthenticated = AuthenticateUser(loginRequest.Username, loginRequest.Password);
            if (isAuthenticated)
            {
                return Results.Ok(new { Message = "Authenticated", Username = loginRequest.Username });
            }
            return Results.Unauthorized();
        });

        app.MapPost("/logoff", () =>
        {
            // Here you would typically clear authentication cookies or tokens.
            // Since we're using Negotiate, there's not much to do here.
            return Results.Ok(new { Message = "Logged off" });
        });

        // Define the scan data endpoint
        //app.MapPost("/submit-scan", [Authorize] async (ScanData scanData, IConfiguration config) =>
        //{
        //    string connString = config.GetConnectionString("OracleDb");
        //    using (var conn = new OracleConnection(connString))
        //    {
        //        await conn.OpenAsync();
        //        using (var cmd = new OracleCommand("YOUR_STORED_PROCEDURE_NAME", conn))
        //        {
        //            cmd.CommandType = System.Data.CommandType.StoredProcedure;
        //            cmd.Parameters.Add("P_ORDER_ID", OracleDbType.Varchar2).Value = scanData.OrderId;
        //            cmd.Parameters.Add("P_BOX_ID", OracleDbType.Varchar2).Value = scanData.BoxId;

        //            // Execute the stored procedure
        //            await cmd.ExecuteNonQueryAsync();
        //        }
        //    }

        //    return Results.Ok(new { Message = "Scan data submitted successfully" });
        //});

        //app.MapGet("/weatherforecast", (HttpContext httpContext) =>
        //{
        //    var forecast = Enumerable.Range(1, 5).Select(index =>
        //        new WeatherForecast
        //        {
        //            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //            TemperatureC = Random.Shared.Next(-20, 55),
        //            Summary = summaries[Random.Shared.Next(summaries.Length)]
        //        })
        //        .ToArray();
        //    return forecast;
        //})
        //.WithName("GetWeatherForecast")
        //.WithOpenApi();

        app.Run();
    }

    // Method to authenticate the user against Active Directory
    static bool AuthenticateUser(string username, string password)
    {
        using (var context = new PrincipalContext(ContextType.Domain))
        {
            return context.ValidateCredentials(username, password);
        }
    }
}
// Define the login request model
public record LoginRequest(string Username, string Password);

public record ScanData(string OrderId, string BoxId);
