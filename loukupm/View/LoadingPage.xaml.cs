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
        // Prevent any navigation until initialization completes.
        try
        {
            // Ensure AppShell registers routes before navigation
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Keep LoadingPage visible until we replace MainPage
            });

            await Task.Delay(500); // small delay for UI stability

            if (string.IsNullOrWhiteSpace(token))
            {
                // Navigate to LoginPage as the new MainPage wrapped in NavigationPage
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
            else
            {
                // Authenticated: set AppShell as MainPage (Shell)
                Application.Current.MainPage = new AppShell();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LoadingPage] Initialization error: {ex.Message}");
            // Fallback to LoginPage on any initialization error
            Application.Current.MainPage = new NavigationPage(new LoginPage());
        }
    }
}