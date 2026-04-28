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
        webView.Navigated += WebView_Navigated;
        webView.UserAgent = "Mozilla/5.0 (Linux; Android 8.0; Pixel 2 Build/OPD3.170816.012)";
      

    }
    private void WebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        Console.WriteLine($"Navigated: {e.Url}");
    }
    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_TERMS_CONDITIONS);
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_REST_PASSWORD);
    }

    protected override bool OnBackButtonPressed()
    {
        // على صفحة تسجيل الدخول، لا تسمح بالخروج من التطبيق
        return true; // منع الضغط على الزر الخلفي
    }


    private async void OnLoginClicked(object sender, EventArgs e)
    {
       

        RegisterButton.IsEnabled = false;

       
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

                    // حفظ التوكن
                    if (!string.IsNullOrEmpty(loginResponse?.Token))
                        await SecureStorage.SetAsync("auth_token", loginResponse.Token);

                    if (!string.IsNullOrEmpty(loginResponse?.Refresh_Token))
                        await SecureStorage.SetAsync("refresh_token", loginResponse.Refresh_Token);
                    var popup = new CompletedLogin();
                    await this.ShowPopupAsync(popup);

                    // ⭐ تحميل بيانات المستخدم قبل التنقل
                    await AppViewModel.Instance.LoadUserDataAsync();

                    // Clear the navigation stack and navigate to HomePage using the navigation service
                    await ShellNavigationManager.NavigateToHomeAndClear();

                    // ⭐ هنا تعريف اليوزر ل OneSignal
                    if (loginResponse?.User != null)
                    {
                        var userId = loginResponse.User.Id.ToString();

                        // ربط الجهاز بالمستخدم
                        OneSignal.Login(userId);

                        // إضافة Tag (حتى تقدر تعمل Filter من Dashboard)
                        OneSignal.User.AddTag("user_no", userId);
                    }

                  
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

  
    void StopGoogleButtonAnimation()
    {
        isGoogleButtonAnimating = false;
        GoogleButton.RotateTo(0, 300, Easing.CubicOut); // يرجع للوضع الطبيعي
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
            // show spinner, hide button
            fglogin.IsVisible = true;
            fglogin.IsVisible = false;
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
                fglogin.IsVisible = false;
                webView.Opacity = 1;

                // wait for redirect with timeout
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

            if (userCredential?.User != null)
            {
                var userId = userCredential.User.Uid;

                // احصل على ID Token من Firebase
                var idToken = await userCredential.User.GetIdTokenAsync();

                // أرسل Token إلى Backend API
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

                        // حفظ التوكن إذا حصلت عليه من Backend
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

                // OneSignal Setup
                OneSignal.Login(userId);
                OneSignal.User.AddTag("email", userCredential.User.Info.Email);
                OneSignal.User.AddTag("login_type", "google");
                OneSignal.User.AddTag("display_name", userCredential.User.Info.DisplayName);

                // تحميل بيانات المستخدم وتنقل إلى الصفحة الرئيسية
                await AppViewModel.Instance.LoadUserDataAsync();

                await ShellNavigationManager.NavigateToHomeAndClear();
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
           fglogin.IsVisible = true;
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

    private async void TapGestureRecognizer_Tapped_2(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_POLICY_PRIVACY);
    }

    //private  void CheckBox_CheckChanged(object sender, EventArgs e)
    //{
    //    UpdateButtonsState();
    //}
    //private void UpdateButtonsState()
    //{
    //    if (DD.IsChecked)
    //    {
    //        GoogleButton.IsEnabled = true;
    //        RegisterButton.IsEnabled = true;
    //        GoogleButton.Opacity = 1;
    //        RegisterButton.Opacity = 1;
    //    }
    //    else
    //    {
    //        GoogleButton.IsEnabled = false;
    //        RegisterButton.IsEnabled = false;
    //        GoogleButton.Opacity = 0.5;
    //        RegisterButton.Opacity = 0.5;
    //    }
    //}

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

        if (!IsValidEmaile(email))
        {
            ShowLiveError(AppResource.ErorEmailInput);
            return;
        }

        HideLiveError();
    }
    private bool IsValidEmaile(string email)
    {
        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }
    private async void ShowLiveError(string message)
    {
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

