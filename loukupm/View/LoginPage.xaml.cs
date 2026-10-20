
using CommunityToolkit.Maui.Views;
using loukupm.Langue;
using loukupm.Model;
using loukupm.services;
using loukupm.Services;
using loukupm.View.MassgingApp;
using loukupm.ViewModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using OneSignalSDK.DotNet;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static loukupm.Model.Auth;

namespace loukupm.View;

public partial class LoginPage : ContentPage
{
    public static bool IsLogged { get; set; } = false;

    private bool _googleInProgress;

    public LoginPage()
    {
        InitializeComponent();
        this.InitializeLanguageTracking();
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(
            NavigationService.ROUTE_TermsAndConditions_Athun);
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(
            NavigationService.ROUTE_REST_PASSWORD);
    }

    private async Task<object?> SafeShowPopupAsync(Popup popup)
    {
        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            if (this.Handler?.MauiContext != null)
            {
                return await this.ShowPopupAsync(popup);
            }

            return null;
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
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
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

            // 🔐 Use centralized AuthenticationService instead of direct HttpClient
            var authService = App.Current?.Handler?.MauiContext?.Services.GetService<AuthenticationService>();

            if (authService == null)
            {
                Console.WriteLine("❌ AuthenticationService not available");
                var popup = new NoServerResponse();
                await SafeShowPopupAsync(popup);
                return;
            }

            var (success, accessToken, refreshToken, user, statusCode, errorMessage) = 
                await authService.LoginAsync(email, password);

            if (success)
            {
                Console.WriteLine("✅ Login successful via AuthenticationService");

                await AppViewModel.Instance.LoadUserDataAsync();

                if (user != null && user.Id > 0)
                    OneSignalService.LinkUser(user.Id.ToString());

                IsLogged = true;
                isNavigationSuccessful = true;

                NavigationService.ResetLogoutFlag();
                NavigationService.SetAppContext();

                CancelLoginButtonAnimations();
                await ShellNavigationManager.NavigateToHomeAndClear();
                QueueResetLoginStateAfterLeave();
                return;
            }

            if (statusCode == 401)
            {
                var popup = new NoEqaulData();
                await SafeShowPopupAsync(popup);
            }
            else if (statusCode == 403)
            {
                var goToVerify = await SafeShowPopupAsync(new EmailNotVerified());
                if (goToVerify is true)
                    await OpenEmailVerificationAsync(email);
            }
            else if (statusCode == 0)
            {
                // Network error
                var popup = new NoEnternetConacted();
                await SafeShowPopupAsync(popup);
            }
            else
            {
                var popup = new NoServerResponse();
                await SafeShowPopupAsync(popup);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Login failed: {ex}");

            var popup = new NoServerResponse();
            await SafeShowPopupAsync(popup);
        }
        finally
        {
            if (!isNavigationSuccessful &&
                this.Handler?.MauiContext != null)
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;

                RegisterButton.IsVisible = true;
                RegisterButton.Opacity = 0;
                RegisterButton.Scale = 0.7;

                await RegisterButton.RotateTo(0);

                await Task.WhenAll(
                    RegisterButton.FadeTo(
                        1,
                        200,
                        Easing.CubicOut),

                    RegisterButton.ScaleTo(
                        1.1,
                        200,
                        Easing.SpringOut)
                );

                await RegisterButton.ScaleTo(
                    1.0,
                    100,
                    Easing.CubicOut);

                RegisterButton.IsEnabled = true;
            }
        }
    }

    private async Task OpenEmailVerificationAsync(string email)
    {
        try
        {
            var api = new ApiServices();
            await api.ResendOtpAsync(
                email: email,
                registrationMethod: "email");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ [LoginPage] Resend OTP before verification failed: {ex.Message}");
        }

        var otpContext = new Auth.OtpContext
        {
            RegistrationMethod = "email",
            Email = email,
            MaskedDestination = MaskEmail(email)
        };

        await NavigationService.NavigateToPage(
            NavigationService.ROUTE_OTP,
            otpContext);
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "your email";

        int atIndex = email.IndexOf('@');
        if (atIndex <= 0)
            return email;

        string username = email.Substring(0, atIndex);
        string domain = email.Substring(atIndex);

        if (username.Length <= 2)
            return username[0] + "***" + domain;

        return username.Substring(0, 2) +
               new string('*', Math.Max(2, username.Length - 2)) +
               domain;
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

        string pattern =
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        return Regex.IsMatch(email, pattern);
    }

    bool isGoogleButtonAnimating = true;

    async void StartGoogleButtonAnimation()
    {
        while (isGoogleButtonAnimating)
        {
            if (this.Handler?.MauiContext == null)
                break;

            await GoogleButton.RotateTo(
                2,
                350,
                Easing.SinInOut);

            await GoogleButton.RotateTo(
                -2,
                350,
                Easing.SinInOut);

            await GoogleButton.RotateTo(
                0,
                300,
                Easing.SinInOut);

            await Task.Delay(3000);
        }
    }

    void StopGoogleButtonAnimation()
    {
        isGoogleButtonAnimating = false;

        if (this.Handler?.MauiContext != null)
        {
            GoogleButton.RotateTo(
                0,
                300,
                Easing.CubicOut);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_googleInProgress)
            return;

        if (IsLogged)
            ResetLoginState();

        IsLogged = false;
        NavigationService.ResetLogoutFlag();

        isGoogleButtonAnimating = true;
        StartGoogleButtonAnimation();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopGoogleButtonAnimation();

        // Successful login is cleared after the next page is shown,
        // so the user never sees the empty form.
        if (!IsLogged && !_googleInProgress)
            ResetLoginState();
    }

    private void QueueResetLoginStateAfterLeave()
    {
        var attempts = 0;

        void TryReset()
        {
            try
            {
                attempts++;

                if (IsStillShowingLoginPage() && attempts < 20)
                {
                    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(50), TryReset);
                    return;
                }

                ResetLoginState();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Login reset after leave skipped: {ex.Message}");
            }
        }

        try
        {
            Dispatcher.Dispatch(TryReset);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Login reset dispatch skipped: {ex.Message}");
        }
    }

    private bool IsStillShowingLoginPage()
    {
        var root = Application.Current?.Windows.FirstOrDefault()?.Page
                   ?? Application.Current?.MainPage;

        if (root == this)
            return true;

        if (root is NavigationPage navigationPage && navigationPage.CurrentPage == this)
            return true;

        if (Shell.Current?.CurrentPage == this)
            return true;

        return false;
    }

    private static bool HasNativeView(VisualElement? view)
    {
        return view?.Handler?.MauiContext != null;
    }

    private void CancelLoginButtonAnimations()
    {
        if (!HasNativeView(RegisterButton))
            return;

        RegisterButton.CancelAnimations();
    }

    private void ResetLoginState()
    {
        // After a successful login the native Android views are already
        // disconnected. Touching buttons then crashes in MapBackground.
        if (Handler?.MauiContext == null)
            return;

        try
        {
            if (HasNativeView(EmailEntry))
                EmailEntry.Text = string.Empty;
            if (HasNativeView(PasswordEntry))
                PasswordEntry.Text = string.Empty;

            HideLiveError();
            StopGoogleButtonAnimation();

            if (HasNativeView(RegisterButton))
            {
                RegisterButton.CancelAnimations();
                RegisterButton.Rotation = 0;
                RegisterButton.Scale = 1;
                RegisterButton.Opacity = 1;
                RegisterButton.IsVisible = true;
                RegisterButton.IsEnabled = true;
            }

            if (HasNativeView(LoadingIndicator))
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }

            if (HasNativeView(GoogleButton))
                GoogleButton.IsVisible = true;

            if (HasNativeView(GoogleLoadingIndicator))
            {
                GoogleLoadingIndicator.IsRunning = false;
                GoogleLoadingIndicator.IsVisible = false;
            }

            if (HasNativeView(fglogin))
                fglogin.IsVisible = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ ResetLoginState skipped: {ex.Message}");
        }
    }

    private async void GoogleButton_Clicked(
        object sender,
        EventArgs e)
    {
        if (_googleInProgress)
            return;

        _googleInProgress = true;
        StopGoogleButtonAnimation();

        var signedIn = false;

        try
        {
            GoogleButton.IsVisible = false;
            GoogleLoadingIndicator.IsVisible = true;
            GoogleLoadingIndicator.IsRunning = true;

            var result = await GoogleAuthSession.SignInAndConfirmAsync();

            if (result.Cancelled)
                return;

            if (!result.Success)
            {
                await DisplayAlert(
                    "Sign-in failed",
                    string.IsNullOrWhiteSpace(result.Error)
                        ? "Google sign-in was not confirmed."
                        : result.Error,
                    "OK");
                return;
            }

            try
            {
                await AppViewModel.Instance.LoadUserDataAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Profile load after Google login failed: {ex.Message}");
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(result.Email))
                    OneSignalService.AddTag("email", result.Email);

                OneSignalService.AddTag("login_type", "google");

                if (!string.IsNullOrWhiteSpace(result.DisplayName))
                    OneSignalService.AddTag("display_name", result.DisplayName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OneSignal Google login tag failed: {ex.Message}");
            }

            signedIn = true;
            IsLogged = true;

            NavigationService.ResetLogoutFlag();
            NavigationService.SetAppContext();

            CancelLoginButtonAnimations();
            await ShellNavigationManager.NavigateToHomeAndClear();
            QueueResetLoginStateAfterLeave();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception during Google sign-in: " + ex);
            IsLogged = false;
            signedIn = false;

            await DisplayAlert(
                "Sign-in failed",
                string.IsNullOrWhiteSpace(ex.Message)
                    ? "Google sign-in failed."
                    : ex.Message,
                "OK");
        }
        finally
        {
            _googleInProgress = false;

            if (!signedIn && this.Handler?.MauiContext != null)
            {
                GoogleLoadingIndicator.IsRunning = false;
                GoogleLoadingIndicator.IsVisible = false;
                GoogleButton.IsVisible = true;
                fglogin.IsVisible = true;

                isGoogleButtonAnimating = true;
                StartGoogleButtonAnimation();
            }
        }
    }


    private async void TapGestureRecognizer_Tapped_2(
        object sender,
        TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(
            NavigationService.ROUTE_POLICY_PRIVACY_AUTH);
    }

    private async void TapGestureRecognizer_Tapped_3(
        object sender,
        TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(
            NavigationService.ROUTE_SIGNIN);
    }

    private void OnEmailTextChanged(
        object sender,
        TextChangedEventArgs e)
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
        if (this.Handler?.MauiContext == null)
            return;

        LiveErrorLabel.Text = message;
        LiveErrorLabel.TextColor = Colors.Red;
        LiveErrorLabel.IsVisible = true;
        LiveErrorLabel.Opacity = 0;

        await LiveErrorLabel.FadeTo(
            1,
            150);
    }

    private void HideLiveError()
    {
        if (!HasNativeView(LiveErrorLabel))
            return;

        LiveErrorLabel.IsVisible = false;
    }
}

