using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Firebase.Auth;
using loukupm.Langue;
using loukupm.Model;
using loukupm.services;
using loukupm.Services;
using loukupm.View.MassgingApp;
using loukupm.ViewModel;
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
        await NavigationService.NavigateToPage(NavigationService.ROUTE_LOGIN);
    }

    private DateTime _lastBackPressed = DateTime.MinValue;
    protected override bool OnBackButtonPressed()
    {
        var currentTime = DateTime.Now;

        if ((currentTime - _lastBackPressed).TotalSeconds <= 2)
        {
#if ANDROID
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#endif
            return true;
        }

        _lastBackPressed = currentTime;
        ShowToast("ÇÖÛØ ãÑÉ ÃÎÑì ááÎÑæÌ");
        return true;
    }

    private async void ShowToast(string message)
    {
        var toast = Toast.Make(message, ToastDuration.Short);
        await toast.Show();
    }



    //private async void OnRegisterClicked(object sender, EventArgs e)
    //{
    //    await Task.WhenAll(
    //        RegisterButton.RotateTo(10),
    //        RegisterButton.RotateTo(-10),
    //        RegisterButton.ScaleTo(0.5, 100, Easing.Linear),
    //        RegisterButton.ScaleTo(0, 150, Easing.CubicIn));

    //    RegisterButton.IsVisible = false;
    //    RegisterButton.Text = "";
    //    LoadingIndicator.IsVisible = true;
    //    LoadingIndicator.IsRunning = true;

    //    try
    //    {
    //        string firstName = RegisterNameEntry.Text?.Trim();
    //        string lastName = RegisterLastNameEntry.Text?.Trim();
    //        string email = RegisterEmailEntry.Text?.Trim();
    //        string phone = RegisterPhoneEntry.Text?.Trim();
    //        string password = RegisterPasswordEntry.Text;
    //        string passwordConfirmation = RegisterConfirmPasswordEntry.Text;

    //        if (string.IsNullOrWhiteSpace(firstName) ||
    //            string.IsNullOrWhiteSpace(lastName) ||
    //            string.IsNullOrWhiteSpace(email) ||
    //            string.IsNullOrWhiteSpace(password) ||
    //            string.IsNullOrWhiteSpace(phone) ||
    //            string.IsNullOrWhiteSpace(passwordConfirmation))
    //        {
    //            await this.ShowPopupAsync(new EnterAllFailed());
    //            return;
    //        }

    //        if (!IsValidEmail(email))
    //        {
    //            await this.ShowPopupAsync(new EroreInputEmaile());
    //            return;
    //        }

    //        if (password != passwordConfirmation)
    //        {
    //            await this.ShowPopupAsync(new Paswordmatch());
    //            return;
    //        }

    //        if (password.Length < 6)
    //        {
    //            await this.ShowPopupAsync(new paslen());
    //            return;
    //        }

    //        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
    //        {
    //            await this.ShowPopupAsync(new NoEnternetConacted());
    //            return;
    //        }

    //        var registerData = new RegisterRequest
    //        {
    //            FirstName = firstName,
    //            LastName = lastName,
    //            Email = email,
    //            Phone = phone,
    //            Password = password,
    //            PasswordConfirmation = passwordConfirmation
    //        };

    //        var json = JsonSerializer.Serialize(registerData);
    //        var content = new StringContent(json, Encoding.UTF8, "application/json");

    //        var handler = new HttpClientHandler
    //        {
    //            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    //        };
    //        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };

    //        var response = await client.PostAsync("https://test.center-yazan.com/api/auth/register", content);
    //        var result = await response.Content.ReadAsStringAsync();

    //        if (response.IsSuccessStatusCode)
    //        {
    //            var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(result,
    //                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    //            if (!string.IsNullOrEmpty(registerResponse?.AccessToken))
    //            {
    //                await SecureStorage.SetAsync("auth_token", registerResponse.AccessToken);
    //                await SecureStorage.SetAsync("refresh_token", registerResponse.RefreshToken);
    //            }


    //            if (registerResponse?.User != null)
    //            {
    //                OneSignalService.RegisterUser(registerResponse.User.Id.ToString());
    //                OneSignalService.AddTag("email", registerResponse.User.Email);
    //                OneSignalService.AddTag("signup_date", DateTime.Now.ToString("yyyy-MM-dd"));
    //            }

    //            await this.ShowPopupAsync(new CompletedLogin());


    //            await AppViewModel.Instance.LoadUserDataAsync();

    //            await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_HOME);
    //        }
    //        else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
    //        {
    //            await this.ShowPopupAsync(new EmaileUsed());
    //        }
    //        else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
    //        {
    //            await this.ShowPopupAsync(new NoServerResponse());
    //        }
    //        else
    //        {
    //            await this.ShowPopupAsync(new NoServerResponse());
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Register Error: {ex.Message}");
    //        await this.ShowPopupAsync(new NoServerResponse());
    //    }
    //    finally
    //    {
    //        LoadingIndicator.IsRunning = false;
    //        LoadingIndicator.IsVisible = false;
    //        RegisterButton.Text = AppResource.CreatAcount2;
    //        RegisterButton.IsVisible = true;
    //        RegisterButton.Scale = 0;
    //        await RegisterButton.RotateTo(0);
    //        await RegisterButton.ScaleTo(1.1, 150, Easing.CubicOut);
    //        await RegisterButton.ScaleTo(1.0, 100, Easing.Linear);
    //    }
    //}
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Task.WhenAll(
            RegisterButton.RotateTo(10),
            RegisterButton.RotateTo(-10),
            RegisterButton.ScaleTo(0.5, 100, Easing.Linear),
            RegisterButton.ScaleTo(0, 150, Easing.CubicIn));

        RegisterButton.IsVisible = false;
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
                PasswordConfirmation = passwordConfirmation,
                RegistrationMethod = string.IsNullOrWhiteSpace(phone) ? "email" : "phone"
            };

            var json = JsonSerializer.Serialize(registerData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
           
            var response = await client.PostAsync(
                "https://test.center-yazan.com/api/auth/register",
                content);
            Console.WriteLine($"Register API response: {response}");
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(
                    result,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // ❌ REMOVE: NO TOKENS HERE ANYMORE

                var otpContext = new OtpContext
                {
                    Email = email,
                    Phone = phone,
                    RegistrationMethod = string.IsNullOrWhiteSpace(phone) ? "email" : "phone",
                    MaskedDestination = registerResponse?.MaskedDestination
                };

                await NavigationService.NavigateToPage(
                    NavigationService.ROUTE_OTP,
                    otpContext);
                RegisterButton.IsVisible = true;

                await this.ShowPopupAsync(new CompletedLogin());
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                await this.ShowPopupAsync(new EmaileUsed());
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                await this.ShowPopupAsync(new NoServerResponse());
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
            RegisterButton.IsVisible = true;
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
                        var googleAuthResponse = JsonSerializer.Deserialize<RegisterResponse>(result,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        // حفظ التوكن إذا حصلت عليه من Backend
                        if (!string.IsNullOrEmpty(googleAuthResponse?.AccessToken))
                            await SecureStorage.SetAsync("auth_token", googleAuthResponse.AccessToken);

                        if (!string.IsNullOrEmpty(googleAuthResponse?.RefreshToken))
                            await SecureStorage.SetAsync("refresh_token", googleAuthResponse.RefreshToken);
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


                OneSignalService.RegisterUser(userId);
                OneSignalService.AddTag("email", userCredential.User.Info.Email);
                OneSignalService.AddTag("signup_type", "google");


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

            GoogleLoadingIndicator.IsRunning = false;
            GoogleLoadingIndicator.IsVisible = false;
            GoogleButton.IsVisible = true;
            fglogin.IsVisible = true;
        }
    }
    private async void TapGestureRecognizer_Tapped_2(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_POLICY_PRIVACY);
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

    private void OnConfirmPasswordTextChanged(object sender, TextChangedEventArgs e)
    {
        ValidatePasswords();
    }
    private void ValidatePasswords()
    {
        var password = RegisterPasswordEntry.Text;
        var confirm = RegisterConfirmPasswordEntry.Text;


        if (string.IsNullOrWhiteSpace(confirm))
        {
            PasswordMatchLabel.Text = AppResource.Thisfieldisrequired;
            PasswordMatchLabel.TextColor = Colors.Orange;
            PasswordMatchLabel.IsVisible = true;
            return;
        }


        if (password != confirm)
        {
            PasswordMatchLabel.Text = AppResource.Passwordnotmatch;
            PasswordMatchLabel.TextColor = Colors.Red;
            PasswordMatchLabel.IsVisible = true;
            return;
        }

        PasswordMatchLabel.Text = AppResource.Passwordsmatch;
        PasswordMatchLabel.TextColor = Colors.Green;
        PasswordMatchLabel.IsVisible = true;
    }

    private void RegisterNameEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        var name = e.NewTextValue?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        { 
            ShowNameError(AppResource.Thisfieldisrequired);
        }
        else
        {
            HideNameError();
        }
    }

    private void HideNameError()
    {
        ErorNameInput.IsVisible = false;
    }

    private void ShowNameError(string thisfieldisrequired)
    {
        ErorNameInput.Text = thisfieldisrequired;
        ErorNameInput.TextColor = Colors.Red;
        ErorNameInput.IsVisible = true;
    }

    private void RegisterLastNameEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        var lastName = e.NewTextValue?.Trim(); 
        if (string.IsNullOrWhiteSpace(lastName))
        {
            ShowLastNameError(AppResource.Thisfieldisrequired);
        } 
        else
        {
            HideLastNameError();
        }
    }

    private void HideLastNameError()
    {
        ErorLastNameInput.IsVisible = false;
    }

    private void ShowLastNameError(string thisfieldisrequired)
    {
        ErorLastNameInput.Text = thisfieldisrequired;
        ErorLastNameInput.TextColor = Colors.Red;
        ErorLastNameInput.IsVisible = true;
    }
}
