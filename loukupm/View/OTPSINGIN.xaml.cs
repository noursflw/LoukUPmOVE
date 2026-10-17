using CommunityToolkit.Maui.Views;
using loukupm.Langue;
using loukupm.Model;
using loukupm.services;
using loukupm.Services;
using loukupm.View.MassgingApp;
using loukupm.ViewModel;
using System.Text.Json;

namespace loukupm.View;

[QueryProperty(nameof(Data), "data")]
public partial class OTPSINGIN : ContentPage
{
    private readonly ApiServices _apiServices = new();

    private Auth.OtpContext? _otpContext;
    private CancellationTokenSource? _timerCancellation;

    private int _remainingSeconds = 60;
    private const int RESEND_COUNTDOWN = 60;

    private bool _isInitialized;
    private bool _isVerifying;
    private bool _isMessageVisible;

    /// <summary>
    /// Receives the serialized OtpContext from Shell navigation.
    /// </summary>
    public string Data
    {
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            try
            {
                string decodedJson = Uri.UnescapeDataString(value);

                var context = JsonSerializer.Deserialize<Auth.OtpContext>(
                    decodedJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (context == null)
                {
                    Console.WriteLine("❌ [OTPSINGIN] Deserialized OtpContext is null");
                    return;
                }

                _otpContext = context;

                Console.WriteLine(
                    $"✅ [OTPSINGIN] OtpContext received: {_otpContext.MaskedDestination}");

                // The QueryProperty setter can be called after the page
                // has already been created, so initialize the UI here too.
                if (_isInitialized)
                {
                    MainThread.BeginInvokeOnMainThread(InitializePageWithContext);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"❌ [OTPSINGIN] Failed to deserialize OtpContext: {ex}");

                _otpContext = null;
            }
        }
    }

    public OTPSINGIN()
    {
        try
        {
            InitializeComponent();

            this.InitializeLanguageTracking();

            _isInitialized = true;

            Console.WriteLine(
                "✅ [OTPSINGIN] Page initialized successfully");

            // In case QueryProperty was already populated before
            // the page finished initializing.
            if (_otpContext != null)
            {
                MainThread.BeginInvokeOnMainThread(
                    InitializePageWithContext);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"❌ [OTPSINGIN] Initialization failed: {ex}");

            throw;
        }
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        try
        {
            Console.WriteLine("➡️ [OTPSINGIN] OnNavigatedTo");

            // QueryProperty is the primary source.
            // BindingContext is kept as a fallback.
            if (_otpContext == null &&
                BindingContext is Auth.OtpContext context)
            {
                _otpContext = context;

                Console.WriteLine(
                    $"✅ [OTPSINGIN] OtpContext loaded from BindingContext: " +
                    $"{_otpContext.MaskedDestination}");
            }

            if (_otpContext == null)
            {
                Console.WriteLine(
                    "⚠️ [OTPSINGIN] No OTP context provided");

                DisplayError(AppResource.OtpSigninNoContext);

                return;
            }

            InitializePageWithContext();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"❌ [OTPSINGIN] OnNavigatedTo error: {ex}");
        }
    }

    private void InitializePageWithContext()
    {
        try
        {
            if (_otpContext == null)
            {
                Console.WriteLine(
                    "❌ [OTPSINGIN] Cannot initialize page: OtpContext is null");

                return;
            }

            Console.WriteLine(
                $"📱 [OTPSINGIN] Initializing with: " +
                $"{_otpContext.MaskedDestination}");

            MaskedDestinationLabel.Text =
                string.IsNullOrWhiteSpace(_otpContext.MaskedDestination)
                    ? AppResource.OtpSigninYourContact
                    : _otpContext.MaskedDestination;

            OtpEntry.Text = string.Empty;

            VerifyButton.IsEnabled = false;

            // Don't force focus before the page is actually visible.
            Dispatcher.DispatchDelayed(
                TimeSpan.FromMilliseconds(250),
                () =>
                {
                    try
                    {
                        if (IsVisible && OtpEntry.IsEnabled)
                        {
                            OtpEntry.Focus();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"⚠️ [OTPSINGIN] Focus failed: {ex.Message}");
                    }
                });
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"❌ [OTPSINGIN] Context initialization error: {ex}");
        }
    }

    private void OnOtpTextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            string otp = e.NewTextValue ?? string.Empty;

            bool isValidOtp =
                otp.Length == 6 &&
                otp.All(char.IsDigit);

            VerifyButton.IsEnabled =
                isValidOtp &&
                !_isVerifying;

        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"⚠️ [OTPSINGIN] TextChanged error: {ex.Message}");
        }
    }

    private async void OnVerifyClicked(object sender, EventArgs e)
    {
        if (_isVerifying)
            return;

        try
        {
            string otp = OtpEntry.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(otp) ||
                otp.Length != 6)
            {
                DisplayError(AppResource.OtpSigninEnterValidCode);

                return;
            }

            if (!otp.All(char.IsDigit))
            {
                DisplayError(AppResource.OtpSigninDigitsOnly);

                return;
            }

            if (_otpContext == null)
            {
                DisplayError(AppResource.OtpSigninContextMissing);

                return;
            }

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                DisplayError(AppResource.OtpSigninNoInternet);

                return;
            }

            _isVerifying = true;

            await VerifyOtpWithApi(otp);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"❌ [OTPSINGIN] Verify clicked error: {ex}");

            DisplayError(AppResource.OtpSigninErrorTryAgain);
        }
        finally
        {
            _isVerifying = false;
        }
    }

    private async Task VerifyOtpWithApi(string otp)
    {
        try
        {
            ShowLoading(true);

            Console.WriteLine(
                $"🔐 [OTPSINGIN] Verifying OTP");

            if (_otpContext == null)
            {
                Console.WriteLine(
                    "❌ [OTPSINGIN] OTP context is null");

                DisplayError(AppResource.OtpSigninContextMissing);

                return;
            }

            var (
                success,
                accessToken,
                refreshToken,
                user,
                statusCode,
                errorMessage
            ) = await _apiServices.VerifyOtpAsync(
                email: _otpContext.Email,
                phone: _otpContext.Phone,
                otp: otp,
                registrationMethod: _otpContext.RegistrationMethod);

            if (success)
            {
                Console.WriteLine(
                    $"✅ [OTPSINGIN] OTP verified successfully for user: " +
                    $"{user?.Email ?? "unknown"}");

                if (user != null && user.Id > 0)
                    OneSignalService.LinkUser(user.Id.ToString());

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    Console.WriteLine(
                        "❌ [OTPSINGIN] Access token is null or empty");

                    DisplayError(AppResource.OtpSigninIncomplete);

                    return;
                }

                Console.WriteLine(
                    "📥 [OTPSINGIN] Loading user data...");

                try
                {
                    if (AppViewModel.Instance == null)
                    {
                        Console.WriteLine(
                            "❌ [OTPSINGIN] AppViewModel.Instance is null");

                        DisplayError(AppResource.OtpSigninInitFailed);

                        return;
                    }

                    await AppViewModel.Instance.LoadUserDataAsync();

                    Console.WriteLine(
                        "✅ [OTPSINGIN] User data loaded successfully");
                }
                catch (NullReferenceException nex)
                {
                    Console.WriteLine(
                        $"❌ [OTPSINGIN] NullReferenceException " +
                        $"during LoadUserDataAsync: {nex}");

                    DisplayError(AppResource.OtpSigninLoadUserFailed);

                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"❌ [OTPSINGIN] Exception during " +
                        $"LoadUserDataAsync: {ex}");

                    DisplayError(AppResource.OtpSigninLoadUserFailed);

                    return;
                }

                Console.WriteLine(
                    "🏠 [OTPSINGIN] Navigating to Home...");

                try
                {
                    NavigationService.ResetLogoutFlag();

                    // CRITICAL: Set app context BEFORE navigating to home
                    // This allows NavigateToHomeAndClear() to proceed
                    NavigationService.SetAppContext();

                    await ShellNavigationManager
                        .NavigateToHomeAndClear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"❌ [OTPSINGIN] Navigation failed: {ex}");

                    DisplayError(AppResource.OtpSigninNavigationFailed);
                }
            }
            else
            {
                Console.WriteLine(
                    $"❌ [OTPSINGIN] OTP verification failed: " +
                    $"{statusCode} - {errorMessage}");

                string errorMsg = statusCode switch
                {
                    422 =>
                        AppResource.OtpSigninInvalidTryAgain,

                    403 =>
                        AppResource.OtpSigninExpired,

                    429 =>
                        AppResource.OtpSigninTooManyAttempts,

                    0 =>
                        AppResource.OtpSigninNetworkError,

                    _ =>
                        string.IsNullOrWhiteSpace(errorMessage)
                            ? AppResource.OtpSigninVerificationFailed
                            : errorMessage
                };

                DisplayError(errorMsg);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"❌ [OTPSINGIN] Exception during OTP verification: {ex}");

            DisplayError(AppResource.OtpSigninUnexpectedError);
        }
        finally
        {
            ShowLoading(false);
        }
    }

    private async void OnResendClicked(object sender, EventArgs e)
    {
        try
        {
            if (_otpContext == null)
            {
                DisplayError(AppResource.OtpSigninContextMissing);

                return;
            }

            if (!ResendButton.IsEnabled)
                return;

            ResendButton.IsEnabled = false;

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                DisplayError(AppResource.OtpSigninNoInternet);

                ResendButton.IsEnabled = true;

                return;
            }

            Console.WriteLine(
                "📤 [OTPSINGIN] Requesting OTP resend...");

            var (
                success,
                statusCode,
                message,
                resendAfter
            ) = await _apiServices.ResendOtpAsync(
                email: _otpContext.Email,
                phone: _otpContext.Phone,
                registrationMethod: _otpContext.RegistrationMethod);

            if (success)
            {
                Console.WriteLine(
                    "✅ [OTPSINGIN] OTP resent successfully");

                bool sentToPhone =
                    _otpContext.RegistrationMethod?
                        .Equals("phone", StringComparison.OrdinalIgnoreCase) == true;

                DisplayMessage(
                    sentToPhone
                        ? AppResource.OtpSigninNewCodePhone
                        : AppResource.OtpSigninNewCodeEmail);

                // If the API gives us a retry-after value,
                int countdown =
     resendAfter.HasValue && resendAfter.Value > 0
         ? resendAfter.Value
         : RESEND_COUNTDOWN;

                await StartResendCountdown(countdown);
            }
            else
            {
                Console.WriteLine(
                    $"❌ [OTPSINGIN] Resend failed: " +
                    $"{statusCode} - {message}");

                string errorMsg = statusCode switch
                {
                    429 =>
                        AppResource.OtpSigninTooManyResend,

                    _ =>
                        string.IsNullOrWhiteSpace(message)
                            ? AppResource.OtpSigninResendFailed
                            : message
                };

                DisplayError(errorMsg);

                ResendButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"❌ [OTPSINGIN] Exception during resend: {ex}");

            DisplayError(AppResource.OtpSigninResendError);

            ResendButton.IsEnabled = true;
        }
    }

    private async Task StartResendCountdown(int seconds = RESEND_COUNTDOWN)
    {
        try
        {
            _timerCancellation?.Cancel();
            _timerCancellation?.Dispose();

            _timerCancellation =
                new CancellationTokenSource();

            CancellationToken token =
                _timerCancellation.Token;

            _remainingSeconds =
                Math.Max(1, seconds);

            ResendButton.IsVisible = false;
            ResendButton.IsEnabled = false;

            TimerLabel.IsVisible = true;
            TimerLabel.Text =
                string.Format(AppResource.OtpSigninSeconds, _remainingSeconds);

            while (_remainingSeconds > 0)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(1),
                    token);

                if (token.IsCancellationRequested)
                    return;

                _remainingSeconds--;

                TimerLabel.Text =
                    string.Format(AppResource.OtpSigninSeconds, _remainingSeconds);
            }

            if (!token.IsCancellationRequested)
            {
                TimerLabel.IsVisible = false;

                ResendButton.IsVisible = true;
                ResendButton.IsEnabled = true;
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine(
                "ℹ️ [OTPSINGIN] Countdown cancelled");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"❌ [OTPSINGIN] Countdown error: {ex}");
        }
    }

    private void OnBackClicked(object? sender, EventArgs? e)
    {
        try
        {
            CancelTimer();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigationService.HandleBackButton(
                        NavigationService.ROUTE_OTP);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"⚠️ [OTPSINGIN] Navigation error: {ex}");
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"❌ [OTPSINGIN] Back clicked error: {ex}");
        }
    }

    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(
            () => OnBackClicked(null, null));

        return true;
    }

    private void DisplayError(string message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await ShowDesignedMessageAsync(
                    AppResource.EROR,
                    message,
                    isSuccess: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"⚠️ [OTPSINGIN] DisplayError failed: {ex.Message}");
            }
        });
    }

    private void DisplayMessage(string message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await ShowDesignedMessageAsync(
                    AppResource.Sacsess,
                    message,
                    isSuccess: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"⚠️ [OTPSINGIN] DisplayMessage failed: {ex.Message}");
            }
        });
    }

    private async Task ShowDesignedMessageAsync(string title, string message, bool isSuccess)
    {
        if (_isMessageVisible)
            return;

        _isMessageVisible = true;

        try
        {
            await this.ShowPopupAsync(
                new OtpMessagePopup(title, message, isSuccess));
        }
        finally
        {
            _isMessageVisible = false;
        }
    }

    private void ShowLoading(bool show)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                LoadingGrid.IsVisible = show;
                LoadingIndicator.IsRunning = show;

                VerifyButton.IsVisible = !show;
                VerifyButton.IsEnabled = !show && !_isVerifying;

                OtpEntry.IsEnabled = !show;

                // Don't allow resend while verification is running.
                ResendButton.IsEnabled =
                    !show &&
                    TimerLabel.IsVisible == false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"⚠️ [OTPSINGIN] ShowLoading failed: {ex.Message}");
            }
        });
    }

    private void CancelTimer()
    {
        try
        {
            _timerCancellation?.Cancel();
            _timerCancellation?.Dispose();
            _timerCancellation = null;

            Console.WriteLine(
                "ℹ️ [OTPSINGIN] Timer cancelled");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"⚠️ [OTPSINGIN] CancelTimer error: {ex.Message}");
        }
    }
    public void SetOtpContext(Auth.OtpContext context)
    {
        if (context == null)
        {
            Console.WriteLine(
                "❌ [OTPSINGIN] SetOtpContext received null");

            return;
        }

        _otpContext = context;

        Console.WriteLine(
            $"✅ [OTPSINGIN] OtpContext received directly: " +
            $"{_otpContext.MaskedDestination}");

        if (_isInitialized)
        {
            MainThread.BeginInvokeOnMainThread(
                InitializePageWithContext);
        }
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        try
        {
            CancelTimer();

            Console.WriteLine(
                "ℹ️ [OTPSINGIN] Page disappearing");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"⚠️ [OTPSINGIN] OnDisappearing error: {ex.Message}");
        }
    }
}