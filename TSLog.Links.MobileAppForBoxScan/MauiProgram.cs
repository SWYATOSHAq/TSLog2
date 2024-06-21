using Microsoft.Extensions.Logging;
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

            // Register HttpClient with a custom handler that uses default credentials
            builder.Services.AddSingleton<HttpClient>(sp =>
            {
                var handler = new HttpClientHandler
                {
                    //UseDefaultCredentials = true,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                return new HttpClient(handler);
            });

            // Register ILogger
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddDebug();
            });

            // Register pages
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
