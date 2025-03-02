using Auth.Helpers;
using Auth.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security;
using TSLog.Common.BO;
using TSLog.Links.Data;
using TSLog.Links.Gs1EdiXml;
using TSLog.Partners.BO;

namespace TSLog.Links.Services;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();
        services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
        services.AddScoped<IUserService, UserService>();

        InitSingletons(services);

        var temp = services.AddControllers(options =>
        {
            options.RespectBrowserAcceptHeader = true; // False by default.
        });
        temp.AddXmlSerializerFormatters();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();
        app.UseHttpsRedirection();
        app.UseRouting();
        // Global cors policy.
        app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.UseAuthorization();
        // Custom jwt auth middleware.
        app.UseMiddleware<JwtMiddleware>();
        //
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    private void InitSingletons(IServiceCollection services)
    {
        // Init storage.
        // This storage is then used to get data from DB
        // information on how data access is then split is in ILinksStorage interface.

        var dataSource = Configuration.GetValue<string>("ConnectionSettings:DataSource");
        var storage = new LinksOracleStorage(dataSource);

        storage.CommonSchema = Configuration.GetValue<string>("ConnectionSettings:CommonSchema");
        storage.WmsSchema = Configuration.GetValue<string>("ConnectionSettings:WmsSchema");
        storage.LinksSchema = Configuration.GetValue<string>("ConnectionSettings:LinksSchema");

        storage.IncomeStates = new Catalog<Entity>("INBOUND_STATES", "—осто€и€ приходов");
        storage.OutgoStates = new Catalog<Entity>("OUTBOUND_STATES", "—осто€и€ расходов");
        storage.Partners = new Catalog<Partner>("PARTNERS", "ѕартнЄры");
        storage.Currencies = new Catalog<Entity>("CURRENCIES", "валюты");

        storage.Products = new();

        var login = Configuration.GetValue<string>("ConnectionSettings:Login");
        var password = Configuration.GetValue<string>("ConnectionSettings:Password");

        var securePassword = new SecureString();
        foreach (var symbol in password)
            securePassword.AppendChar(symbol);
        password = string.Empty;
        storage.Connect(login, securePassword);

        storage.IncomeStates.Add(storage.GetIncomeStates());
        storage.OutgoStates.Add(storage.GetOutgoStates());
        storage.Currencies.Add(storage.GetCurrncies());

        var xmlConverter = new Gs1EdiXmlConverter();
        xmlConverter.LinksStorage = storage;

        services.AddSingleton<ILinksCommonStorage>(storage);
        services.AddSingleton(xmlConverter);
    }
}
