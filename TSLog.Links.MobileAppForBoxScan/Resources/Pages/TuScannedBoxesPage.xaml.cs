namespace TSLog.Links.MobileAppForBoxScan.Resources.Pages;

/// <summary>
/// Страница отсканированных коробов для ТЕ.
/// </summary>
public partial class TuScannedBoxesPage : ContentPage, IQueryAttributable
{
    private readonly HttpClient _httpClient;
    private string _outgoId = string.Empty;
    private string _tuId = string.Empty;
    private string _userName = string.Empty;
    private string _token = string.Empty;
    private readonly ISet<string> _scannedSsccs = new HashSet<string>();

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
        if (query.TryGetValue("ScannedSsccs", out object? scannedSsccsValue))
        {
            var temp = (string[])scannedSsccsValue;
            foreach (var sscc in temp)
                _scannedSsccs.Add(sscc);
        }
            
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
        void ShowButton(string sscc, Color color)
        {
            var button = new Button()
            {
                StyleId = sscc,
                Text = sscc,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = color
            };
            button.Clicked += OnBoxClicked;
            boxesStackLayout.Children.Add(button);
        }

        var storedSsccs = await Api.GetTuScannedBoxes(_tuId, this, _httpClient, _token);
        foreach (var storedSscc in storedSsccs)
            ShowButton(storedSscc, Colors.Green);

        foreach (var scannedSscc in _scannedSsccs)
            ShowButton(scannedSscc, Colors.LightBlue);
    }
    public TuScannedBoxesPage(HttpClient httpClient)
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
        var button = (Button)sender;
        var sscc = button.StyleId;
        bool result = await DisplayAlert("Подтвердите действие.", $"Удалить короб {sscc}?", "Да", "Нет");
        if (result)
        {
            if (_scannedSsccs.Contains(sscc))
            {
                _scannedSsccs.Remove(sscc);
                boxesStackLayout.Children.Remove(button);
            }
            else if (await Api.RemoveSscc(sscc, this, _httpClient, _token))
                boxesStackLayout.Children.Remove(button);
            SetControlsState();
        }
    }
    private async void OnDeleteAllBoxesClicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Подтвердите действие.", $"Удалить ВСЕ короба?", "Да", "Нет");
        if (result)
        {
            if (await Api.RemoveTuSsccs(_tuId, this, _httpClient, _token))
                boxesStackLayout.Children.Clear();
            _scannedSsccs.Clear();
            SetControlsState();
        }
    }
    private async void OnBackClicked(object sender, EventArgs e)
    {
        var navigationParameters = new Dictionary<string, object>
        {
            { "OutgoId", _outgoId },
            { "TuId", _tuId },
            { "ScannedSsccs", _scannedSsccs.ToArray() },
            { "Token", _token },
            { "UserName", _userName },
        };
        await Shell.Current.GoToAsync($"{nameof(TuPage)}", navigationParameters);
    }
}
