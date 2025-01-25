using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TSLog.Links.MobileAppForBoxScan.Models;
using TSLog.Links.MobileAppForBoxScan.Resources.Pages;

namespace TSLog.Links.MobileAppForBoxScan
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Load configuration from appsettings.json.
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "TSLog.Links.MobileAppForBoxScan.appsettings.json";
            using var stream = assembly.GetManifestResourceStream(resourceName) ??
                throw new InvalidOperationException($"Resource '{resourceName}' not found.");
            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            // Add configuration to the builder.
            builder.Configuration.AddConfiguration(config);

            var apiSettings = new ApiSettings();
            builder.Configuration.GetSection("ApiSettings").Bind(apiSettings);
            builder.Services.AddSingleton(apiSettings);

            // Register HttpClient with a custom handler that uses default credentials.
            builder.Services.AddSingleton<HttpClient>(sp =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                var client = new HttpClient(handler);
                client.BaseAddress = new Uri(apiSettings.BaseUrl); // Set the base address.
                return client;
            });

            // Register pages.
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<ShippingInvoiceScanPage>();
            builder.Services.AddTransient<ScanBoxPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
