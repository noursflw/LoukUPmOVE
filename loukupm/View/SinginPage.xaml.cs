using CommunityToolkit.Maui.Views;
using Firebase.Auth;
using loukupm.Model;
using loukupm.services;
using loukupm.Services;
using loukupm.View.MassgingApp;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static loukupm.Model.Auth;

namespace loukupm.View;

public partial class SinginPage : ContentPage
{
    public static bool IsLogged { get; set; } = false;
    private UserCredential userCredential;
    private string redirectUri;
    public SinginPage()
    {
        InitializeComponent();
        webView.Navigated += WebView_Navigated;
        webView.UserAgent = "Mozilla/5.0 (Linux; Android 8.0; Pixel 2 Build/OPD3.170816.012)";
        UpdateButtonsState();
    }
    private void WebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        Console.WriteLine($"Navigated: {e.Url}");
    }
    private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("LoginPage");
    }

    protected override bool OnBackButtonPressed()
    {
        _ = NavigationService.HandleBackButton("SinginPage");
        return true;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Task.WhenAll(
            RegisterButton.RotateTo(10),
            RegisterButton.RotateTo(-10),
            RegisterButton.ScaleTo(0.5, 100, Easing.Linear),
            RegisterButton.ScaleTo(0, 150, Easing.CubicIn));

        RegisterButton.IsVisible = false;
        RegisterButton.Text = "";
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            string firstName = RegisterNameEntry.Text?.Trim();
            string lastName = RegisterLastNameEntry.Text?.Trim();
            string email = RegisterEmailEntry.Text?.Trim();
            string phone = RegisterPhoneEntry.Text?.Trim();
            string password = RegisterPasswordEntry.Text;
            string passwordConfirmation = RegisterConfirmPasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(passwordConfirmation))
            {
                await this.ShowPopupAsync(new EnterAllFailed());
                return;
            }

            if (!IsValidEmail(email))
            {
                await this.ShowPopupAsync(new EroreInputEmaile());
                return;
            }

            if (password != passwordConfirmation)
            {
                await this.ShowPopupAsync(new Paswordmatch());
                return;
            }

            if (password.Length < 6)
            {
                await this.ShowPopupAsync(new paslen());
                return;
            }

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await this.ShowPopupAsync(new NoEnternetConacted());
                return;
            }

            var registerData = new RegisterRequest
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                Password = password,
                PasswordConfirmation = passwordConfirmation
            };

            var json = JsonSerializer.Serialize(registerData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };

            var response = await client.PostAsync("https://test.center-yazan.com/api/auth/register", content);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(result,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (!string.IsNullOrEmpty(registerResponse?.AccessToken))
                {
                    await SecureStorage.SetAsync("auth_token", registerResponse.AccessToken);
                    await SecureStorage.SetAsync("refresh_token", registerResponse.RefreshToken);
                }

                // ✨ NEW: ربط المستخدم بـ OneSignal
                if (registerResponse?.User != null)
                {
                    OneSignalService.RegisterUser(registerResponse.User.Id.ToString());
                    OneSignalService.AddTag("email", registerResponse.User.Email);
                    OneSignalService.AddTag("signup_date", DateTime.Now.ToString("yyyy-MM-dd"));
                }

                await this.ShowPopupAsync(new CompletedLogin());
                await Shell.Current.GoToAsync("//HomePage");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                await this.ShowPopupAsync(new EmaileUsed());
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                await this.ShowPopupAsync(new NoServerResponse()); // أو يمكن Popup مخصص "بيانات غير صالحة"
            }
            else
            {
                await this.ShowPopupAsync(new NoServerResponse());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Register Error: {ex.Message}");
            await this.ShowPopupAsync(new NoServerResponse());
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            RegisterButton.Text = "إنشاء حساب";
            RegisterButton.IsVisible = true;
            RegisterButton.Scale = 0;
            await RegisterButton.RotateTo(0);
            await RegisterButton.ScaleTo(1.1, 150, Easing.CubicOut);
            await RegisterButton.ScaleTo(1.0, 100, Easing.Linear);
        }
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
            await GoogleButton.RotateTo(2, 250, Easing.SinInOut);
            await GoogleButton.RotateTo(-2, 250, Easing.SinInOut);
            await GoogleButton.RotateTo(0, 300, Easing.SinInOut);
            await Task.Delay(600);
        }
    }

    void StopGoogleButtonAnimation()
    {
        isGoogleButtonAnimating = false;
        GoogleButton.RotateTo(0, 300, Easing.CubicOut);
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
        try
        {
          
            fglogin.IsVisible = true;
            fglogin.IsVisible = false;
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
                webView.Source = uri;
                webView.IsVisible = true;
                fg.IsVisible = true;
                fglogin.IsVisible = false;
                webView.Opacity = 1;

                
                string finalUrl = await WaitForNavigationToUrlAsync("https://test-23def.web.app/__/auth/handler", TimeSpan.FromSeconds(80));
                fg.IsVisible = false;
                fglogin.IsVisible = true;
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

            if (userCredential?.User != null)
            {
                OneSignalService.RegisterUser(userCredential.User.Uid);
                OneSignalService.AddTag("email", userCredential.User.Info.Email);
                OneSignalService.AddTag("signup_type", "google");
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
            
            GoogleLoadingIndicator.IsRunning = false;
            GoogleLoadingIndicator.IsVisible = false;
            GoogleButton.IsVisible = true;
            fglogin.IsVisible = true;
        }
    }
    private async void TapGestureRecognizer_Tapped_2(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new PolicyandPrivacyPage());
    }
    private void CheckBox_CheckChanged(object sender, EventArgs e)
    {
        UpdateButtonsState();
    }
    private void UpdateButtonsState()
    {
        if (DD.IsChecked)
        {
            GoogleButton.IsEnabled = true;
            RegisterButton.IsEnabled = true;
            GoogleButton.Opacity = 1;
            RegisterButton.Opacity = 1;
        }
        else
        {
            GoogleButton.IsEnabled = false;
            RegisterButton.IsEnabled = false;
            GoogleButton.Opacity = 0.5;
            RegisterButton.Opacity = 0.5;
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
    // End Section Google Sign-In With FireBase aND SingUp API Authentication 
}
