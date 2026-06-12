using loukupm.Services;
using loukupm.View;
using Microsoft.Maui.Storage;

namespace loukupm.View;

public partial class LoadingPage : ContentPage
{
    private CancellationTokenSource _cts;

    public LoadingPage()
    {
        InitializeComponent();
        NavigationPage.SetHasNavigationBar(this, false);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _cts = new CancellationTokenSource();

        // Start animation safely
        _ = StartLogoAnimation(_cts.Token);

        // Initialize app flow
        await InitializeApp();
    }

    private async Task InitializeApp()
    {
        try
        {
            var startTime = DateTime.UtcNow;

            var token = await SecureStorage.GetAsync("auth_token");

            // 🎯 Minimum splash duration BEFORE navigation
            var minSplashTime = TimeSpan.FromSeconds(4);

            await Task.Delay(300); // UX buffer

            var elapsed = DateTime.UtcNow - startTime;
            if (elapsed < minSplashTime)
            {
                await Task.Delay(minSplashTime - elapsed);
            }

            Application.Current.MainPage = new AppShell();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    await Shell.Current.GoToAsync($"//{NavigationService.ROUTE_LOGIN}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"//{NavigationService.ROUTE_HOME}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LoadingPage] Initialization error: {ex.Message}");

            await Task.Delay(1500);

            Application.Current.MainPage =
                new NavigationPage(new LoginPage());
        }
    }
    private async Task StartLogoAnimation(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await LogoImage.ScaleTo(1.1, 800, Easing.CubicInOut);
                await LogoImage.ScaleTo(1.0, 800, Easing.CubicInOut);

                await LogoImage.FadeTo(1.0, 500);
                await LogoImage.FadeTo(0.7, 500);
            }
        }
        catch (TaskCanceledException)
        {
            // expected when page is disposed
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}