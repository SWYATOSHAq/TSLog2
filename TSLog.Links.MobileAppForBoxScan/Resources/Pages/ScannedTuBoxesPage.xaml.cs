namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;

public partial class ScannedTuBoxesPage : ContentPage, IQueryAttributable
{
    private readonly HttpClient _httpClient;
    private string _outgoId = string.Empty;
    private string _tuId = string.Empty;
    private string _userName = string.Empty;
    private string _token = string.Empty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.TryGetValue("OutgoId", out object? outgoIdValue))
            _outgoId = outgoIdValue.ToString() ?? string.Empty;
        else
            throw new Exception("OutgoId not foud");
        if (query.TryGetValue("TuId", out object? outgoTuIdValue))
            _tuId = outgoTuIdValue.ToString() ?? string.Empty;
        else
            throw new Exception("TuId not foud");
        if (query.TryGetValue("UserName", out object? userNameValue))
            _userName = userNameValue.ToString() ?? string.Empty;
        else
            throw new Exception("UserName not foud!");
        if (query.TryGetValue("Token", out object? tokenValue))
            _token = tokenValue.ToString() ?? string.Empty;
        else
            throw new Exception("Token not foud!");
    }
    private async Task ShowScannedTus()
    {
        var ssccs = await Api.GetTuScannedBoxes(_tuId, this, _httpClient, _token);
        foreach (var sscc in ssccs)
        {
            var button = new Button()
            {
                StyleId = $"sscc_{sscc}",
                Text = sscc,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Fill
            };
            button.Clicked += OnBoxClicked;
            boxesStackLayout.Children.Add(button);
        }
    }
    public ScannedTuBoxesPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
        BindingContext = this;
        versionLabel.Text = Common.Version;
    }

    private void SetControlsState()
    {
        deleteAllBoxes.IsEnabled = boxesStackLayout.Children.Count > 0;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ShowScannedTus();
        SetControlsState();
    }

    private async void OnBoxClicked(object sender, EventArgs e)
    {
        var sscc = ((Button)sender).StyleId;
        bool result = await DisplayAlert("Подтвердите действие.", $"Удалить короб {sscc}?", "Да", "Нет");
        if (result)
        {
            // TODO: call api.
            var button = (Button)sender;
            boxesStackLayout.Children.Remove(button);
            SetControlsState();
        }
    }
    private async void OnDeleteAllBoxesClicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Подтвердите действие.", $"Удалить ВСЕ короба?", "Да", "Нет");
        if (result)
        {
            // TODO: call api.
            boxesStackLayout.Children.Clear();
            SetControlsState();
        }
    }
    private async void OnBackClicked(object sender, EventArgs e)
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "OutgoId", _outgoId },
            { "TuId", _tuId },
            { "Token", _token },
            { "UserName", _userName },
        };
        await Shell.Current.GoToAsync($"{nameof(SsccPage)}", navigationParameters);
    }
}
