using TSLog.Links.MobileAppForBoxScan.Resources.Pages;

namespace TSLog.Links.MobileAppForBoxScan
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(OutgoPage), typeof(OutgoPage));
			Routing.RegisterRoute(nameof(ScannedTusPage), typeof(ScannedTusPage));
            Routing.RegisterRoute(nameof(SsccPage), typeof(SsccPage));
            Routing.RegisterRoute(nameof(ScannedTuBoxesPage), typeof(ScannedTuBoxesPage));
        }
    }
}
