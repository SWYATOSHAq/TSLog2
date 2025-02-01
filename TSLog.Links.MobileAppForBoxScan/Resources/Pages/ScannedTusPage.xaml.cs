using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

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
        if (query.TryGetValue("UserName", out object? userNameValue))
            _userName = userNameValue.ToString() ?? string.Empty;
        if (query.TryGetValue("Token", out object? tokenValue))
            _token = tokenValue.ToString() ?? string.Empty;
    }
    private void ShowScannedTus()
    {
        var tuIds = new List<string>();
        tuIds.Add("TU_0001");
        tuIds.Add("TU_0002");
        tuIds.Add("TU_0003");

        foreach (var tuId in tuIds)
        {
            Button button = new Button
            {
                StyleId = $"tu_{tuId}",
                Text = tuId,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
            };
            button.Clicked += OnTeClicked;
            tesStackLayout.Children.Add(button);
        }
    }
    public ScannedTusPage(HttpClient httpClient)
    {
        InitializeComponent();
        _httpClient = httpClient;
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ShowScannedTus();
    }

    private async void OnTeClicked(object sender, EventArgs e)
    {
        var tuId = ((Button)sender).StyleId;
        bool result = await DisplayAlert("Подтвердите действие.", $"Удалить ТЕ {tuId}?", "Да", "Нет");
        if (result)
        {
            // TODO: delete selected.
        }
    }
    private async void OnDeleteAllTusClick(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Подтвердите действие.", $"Удалить ВСЕ ТЕ?", "Да", "Нет");
        if (result)
        {
            // TODO: delete all.
        }
    }
    private async void OnBackClick(object sender, EventArgs e)
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
