using TSLog.Links.MobileAppForBoxScan.Resources.Pages;

namespace TSLog.Links.MobileAppForBoxScan;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(OutgoPage), typeof(OutgoPage));
        Routing.RegisterRoute(nameof(OutgoScannedTusPage), typeof(OutgoScannedTusPage));
        Routing.RegisterRoute(nameof(TuPage), typeof(TuPage));
        Routing.RegisterRoute(nameof(TuScannedBoxesPage), typeof(TuScannedBoxesPage));
    }
}
