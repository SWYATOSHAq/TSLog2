using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using TSLog.Authorization;
using TSLog.Common.Middleware;
using TSLog.Yard;
using TSLog.Yard.BO;
using TSLog.Yard.WebUI.Data;

namespace TSLog.Yard.WebUI;
public class Program
{
    public static void Main(string[] args)
    {
        const string DefaultCommonSchema = "common";
        const string DefaultStaffSchema = "staff";
        const string DefaultLegalEntitiesSchema = "legal_entities";
        const string DefaultPartnersSchema = "partners";
        const string DefaultYardSchema = "yard";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Log.Information("Starting web application");

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services
                .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            builder.Services
                .AddDatabaseDeveloperPageExceptionFilter();
            builder.Services
                .AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            var useAuthentication = builder.Configuration.GetValue<bool>("UseAuthentication");
            if (useAuthentication)
            {
                builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
                builder.Services.AddAuthorization(options => { options.FallbackPolicy = options.DefaultPolicy; });
            }

            builder.Services.AddRazorPages();
            builder.Services.AddMvc().AddRazorPagesOptions(options =>
            {
                options.RootDirectory = "/View";
            });

            var isDebug = builder.Configuration.GetValue<bool>("IsDebug");

            var oracleConnectionSettings = isDebug
                ? builder.Configuration.GetSection("OracleConnectionDebugSettings")
                : builder.Configuration.GetSection("OracleConnectionSettings");
            var oracleTns = oracleConnectionSettings.GetValue<string>("DataSource") ?? string.Empty;
            var oracleUserName = oracleConnectionSettings.GetValue<string>("UserName") ?? string.Empty;
            var oraclePassword = oracleConnectionSettings.GetValue<string>("Password") ?? string.Empty;
            var oracleCommonSchema = oracleConnectionSettings.GetValue<string>("CommonSchema") ?? DefaultCommonSchema;
            var oracleStaffSchema = oracleConnectionSettings.GetValue<string>("StaffSchema") ?? DefaultStaffSchema;
            var oracleLegalEntitiesSchema = oracleConnectionSettings.GetValue<string>("LegalEntitiesSchema")?? DefaultLegalEntitiesSchema;
            var oraclePartnersSchema = oracleConnectionSettings.GetValue<string>("PartnersSchema") ?? DefaultPartnersSchema;

            var pgsqlConnectionSettings = isDebug
                ? builder.Configuration.GetSection("PostgreSqlConnectionDebugSettings")
                : builder.Configuration.GetSection("PostgreSqlConnectionSettings");
            var pgsqlHost = pgsqlConnectionSettings.GetValue<string>("Host") ?? string.Empty;
            var pgsqlUserName = pgsqlConnectionSettings.GetValue<string>("UserName") ?? string.Empty;
            var pgsqlPassword = pgsqlConnectionSettings.GetValue<string>("Password") ?? string.Empty;
            var pgsqlDatabase = pgsqlConnectionSettings.GetValue<string>("Database") ?? string.Empty;
            var pgsqlPooling = pgsqlConnectionSettings.GetValue<bool>("Pooling");
            var pgsqlMaxPoolSize = pgsqlConnectionSettings.GetValue<int>("MaxPoolSize");
            var pgsqlConnectionIdleLifetime = pgsqlConnectionSettings.GetValue<int>("ConnectionIdleLifetime");
            var pgsqlConnectionPruningInterval = pgsqlConnectionSettings.GetValue<int>("ConnectionPruningInterval");
            var pgsqlYardSchema = pgsqlConnectionSettings.GetValue<string>("YardSchema") ?? DefaultYardSchema;
            var extensionParameters = new Dictionary<string, object>
            {
                { "Pooling", pgsqlPooling },
                { "MaxPoolSize", pgsqlMaxPoolSize },
                { "ConnectionIdleLifetime", pgsqlConnectionIdleLifetime },
                { "ConnectionPruningInterval", pgsqlConnectionPruningInterval }
            };

            var storage = new YardStorage(oracleTns, pgsqlHost, pgsqlDatabase, extensionParameters);
            storage.MaxRowsCount = builder.Configuration.GetValue<int>("MaxRowsCount");

            storage.ConnectOracle(oracleUserName, oraclePassword);
            storage.SetOracleCommonSchema(oracleCommonSchema);
            storage.SetOracleStaffSchema(oracleStaffSchema);
            storage.SetOracleLegalEntitiesSchema(oracleLegalEntitiesSchema);
            storage.SetOraclePartnersSchema(oraclePartnersSchema);
            storage.ConnectPgSql(pgsqlUserName, pgsqlPassword);
            storage.SetPgsqlYardSchema(pgsqlYardSchema);

            builder.Services.AddSingleton<IYardStorage>(storage);

            var rightsChecker = new RightsChecker();
            builder.Services.AddSingleton<IRightsChecker>(rightsChecker);
            var programFileName = Assembly.GetExecutingAssembly().Location;
            var programDirectory = Path.GetDirectoryName(programFileName) ?? throw new Exception("Rights file not found");
            var rightsFileName = Path.Combine(programDirectory, "rights.xml");
            var userPartnersFileName = Path.Combine(programDirectory, "userPartners.xml");
            rightsChecker.LoadRights(rightsFileName);
            rightsChecker.LoadUserPartners(userPartnersFileName);

            var app = builder.Build();

            app.UseSerilogRequestLogging();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
                app.UseMigrationsEndPoint();
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            if (useAuthentication)
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }

            // Map api.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            app.MapRazorPages();
            app.Run();
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}