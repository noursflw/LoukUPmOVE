using Microsoft.Maui.Storage;
using loukupm.View;

namespace loukupm.View;

public partial class LoadingPage : ContentPage
{
    public LoadingPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Delay(300); 

        var token = await SecureStorage.GetAsync("auth_token");

        if (string.IsNullOrWhiteSpace(token))
        {
           
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
        else
        {
            
            Application.Current.MainPage = new AppShell();
        }
    }
}