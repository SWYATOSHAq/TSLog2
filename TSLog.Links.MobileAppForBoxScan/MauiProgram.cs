using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
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

            var isDebug = builder.Configuration.GetValue<bool>("IsDebug");
            var apiSettings = builder.Configuration.GetSection("ApiSettings");

            // Register HttpClient with a custom handler that uses default credentials.
            builder.Services.AddSingleton<HttpClient>(sp =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                var client = new HttpClient(handler);
                var buaseUrl = apiSettings.GetValue<string>("BaseUrl") ?? string.Empty;
                var debugBaseUrl = apiSettings.GetValue<string>("DebugBaseUrl") ?? string.Empty;
                if (!isDebug)
                    client.BaseAddress = new Uri(buaseUrl); // Set the base address.
                else
                    client.BaseAddress = new Uri(debugBaseUrl); // Set the base address for debug.
                return client;
            });

            // Register pages.
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<OutgoPage>();
            builder.Services.AddTransient<SsccPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
