namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;

public partial class ShippingInvoiceScanPage : ContentPage
{
	public ShippingInvoiceScanPage()
	{
		InitializeComponent();
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Use the Dispatcher to ensure the UI is ready before setting the focus
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
        {
            invoiceEntry.Focus();
            return false; // Run the timer callback only once
        });
    }
}