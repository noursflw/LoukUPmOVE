using CommunityToolkit.Maui.Views;
using Firebase.Auth;
using loukupm.Model;
using Firebase.Auth.Providers;
using loukupm.services;
using loukupm.View.MassgingApp;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static loukupm.Model.Auth;
using System.Diagnostics.CodeAnalysis;

namespace loukupm.View;

public partial class LoginPage : ContentPage
{
    public static bool IsLogged { get; set; } = false;
    private UserCredential userCredential;

    private string redirectUri;

    public LoginPage()
    {
        InitializeComponent();
        webView.Navigated += WebView_Navigated;
        webView.UserAgent = "Mozilla/5.0 (Linux; Android 8.0; Pixel 2 Build/OPD3.170816.012)";

    }
    private void WebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        Console.WriteLine($"Navigated: {e.Url}");
    }
    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new SinginPage());
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new RestPassword());
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//MainPage");
        fg.IsVisible = false;
        return true;
    }


    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // ✅ منع الضغط المزدوج مباشرة
        if (!RegisterButton.IsEnabled)
            return;

        RegisterButton.IsEnabled = false;

        // 🎬 أنيميشن ضغط الزر + إخفاء ناعم
        await Task.WhenAll(
          RegisterButton.RotateTo(10),
          RegisterButton.RotateTo(-10),
          RegisterButton.ScaleTo(0.5, 100, Easing.Linear),
          RegisterButton.ScaleTo(0, 150, Easing.CubicIn));

        // hide button and show loading
        RegisterButton.IsVisible = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        string email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text;

        try
        {
            // ✅ التحقق من الإدخالات بعد إخفاء الزر (حتى المستخدم ما يضغط مرتين)
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                var popup = new DisplayAlretCoustm();
                await this.ShowPopupAsync(popup);
                return;
            }

            if (!IsValidEmail(email))
            {
                var popup = new EroreInputEmaile();
                await this.ShowPopupAsync(popup);
                return;
            }

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                var popup = new NoEnternetConacted();
                await this.ShowPopupAsync(popup);
                return;
            }

            var loginData = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://test.center-yazan.com/api/auth/login", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(result,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!string.IsNullOrEmpty(loginResponse?.Token))
                    await SecureStorage.SetAsync("auth_token", loginResponse.Token);

                if (!string.IsNullOrEmpty(loginResponse?.Refresh_Token))
                    await SecureStorage.SetAsync("refresh_token", loginResponse.Refresh_Token);

                var popup = new CompletedLogin();
                await this.ShowPopupAsync(popup);
                await Shell.Current.GoToAsync("//HomePage");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var popup = new NoEqaulData();
                await this.ShowPopupAsync(popup);
            }
            else
            {
                var popup = new NoServerResponse();
                await this.ShowPopupAsync(popup);
            }
        }
        catch
        {
            var popup = new NoServerResponse();
            await this.ShowPopupAsync(popup);
        }
        finally
        {
            // 🎯 إعادة الحالة الطبيعية دائمًا
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;

            RegisterButton.IsVisible = true;
            RegisterButton.Opacity = 0;
            RegisterButton.Scale = 0.7;
            await RegisterButton.RotateTo(0);

            await Task.WhenAll(
                RegisterButton.FadeTo(1, 200, Easing.CubicOut),

            RegisterButton.ScaleTo(1.1, 200, Easing.SpringOut)
            );
            await RegisterButton.ScaleTo(1.0, 100, Easing.CubicOut);

            RegisterButton.IsEnabled = true; // ✅ تفعيل الزر من جديد
        }
    }



    private string GetArabicFieldName(string fieldName)
    {
        return fieldName.ToLower() switch
        {
            "email" => "البريد الإلكتروني",
            "password" => "كلمة المرور",
            "name" => "الاسم",
            _ => fieldName
        };
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }

    bool isGoogleButtonAnimating = true;
    async void StartGoogleButtonAnimation()
    {
        while (isGoogleButtonAnimating)
        {
            await GoogleButton.RotateTo(2, 350, Easing.SinInOut);
            await GoogleButton.RotateTo(-2, 350, Easing.SinInOut);
            await GoogleButton.RotateTo(0, 300, Easing.SinInOut);
            await Task.Delay(3000);
        }
    }

    // لإيقاف الحركة (عندما تغادر الصفحة أو تنتهي العملية)
    void StopGoogleButtonAnimation()
    {
        isGoogleButtonAnimating = false;
        GoogleButton.RotateTo(0, 300, Easing.CubicOut); // يرجع للوضع الطبيعي
    }

    // مثلاً تبدأ الأنيميشن في OnAppearing()
    protected override void OnAppearing()
    {
        base.OnAppearing();
        isGoogleButtonAnimating = true;
        StartGoogleButtonAnimation();
    }

    // وتوقفها في OnDisappearing()
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopGoogleButtonAnimation();
    }

    private async void GoogleButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            // show spinner, hide button
            GoogleButton.IsVisible = false;
            GoogleLoadingIndicator.IsVisible = true;
            GoogleLoadingIndicator.IsRunning = true;

            // Validate config before calling Firebase
            if (MauiProgram.firebaseconfig == null || MauiProgram.firebaseconfig.Providers == null || MauiProgram.firebaseconfig.Providers.Length == 0)
            {
                Console.WriteLine("Firebase configuration or providers are missing.");
                await DisplayAlert("Configuration error", "Firebase providers not configured", "OK");
                return;
            }

            var providerEntry = MauiProgram.firebaseconfig.Providers[0];
            if (providerEntry == null || providerEntry.ProviderType == null)
            {
                Console.WriteLine("Provider entry or ProviderType is null.");
                await DisplayAlert("Configuration error", "Firebase provider invalid", "OK");
                return;
            }

            var provider = providerEntry.ProviderType;

            // Run sign-in with a timeout to avoid indefinite hanging
            var signInTask = MauiProgram.firebaseclient.SignInWithRedirectAsync(provider, async uri =>
            {
                webView.Source = uri;
                webView.IsVisible = true;
                fg.IsVisible = true;
                webView.Opacity = 1;

                // wait for redirect with timeout
                string finalUrl = await WaitForNavigationToUrlAsync("https://test-23def.web.app/__/auth/handler", TimeSpan.FromSeconds(60));
                fg.IsVisible = false;
                webView.IsVisible = false;
                webView.Source = null;

                return finalUrl;
            });

            var completed = await Task.WhenAny(signInTask, Task.Delay(TimeSpan.FromSeconds(70)));
            if (completed != signInTask)
                throw new TimeoutException("Firebase sign-in timed out.");

            userCredential = await signInTask;

            if (userCredential != null)
            {
                var user = userCredential.User;
                Console.WriteLine($"Logged in: {user.Info.DisplayName} ({user.Info.Email})");
                await DisplayAlert("Sign In", "Welcome: " + user.Info.DisplayName, "OK");
                IsLogged = true;
            }
        }
        catch (FirebaseAuthHttpException fae)
        {
            Console.WriteLine("FirebaseAuthHttpException: " + fae.ToString());
            var msg = !string.IsNullOrEmpty(fae.Message) ? fae.Message : "Firebase HTTP error during authentication.";
            await DisplayAlert("Authentication error", msg, "OK");
        }
        catch (TimeoutException tex)
        {
            Console.WriteLine("Timeout: " + tex.Message);
            await DisplayAlert("Timeout", "Authentication request timed out. Please try again.", "OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception during Google sign-in: " + ex.ToString());
            var inner = ex.InnerException;
            string detail = ex.Message;
            if (inner != null) detail += "\nInner: " + inner.Message + " (" + inner.GetType().FullName + ")";
            await DisplayAlert("Sign-in failed", detail, "OK");
        }
        finally
        {
            // restore button/spinner
            GoogleLoadingIndicator.IsRunning = false;
            GoogleLoadingIndicator.IsVisible = false;
            GoogleButton.IsVisible = true;
        }
    }

    private async Task<string> WaitForNavigationToUrlAsync(string targetUrl, TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<string>();
        EventHandler<WebNavigatedEventArgs>? handler = null;

        handler = (s, e) =>
        {
            try
            {
                if (!string.IsNullOrEmpty(e.Url) && e.Url.StartsWith(targetUrl, StringComparison.OrdinalIgnoreCase))
                {
                    webView.Navigated -= handler;
                    tcs.TrySetResult(e.Url);
                }
            }
            catch (Exception ex)
            {
                webView.Navigated -= handler;
                tcs.TrySetException(ex);
            }
        };

        webView.Navigated += handler;

        var delayTask = Task.Delay(timeout);
        var completed = await Task.WhenAny(tcs.Task, delayTask);

        if (completed == tcs.Task)
            return await tcs.Task;

        // timeout: remove handler to avoid leaks and throw
        webView.Navigated -= handler;
        throw new TimeoutException("Navigation to redirect URL timed out.");
    }

    // ... other methods unchanged ...
}

