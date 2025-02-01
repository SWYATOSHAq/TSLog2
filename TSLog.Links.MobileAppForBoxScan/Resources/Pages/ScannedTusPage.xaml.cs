namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;

public partial class ScannedTusPage : ContentPage, IQueryAttributable
{
    private readonly HttpClient _httpClient;
    private string _outgoId = string.Empty;
    private string _userName = string.Empty;
    private string _token = string.Empty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.TryGetValue("OutgoId", out object? outgoIdValue))
            _outgoId = outgoIdValue.ToString() ?? string.Empty;
        else
            throw new Exception("OutgoId not foud!");
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
        var tuIds = await Api.GetOutgoScannedTus(_outgoId, this, _httpClient, _token);
        foreach (var tuId in tuIds)
        {
            var button = new Button()
            {
                StyleId = $"tu_{tuId}",
                Text = tuId,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Fill
            };
            button.Clicked += OnTeClicked;
            tusStackLayout.Children.Add(button);
        }
    }
    public ScannedTusPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
        BindingContext = this;
        versionLabel.Text = Common.Version;
    }

    private void SetControlsState()
    {
        deleteAllTus.IsEnabled = tusStackLayout.Children.Count > 0;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ShowScannedTus();
        SetControlsState();
    }

    private async void OnTeClicked(object sender, EventArgs e)
    {
        var tuId = ((Button)sender).StyleId;
        bool result = await DisplayAlert("Подтвердите действие.", $"Удалить ТЕ {tuId}?", "Да", "Нет");
        if (result)
        {
            // TODO: call api.
            var button = (Button)sender;
            tusStackLayout.Children.Remove(button);
            SetControlsState();
        }
    }
    private async void OnDeleteAllTusClicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Подтвердите действие.", $"Удалить ВСЕ ТЕ?", "Да", "Нет");
        if (result)
        {
            // TODO: call api.
            tusStackLayout.Children.Clear();
            SetControlsState();
        }
    }
    private async void OnBackClicked(object sender, EventArgs e)
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "OutgoId", _outgoId },
            { "Token", _token },
            { "UserName", _userName },
        };
        await Shell.Current.GoToAsync($"//{nameof(OutgoPage)}", navigationParameters);
    }
}
