using CommunityToolkit.Maui.Views;
using Firebase.Auth;
using Firebase.Auth.Providers;
using loukupm.Langue;
using loukupm.Model;
using loukupm.services;
using loukupm.Services;
using loukupm.View.MassgingApp;
using loukupm.ViewModel;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.PlatformConfiguration;
using OneSignalSDK.DotNet;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static loukupm.Model.Auth;

namespace loukupm.View;

public partial class LoginPage : ContentPage
{
    public static bool IsLogged { get; set; } = false;
    private UserCredential userCredential;
    private string redirectUri;

    public LoginPage()
    {
        InitializeComponent();
        this.InitializeLanguageTracking();
        webView.Navigated += WebView_Navigated;
        webView.UserAgent = "Mozilla/5.0 (Linux; Android 8.0; Pixel 2 Build/OPD3.170816.012)";
    }

    private void WebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        Console.WriteLine($"Navigated: {e.Url}");
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_TermsAndConditions_Athun);
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_REST_PASSWORD);
    }

    private async Task SafeShowPopupAsync(Popup popup)
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (this.Handler?.MauiContext != null)
            {
                await this.ShowPopupAsync(popup);
            }
        });
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        RegisterButton.IsEnabled = false;

        await Task.WhenAll(
          RegisterButton.RotateTo(10),
          RegisterButton.RotateTo(-10),
          RegisterButton.ScaleTo(0.5, 100, Easing.Linear),
          RegisterButton.ScaleTo(0, 150, Easing.CubicIn));

        RegisterButton.IsVisible = false;
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        string email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text;

        bool isNavigationSuccessful = false;

        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                var popup = new DisplayAlretCoustm();
                await SafeShowPopupAsync(popup);
                return;
            }

            if (!IsValidEmail(email))
            {
                var popup = new EroreInputEmaile();
                await SafeShowPopupAsync(popup);
                return;
            }

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                var popup = new NoEnternetConacted();
                await SafeShowPopupAsync(popup);
                return;
            }

            var loginData = new LoginRequest
            {
                Email = email,
                Password = password,
                RegistrationMethod = "email"
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
                await SafeShowPopupAsync(popup);

                await AppViewModel.Instance.LoadUserDataAsync();

                if (loginResponse?.User != null)
                {
                    var userId = loginResponse.User.Id.ToString();
                    OneSignal.Login(userId);
                    OneSignal.User.AddTag("user_no", userId);
                }

                isNavigationSuccessful = true;
                // Clear any previous logout-in-progress state before navigating to home
                NavigationService.ResetLogoutFlag();
                await ShellNavigationManager.NavigateToHomeAndClear();
                return;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var popup = new NoEqaulData();
                await SafeShowPopupAsync(popup);
            }
            else
            {
                var popup = new NoServerResponse();
                await SafeShowPopupAsync(popup);
            }
        }
        catch
        {
            var popup = new NoServerResponse();
            await SafeShowPopupAsync(popup);
        }
        finally
        {
            if (!isNavigationSuccessful && this.Handler?.MauiContext != null)
            {
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

                RegisterButton.IsEnabled = true;
            }
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
            if (this.Handler?.MauiContext == null) break;

            await GoogleButton.RotateTo(2, 350, Easing.SinInOut);
            await GoogleButton.RotateTo(-2, 350, Easing.SinInOut);
            await GoogleButton.RotateTo(0, 300, Easing.SinInOut);
            await Task.Delay(3000);
        }
    }

    void StopGoogleButtonAnimation()
    {
        isGoogleButtonAnimating = false;
        if (this.Handler?.MauiContext != null)
        {
            GoogleButton.RotateTo(0, 300, Easing.CubicOut);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        isGoogleButtonAnimating = true;
        StartGoogleButtonAnimation();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopGoogleButtonAnimation();
    }

    private async void GoogleButton_Clicked(object sender, EventArgs e)
    {
        bool isGoogleNavSuccessful = false;
        StopGoogleButtonAnimation(); // إيقاف الأنيميشن فوراً عند النقر لتوفير موارد المعالج

        try
        {
            GoogleButton.IsVisible = false;
            GoogleLoadingIndicator.IsVisible = true;
            GoogleLoadingIndicator.IsRunning = true;

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

            var signInTask = MauiProgram.firebaseclient.SignInWithRedirectAsync(provider, async uri =>
            {
                fglogin.IsVisible = false;
                webView.Source = uri;
                webView.IsVisible = true;
                fg.IsVisible = true;
                webView.Opacity = 1;

                try
                {
                    string finalUrl = await WaitForNavigationToUrlAsync("https://test-23def.web.app/__/auth/handler", TimeSpan.FromSeconds(80));
                    return finalUrl;
                }
                finally
                {
                    // الحماية هنا: نضمن إغلاق الـ WebView وإعادة الحاوية الأصلية حتى لو حدث Timeout أو إلغاء
                    fg.IsVisible = false;
                    webView.IsVisible = false;
                    webView.Source = null;
                    fglogin.IsVisible = true;
                }
            });

            var completed = await Task.WhenAny(signInTask, Task.Delay(TimeSpan.FromSeconds(70)));
            if (completed != signInTask)
                throw new TimeoutException("Firebase sign-in timed out.");

            userCredential = await signInTask;

            if (userCredential?.User != null)
            {
                var userId = userCredential.User.Uid;
                var idToken = await userCredential.User.GetIdTokenAsync();

                var googleAuthData = new
                {
                    Token = idToken,
                    UserId = userId,
                    Email = userCredential.User.Info.Email,
                    DisplayName = userCredential.User.Info.DisplayName
                };

                try
                {
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                    };

                    using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
                    var json = JsonSerializer.Serialize(googleAuthData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("https://test.center-yazan.com/api/auth/google", content);
                    var result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var googleAuthResponse = JsonSerializer.Deserialize<LoginResponse>(result,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (!string.IsNullOrEmpty(googleAuthResponse?.Token))
                            await SecureStorage.SetAsync("auth_token", googleAuthResponse.Token);

                        if (!string.IsNullOrEmpty(googleAuthResponse?.Refresh_Token))
                            await SecureStorage.SetAsync("refresh_token", googleAuthResponse.Refresh_Token);
                    }
                    else
                    {
                        Console.WriteLine($"Backend API error: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending token to backend: {ex.Message}");
                }

                OneSignal.Login(userId);
                OneSignal.User.AddTag("email", userCredential.User.Info.Email);
                OneSignal.User.AddTag("login_type", "google");
                OneSignal.User.AddTag("display_name", userCredential.User.Info.DisplayName);

                await AppViewModel.Instance.LoadUserDataAsync();

                isGoogleNavSuccessful = true;
                NavigationService.ResetLogoutFlag();
                await ShellNavigationManager.NavigateToHomeAndClear();
                return;
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
            if (!isGoogleNavSuccessful && this.Handler?.MauiContext != null)
            {
                GoogleLoadingIndicator.IsRunning = false;
                GoogleLoadingIndicator.IsVisible = false;
                GoogleButton.IsVisible = true;
                fglogin.IsVisible = true;
                fg.IsVisible = false;
                webView.IsVisible = false;

                // إعادة تشغيل أنيميشن الزر لأن عملية الدخول فشلت وبقينا في نفس الصفحة
                isGoogleButtonAnimating = true;
                StartGoogleButtonAnimation();
            }
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

        webView.Navigated -= handler;
        throw new TimeoutException("Navigation to redirect URL timed out.");
    }

    private async void TapGestureRecognizer_Tapped_2(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_POLICY_PRIVACY_AUTH);
    }

    private async void TapGestureRecognizer_Tapped_3(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_SIGNIN);
    }

    private void OnEmailTextChanged(object sender, TextChangedEventArgs e)
    {
        var email = e.NewTextValue?.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            ShowLiveError(AppResource.Emailisrequired);
            return;
        }

        if (!IsValidEmail(email))
        {
            ShowLiveError(AppResource.ErorEmailInput);
            return;
        }

        HideLiveError();
    }

    private async void ShowLiveError(string message)
    {
        if (this.Handler?.MauiContext == null) return;

        LiveErrorLabel.Text = message;
        LiveErrorLabel.TextColor = Colors.Red;
        LiveErrorLabel.IsVisible = true;

        LiveErrorLabel.Opacity = 0;
        await LiveErrorLabel.FadeTo(1, 150);
    }

    private void HideLiveError()
    {
        LiveErrorLabel.IsVisible = false;
    }
}