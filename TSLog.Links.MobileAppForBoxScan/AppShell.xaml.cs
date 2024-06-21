using TSLog.Links.MobileAppForBoxScan.Resources.Pages;

namespace TSLog.Links.MobileAppForBoxScan
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ShippingInvoiceScanPage), typeof(ShippingInvoiceScanPage));
            Routing.RegisterRoute(nameof(ScanBoxPage), typeof(ScanBoxPage));
        }
    }
}
