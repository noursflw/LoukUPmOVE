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
using OneSignalSDK.DotNet;
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

        this.InitializeLanguageTracking();

        webView.Navigated += WebView_Navigated;

        webView.UserAgent =
            "Mozilla/5.0 (Linux; Android 8.0; Pixel 2 Build/OPD3.170816.012)";

        UpdateButtonsState();
    }

    private void WebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        Console.WriteLine($"Navigated: {e.Url}");
    }

    private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        await NavigationService.NavigateToPage(
            NavigationService.ROUTE_LOGIN);
    }

    private int _loginNavigationStarted;
    private bool _leavingForLogin;

    private async Task GoToLoginFromRegisterAsync()
    {
        if (Interlocked.Exchange(ref _loginNavigationStarted, 1) == 1)
            return;

        _leavingForLogin = true;

        try
        {
            // Let the Android release dialog finish closing before changing the stack.
            await Task.Delay(250);

            if (Navigation?.NavigationStack is { Count: > 1 })
            {
                await Navigation.PopToRootAsync(false);
                return;
            }

            var root = Application.Current?.Windows.FirstOrDefault()?.Page
                       ?? Application.Current?.MainPage;

            if (root is NavigationPage navPage &&
                navPage.Navigation.NavigationStack.Count > 1)
            {
                await navPage.PopToRootAsync(false);
                return;
            }

            if (Shell.Current is loukupm.AppShell appShell)
                await appShell.GoToLoginAsync();

            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync(
                    $"//{NavigationService.ROUTE_LOGIN}",
                    animate: false);
                return;
            }

            await NavigationService.NavigateToPage(
                NavigationService.ROUTE_LOGIN);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[SinginPage] GoToLogin failed: {ex.Message}");

            Interlocked.Exchange(ref _loginNavigationStarted, 0);

            try
            {
                await NavigationService.NavigateToPage(
                    NavigationService.ROUTE_LOGIN);
            }
            catch (Exception fallbackEx)
            {
                _leavingForLogin = false;
                Console.WriteLine(
                    $"[SinginPage] Login fallback failed: {fallbackEx.Message}");
            }
        }
    }

    private DateTime _lastBackPressed = DateTime.MinValue;

    private async void ShowToast(string message)
    {
        var toast = Toast.Make(
            message,
            ToastDuration.Short);

        await toast.Show();
    }

    // ============================================================
    // REGISTER
    // ============================================================

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Task.WhenAll(
            RegisterButton.RotateTo(
                10,
                100,
                Easing.Linear),

            RegisterButton.RotateTo(
                -10,
                100,
                Easing.Linear),

            RegisterButton.ScaleTo(
                0.5,
                100,
                Easing.Linear),

            RegisterButton.ScaleTo(
                0,
                150,
                Easing.CubicIn));

        RegisterButton.IsVisible = false;

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            string firstName =
                RegisterNameEntry.Text?.Trim();

            string lastName =
                RegisterLastNameEntry.Text?.Trim();

            string email =
                RegisterEmailEntry.Text?.Trim();

            string phone =
                RegisterPhoneEntry.Text?.Trim();

            string password =
                RegisterPasswordEntry.Text;

            string passwordConfirmation =
                RegisterConfirmPasswordEntry.Text;

            // ========================================================
            // VALIDATION
            // ========================================================

            if (string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(passwordConfirmation))
            {
                await this.ShowPopupAsync(
                    new EnterAllFailed());

                return;
            }

            if (!PhoneNumberFormatter.TryNormalize(phone, out var normalizedPhone))
            {
                ShowPhonePrefixError();
                return;
            }

            phone = normalizedPhone;
            RegisterPhoneEntry.Text = normalizedPhone;
            HidePhonePrefixError();

            if (!IsValidEmail(email))
            {
                await this.ShowPopupAsync(
                    new EroreInputEmaile());

                return;
            }

            if (password != passwordConfirmation)
            {
                await this.ShowPopupAsync(
                    new Paswordmatch());

                return;
            }

            if (password.Length < 6)
            {
                await this.ShowPopupAsync(
                    new paslen());

                return;
            }

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await this.ShowPopupAsync(
                    new NoEnternetConacted());

                return;
            }

            // ========================================================
            // AUTHENTICATION SERVICE
            // ========================================================

            var authService =
                App.Current?.Handler?.MauiContext?
                    .Services.GetService<AuthenticationService>();

            if (authService == null)
            {
                Console.WriteLine(
                    "❌ [SinginPage] AuthenticationService not available");

                await this.ShowPopupAsync(
                    new NoServerResponse());

                return;
            }

            Console.WriteLine(
                "📤 [SinginPage] Sending registration request...");

            var (
                success,
                accessToken,
                refreshToken,
                user,
                statusCode,
                errorMessage
            ) = await authService.RegisterAsync(
                firstName,
                lastName,
                email,
                phone,
                password,
                passwordConfirmation);

            Console.WriteLine(
                $"📥 [SinginPage] Register result: " +
                $"Success={success}, " +
                $"Status={statusCode}, " +
                $"HasToken={!string.IsNullOrWhiteSpace(accessToken)}, " +
                $"Message={errorMessage}");

            // ========================================================
            // RATE LIMIT
            // ========================================================

            if (statusCode == 429)
            {
                await DisplayAlert(
                    "Too many attempts",
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "Too many registration attempts. Please wait and try again later."
                        : errorMessage,
                    "OK");

                return;
            }

            // ========================================================
            // SUCCESS
            // ========================================================

            if (success)
            {
                // ====================================================
                // NEW BACKEND FLOW
                //
                // Register
                //     ↓
                // OTP
                //     ↓
                // Verify OTP
                //     ↓
                // Access + Refresh Tokens
                //
                // Register does NOT return tokens.
                // ====================================================

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    Console.WriteLine(
                        "⏳ [SinginPage] Registration successful. " +
                        "OTP verification required.");

                    // =================================================
                    // CREATE OTP CONTEXT
                    // =================================================

                    var otpContext = new Auth.OtpContext
                    {
                        RegistrationMethod = "email",
                        Email = email,
                        Phone = phone,
                        MaskedDestination = MaskEmail(email)
                    };

                    Console.WriteLine(
                        "📦 [SinginPage] OTP context created");

                    Console.WriteLine(
                        $"   RegistrationMethod: " +
                        $"{otpContext.RegistrationMethod}");

                    Console.WriteLine(
                        $"   Email: {otpContext.Email}");

                    Console.WriteLine(
                        $"   Phone: {otpContext.Phone}");

                    Console.WriteLine(
                        $"   MaskedDestination: " +
                        $"{otpContext.MaskedDestination}");

                    // =================================================
                    // ORIGINAL PROJECT NAVIGATION
                    //
                    // IMPORTANT:
                    // Do NOT use Shell.Current.GoToAsync here.
                    // The project already supports passing the
                    // context through NavigationService.
                    // =================================================

                    await NavigationService.NavigateToPage(
                        NavigationService.ROUTE_OTP,
                        otpContext);

                    return;
                }

                // ====================================================
                // LEGACY FALLBACK
                //
                // Should not normally happen with the new backend.
                // ====================================================

                Console.WriteLine(
                    "⚠️ [SinginPage] Register returned an access token. " +
                    "Using legacy authenticated registration flow.");

                if (user != null)
                {
                    try
                    {
                        OneSignal.Login(
                            user.Id.ToString());

                        if (!string.IsNullOrWhiteSpace(user.Email))
                        {
                            OneSignal.User.AddTag(
                                "email",
                                user.Email);
                        }

                        OneSignal.User.AddTag(
                            "signup_date",
                            DateTime.Now.ToString("yyyy-MM-dd"));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"⚠️ [SinginPage] OneSignal error: " +
                            $"{ex.Message}");
                    }
                }

                await this.ShowPopupAsync(
                    new CompletedLogin());

                await AppViewModel.Instance
                    .LoadUserDataAsync();

                NavigationService.ResetLogoutFlag();

                // CRITICAL: Set app context BEFORE navigating
                // This allows navigation to proceed with Shell
                NavigationService.SetAppContext();

                await NavigationService.NavigateToTabBarPage(
                    NavigationService.ROUTE_HOME);

                return;
            }

            // ========================================================
            // EMAIL ALREADY EXISTS
            // ========================================================

            if (statusCode == 409)
            {
                var popup = new EmaileUsed(GoToLoginFromRegisterAsync);
                await this.ShowPopupAsync(popup);

                if (popup.GoToLogin)
                    await GoToLoginFromRegisterAsync();

                return;
            }

            // ========================================================
            // VALIDATION ERROR
            // ========================================================

            if (statusCode == 422)
            {
                Console.WriteLine(
                    $"❌ [SinginPage] Validation error: " +
                    $"{errorMessage}");

                if (PhoneNumberFormatter.IsServerPrefixMessage(errorMessage))
                {
                    ShowPhonePrefixError();
                    return;
                }

                await this.ShowPopupAsync(
                    new NoServerResponse());

                return;
            }

            // ========================================================
            // OTHER ERROR
            // ========================================================

            Console.WriteLine(
                $"❌ [SinginPage] Registration failed: " +
                $"{statusCode} - {errorMessage}");

            await this.ShowPopupAsync(
                new NoServerResponse());
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(
                $"❌ [SinginPage] Network error: {ex}");

            await this.ShowPopupAsync(
                new NoServerResponse());
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"❌ [SinginPage] Register Error: {ex}");

            Console.WriteLine(
                $"Message: {ex.Message}");

            Console.WriteLine(
                $"Inner: {ex.InnerException?.Message}");

            await this.ShowPopupAsync(
                new NoServerResponse());
        }
        finally
        {
            if (!_leavingForLogin)
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;

                RegisterButton.Text =
                    AppResource.CreatAcount2;

                RegisterButton.IsVisible = true;

                RegisterButton.Scale = 0;

                await RegisterButton.RotateTo(0);

                await RegisterButton.ScaleTo(
                    1.1,
                    150,
                    Easing.CubicOut);

                await RegisterButton.ScaleTo(
                    1.0,
                    100,
                    Easing.Linear);
            }
        }
    }

    // ============================================================
    // MASK EMAIL
    // ============================================================

    private string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "your email";

        int atIndex =
            email.IndexOf('@');

        if (atIndex <= 0)
            return email;

        string username =
            email.Substring(
                0,
                atIndex);

        string domain =
            email.Substring(
                atIndex);

        if (username.Length <= 2)
        {
            return username[0] +
                   "***" +
                   domain;
        }

        return username.Substring(0, 2) +
               new string(
                   '*',
                   Math.Max(
                       2,
                       username.Length - 2)) +
               domain;
    }

    // ============================================================
    // RESET REGISTER UI
    // ============================================================

    private void ResetRegisterUI()
    {
        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;

        RegisterButton.IsVisible = true;
        RegisterButton.Opacity = 1;
        RegisterButton.Scale = 1;
        RegisterButton.Rotation = 0;
    }

    // ============================================================
    // EMAIL VALIDATION
    // ============================================================

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        string pattern =
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        return Regex.IsMatch(
            email,
            pattern);
    }

    // ============================================================
    // GOOGLE ANIMATION
    // ============================================================

    bool isGoogleButtonAnimating = true;

    async void StartGoogleButtonAnimation()
    {
        while (isGoogleButtonAnimating)
        {
            await GoogleButton.RotateTo(
                2,
                250,
                Easing.SinInOut);

            await GoogleButton.RotateTo(
                -2,
                250,
                Easing.SinInOut);

            await GoogleButton.RotateTo(
                0,
                300,
                Easing.SinInOut);

            await Task.Delay(600);
        }
    }

    void StopGoogleButtonAnimation()
    {
        isGoogleButtonAnimating = false;

        GoogleButton.RotateTo(
            0,
            300,
            Easing.CubicOut);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        PhonePrefixHintLabel.Text =
            PhoneNumberFormatter.GetGuidanceMessage();

        _phoneNational = PhoneNumberFormatter.ReadNational(RegisterPhoneEntry.Text);
        RegisterPhoneEntry.Text = PhoneNumberFormatter.GermanyPrefix + _phoneNational;

        isGoogleButtonAnimating = true;

        StartGoogleButtonAnimation();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        StopGoogleButtonAnimation();
    }

    // ============================================================
    // GOOGLE LOGIN
    // ============================================================

    private async void GoogleButton_Clicked(
        object sender,
        EventArgs e)
    {
        try
        {
            fglogin.IsVisible = true;
            fglogin.IsVisible = false;

            GoogleButton.IsVisible = false;

            GoogleLoadingIndicator.IsVisible = true;
            GoogleLoadingIndicator.IsRunning = true;

            if (MauiProgram.firebaseconfig == null ||
                MauiProgram.firebaseconfig.Providers == null ||
                MauiProgram.firebaseconfig.Providers.Length == 0)
            {
                Console.WriteLine(
                    "Firebase configuration or providers are missing.");

                await DisplayAlert(
                    "Configuration error",
                    "Firebase providers not configured",
                    "OK");

                return;
            }

            var providerEntry =
                MauiProgram.firebaseconfig.Providers[0];

            if (providerEntry == null ||
                providerEntry.ProviderType == null)
            {
                Console.WriteLine(
                    "Provider entry or ProviderType is null.");

                await DisplayAlert(
                    "Configuration error",
                    "Firebase provider invalid",
                    "OK");

                return;
            }

            var provider =
                providerEntry.ProviderType;

            var signInTask =
                MauiProgram.firebaseclient.SignInWithRedirectAsync(
                    provider,
                    async uri =>
                    {
                        webView.Source = uri;

                        webView.IsVisible = true;

                        fg.IsVisible = true;

                        fglogin.IsVisible = false;

                        webView.Opacity = 1;

                        string finalUrl =
                            await WaitForNavigationToUrlAsync(
                                "https://test-23def.web.app/__/auth/handler",
                                TimeSpan.FromSeconds(80));

                        fg.IsVisible = false;

                        fglogin.IsVisible = true;

                        webView.IsVisible = false;

                        webView.Source = null;

                        return finalUrl;
                    });

            var completed =
                await Task.WhenAny(
                    signInTask,
                    Task.Delay(
                        TimeSpan.FromSeconds(70)));

            if (completed != signInTask)
            {
                throw new TimeoutException(
                    "Firebase sign-in timed out.");
            }

            userCredential =
                await signInTask;

            if (userCredential != null)
            {
                var user =
                    userCredential.User;

                Console.WriteLine(
                    $"Logged in: " +
                    $"{user.Info.DisplayName} " +
                    $"({user.Info.Email})");

                await DisplayAlert(
                    "Sign In",
                    "Welcome: " +
                    user.Info.DisplayName,
                    "OK");

                IsLogged = true;
            }

            if (userCredential?.User != null)
            {
                var userId =
                    userCredential.User.Uid;

                var idToken =
                    await userCredential.User
                        .GetIdTokenAsync();

                try
                {
                    var authService =
                        AppServices.Get<AuthenticationService>();

                    if (authService != null)
                    {
                        await authService.LoginWithGoogleAsync(
                            idToken,
                            userId,
                            userCredential.User.Info.Email,
                            userCredential.User.Info.DisplayName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Error sending token to backend: " +
                        $"{ex.Message}");
                }

                OneSignalService.RegisterUser(
                    userId);

                OneSignalService.AddTag(
                    "email",
                    userCredential.User.Info.Email);

                OneSignalService.AddTag(
                    "signup_type",
                    "google");

                await AppViewModel.Instance
                    .LoadUserDataAsync();

                NavigationService.ResetLogoutFlag();

                await ShellNavigationManager
                    .NavigateToHomeAndClear();
            }
        }
        catch (FirebaseAuthHttpException fae)
        {
            Console.WriteLine(
                "FirebaseAuthHttpException: " +
                fae);

            var msg =
                !string.IsNullOrEmpty(fae.Message)
                    ? fae.Message
                    : "Firebase HTTP error during authentication.";

            await DisplayAlert(
                "Authentication error",
                msg,
                "OK");
        }
        catch (TimeoutException tex)
        {
            Console.WriteLine(
                "Timeout: " +
                tex.Message);

            await DisplayAlert(
                "Timeout",
                "Authentication request timed out. Please try again.",
                "OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "Exception during Google sign-in: " +
                ex);

            var inner =
                ex.InnerException;

            string detail =
                ex.Message;

            if (inner != null)
            {
                detail +=
                    "\nInner: " +
                    inner.Message +
                    " (" +
                    inner.GetType().FullName +
                    ")";
            }

            await DisplayAlert(
                "Sign-in failed",
                detail,
                "OK");
        }
        finally
        {
            GoogleLoadingIndicator.IsRunning = false;
            GoogleLoadingIndicator.IsVisible = false;

            GoogleButton.IsVisible = true;

            fglogin.IsVisible = true;
        }
    }

    // ============================================================
    // PRIVACY
    // ============================================================

    private async void TapGestureRecognizer_Tapped_2(
        object sender,
        TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(
            NavigationService.ROUTE_POLICY_PRIVACY_AUTH);
    }

    // ============================================================
    // CHECKBOX
    // ============================================================

    private void CheckBox_CheckChanged(
        object sender,
        EventArgs e)
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

    // ============================================================
    // WAIT FOR GOOGLE REDIRECT
    // ============================================================

    private async Task<string> WaitForNavigationToUrlAsync(
        string targetUrl,
        TimeSpan timeout)
    {
        var tcs =
            new TaskCompletionSource<string>();

        EventHandler<WebNavigatedEventArgs>? handler = null;

        handler = (s, e) =>
        {
            try
            {
                if (!string.IsNullOrEmpty(e.Url) &&
                    e.Url.StartsWith(
                        targetUrl,
                        StringComparison.OrdinalIgnoreCase))
                {
                    webView.Navigated -= handler;

                    tcs.TrySetResult(
                        e.Url);
                }
            }
            catch (Exception ex)
            {
                webView.Navigated -= handler;

                tcs.TrySetException(ex);
            }
        };

        webView.Navigated += handler;

        var delayTask =
            Task.Delay(timeout);

        var completed =
            await Task.WhenAny(
                tcs.Task,
                delayTask);

        if (completed == tcs.Task)
            return await tcs.Task;

        webView.Navigated -= handler;

        throw new TimeoutException(
            "Navigation to redirect URL timed out.");
    }

    // ============================================================
    // EMAIL TEXT CHANGED
    // ============================================================

    private void OnEmailTextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        var email =
            e.NewTextValue?.Trim();

        if (string.IsNullOrWhiteSpace(email))
        {
            ShowLiveError(
                AppResource.Emailisrequired);

            return;
        }

        if (!IsValidEmaile(email))
        {
            ShowLiveError(
                AppResource.ErorEmailInput);

            return;
        }

        HideLiveError();
    }

    private bool IsValidEmaile(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        string pattern =
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        return Regex.IsMatch(
            email,
            pattern);
    }

    private async void ShowLiveError(
        string message)
    {
        LiveErrorLabel.Text =
            message;

        LiveErrorLabel.TextColor =
            Colors.Red;

        LiveErrorLabel.IsVisible =
            true;

        LiveErrorLabel.Opacity =
            0;

        await LiveErrorLabel.FadeTo(
            1,
            150);
    }

    private void HideLiveError()
    {
        LiveErrorLabel.IsVisible = false;
    }

    // ============================================================
    // CONFIRM PASSWORD
    // ============================================================

    private void OnConfirmPasswordTextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        ValidatePasswords();
    }

    private void ValidatePasswords()
    {
        var password =
            RegisterPasswordEntry.Text;

        var confirm =
            RegisterConfirmPasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(confirm))
        {
            PasswordMatchLabel.Text =
                AppResource.Thisfieldisrequired;

            PasswordMatchLabel.TextColor =
                Colors.Orange;

            PasswordMatchLabel.IsVisible =
                true;

            return;
        }

        if (password != confirm)
        {
            PasswordMatchLabel.Text =
                AppResource.Passwordnotmatch;

            PasswordMatchLabel.TextColor =
                Colors.Red;

            PasswordMatchLabel.IsVisible =
                true;

            return;
        }

        PasswordMatchLabel.Text =
            AppResource.Passwordsmatch;

        PasswordMatchLabel.TextColor =
            Colors.Green;

        PasswordMatchLabel.IsVisible =
            true;
    }

    // ============================================================
    // FIRST NAME
    // ============================================================

    private bool _isUpdatingPhonePrefix;
    private int _phoneCursorFix;
    private int _phoneTextFix;
    private string _phoneNational = string.Empty;

    private void RegisterPhoneEntry_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (_isUpdatingPhonePrefix)
            return;

        var edit = PhoneNumberFormatter.MergeFixedPrefix(_phoneNational, e.NewTextValue);
        _phoneNational = edit.National;

        ApplyCorrectedPhoneText(edit.Text);
        PlacePhoneCursor(edit.Cursor);
        HidePhonePrefixError();
    }

    private void ApplyCorrectedPhoneText(string corrected)
    {
        var fix = ++_phoneTextFix;

        void Apply()
        {
            if (fix != _phoneTextFix)
                return;

            var entry = RegisterPhoneEntry?.EntryView;
            if (string.Equals(RegisterPhoneEntry.Text, corrected, StringComparison.Ordinal)
                && string.Equals(entry?.Text, corrected, StringComparison.Ordinal))
                return;

            _isUpdatingPhonePrefix = true;
            RegisterPhoneEntry.Text = corrected;
            if (entry != null)
                entry.Text = corrected;
            _isUpdatingPhonePrefix = false;
        }

        Apply();
        Dispatcher.Dispatch(Apply);
    }

    private void PlacePhoneCursor(int position)
    {
        var fix = ++_phoneCursorFix;

        void Apply()
        {
            if (fix != _phoneCursorFix)
                return;

            var entry = RegisterPhoneEntry?.EntryView;
            if (entry == null)
                return;

            var length = entry.Text?.Length ?? 0;
            var minimum = PhoneNumberFormatter.GermanyPrefix.Length;
            var target = Math.Clamp(position, minimum, Math.Max(minimum, length));
            entry.CursorPosition = target;
            entry.SelectionLength = 0;
        }

        Apply();
        Dispatcher.Dispatch(Apply);
    }

    private void ShowPhonePrefixError()
    {
        PhonePrefixHintLabel.Text =
            PhoneNumberFormatter.GetGuidanceMessage();

        PhonePrefixHintBorder.Stroke = Color.FromArgb("#E57373");
        PhonePrefixHintBorder.BackgroundColor = Color.FromArgb("#33E57373");
    }

    private void HidePhonePrefixError()
    {
        PhonePrefixHintBorder.Stroke = Color.FromArgb("#66EBD750");
        PhonePrefixHintBorder.BackgroundColor = Color.FromArgb("#1AFFFFFF");
    }

    private void RegisterNameEntry_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        var name =
            e.NewTextValue?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            ShowNameError(
                AppResource.Thisfieldisrequired);
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

    private void ShowNameError(
        string thisfieldisrequired)
    {
        ErorNameInput.Text =
            thisfieldisrequired;

        ErorNameInput.TextColor =
            Colors.Red;

        ErorNameInput.IsVisible =
            true;
    }

    // ============================================================
    // LAST NAME
    // ============================================================

    private void RegisterLastNameEntry_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        var lastName =
            e.NewTextValue?.Trim();

        if (string.IsNullOrWhiteSpace(lastName))
        {
            ShowLastNameError(
                AppResource.Thisfieldisrequired);
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

    private void ShowLastNameError(
        string thisfieldisrequired)
    {
        ErorLastNameInput.Text =
            thisfieldisrequired;

        ErorLastNameInput.TextColor =
            Colors.Red;

        ErorLastNameInput.IsVisible =
            true;
    }

    // ============================================================
    // TERMS
    // ============================================================

    private async void TapGestureRecognizer_Tapped_1(
        object sender,
        TappedEventArgs e)
    {
        await NavigationService.NavigateToPage(
            NavigationService.ROUTE_TermsAndConditions_Athun);
    }

    // ============================================================
    // PASSWORD VALIDATION
    // ============================================================

    private bool _hasMinLength;
    private bool _hasUppercase;
    private bool _hasNumber;
    private bool _isPasswordValid;

    private void RegisterPasswordEntry_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        var password =
            e.NewTextValue ?? "";

        _hasMinLength =
            password.Length >= 8;

        _hasUppercase =
            Regex.IsMatch(
                password,
                "[A-Z]");

        _hasNumber =
            Regex.IsMatch(
                password,
                "[0-9]");

        _isPasswordValid =
            _hasMinLength &&
            _hasUppercase &&
            _hasNumber;

        UpdatePasswordUI();

        ValidateForm();
    }

    private void UpdatePasswordUI()
    {
        SetRuleUI(
            LengthFrame,
            LengthIcon,
            _hasMinLength);

        SetRuleUI(
            UpperFrame,
            UpperIcon,
            _hasUppercase);

        SetRuleUI(
            NumberFrame,
            NumberIcon,
            _hasNumber);
    }

    private void SetRuleUI(
        Frame frame,
        Label icon,
        bool isValid)
    {
        frame.BackgroundColor =
            isValid
                ? Colors.Green
                : Colors.Red;

        icon.Text =
            isValid
                ? "✔"
                : "○";
    }

    private void ValidateForm()
    {
        var isNameValid =
            !string.IsNullOrWhiteSpace(
                RegisterNameEntry.Text);

        var isLastNameValid =
            !string.IsNullOrWhiteSpace(
                RegisterLastNameEntry.Text);

        var isEmailValid =
            IsValidEmaile(
                RegisterEmailEntry.Text);

        var isPasswordValid =
            _hasMinLength &&
            _hasUppercase &&
            _hasNumber;

        RegisterButton.IsEnabled =
            isNameValid &&
            isLastNameValid &&
            isEmailValid &&
            isPasswordValid;
    }

    // ============================================================
    // BACK BUTTON
    // ============================================================

    protected override bool OnBackButtonPressed()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(
                async () =>
                {
                    try
                    {
                        await NavigationService.HandleBackButton(
                            NavigationService.ROUTE_SIGNIN);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[SinginPage] Back button error: " +
                            $"{ex.Message}");
                    }
                });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[SinginPage] OnBackButtonPressed crash: " +
                $"{ex.Message}");

            return true;
        }
    }

    private async void TapGestureRecognizer_Tapped_3(object sender, TappedEventArgs e)
    {
        // In Authentication context (NavigationPage), pop back to LoginPage
        try
        {
            var navPage = Application.Current?.MainPage as NavigationPage;
            if (navPage != null && navPage.Navigation.NavigationStack.Count > 1)
            {
                await navPage.PopAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Navigation] Error popping in SinginPage: {ex.Message}");
        }
    }
}