using CommunityToolkit.Maui.Views;
using loukupm.Services;
using loukupm.View.MassgingApp;
using loukupm.ViewModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace loukupm.View;

/// <summary>
/// Guide §9: user enters OTP + new password → POST reset-password
/// </summary>
public partial class EditPasswordVerification : ContentPage
{
    private bool _isProcessing;
    private bool _navigatingAway;
    private readonly string _email;

    public EditPasswordVerification() : this(PasswordResetSession.Email)
    {
    }

    public EditPasswordVerification(string email)
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);
        NavigationPage.SetHasNavigationBar(this, false);
        BindingContext = AppViewModel.Instance;

        _email = (email ?? PasswordResetSession.Email)?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(_email))
        {
            PasswordResetSession.SetEmail(_email);
            if (AppViewModel.Instance != null)
                AppViewModel.Instance.Email = _email;
        }

        // Clear any leftover OTP — user must type it
        PasswordResetSession.ClearOtp();
        if (AppViewModel.Instance != null)
            AppViewModel.Instance.OtpCode = string.Empty;

        Console.WriteLine($"[EditPasswordVerification] init email={_email}");
    }

    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(NavigationService.ROUTE_EDIT_PASSWORD_VERIFICATION);
        });
        return true;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await NavigationService.HandleBackButton(NavigationService.ROUTE_EDIT_PASSWORD_VERIFICATION);
    }

    private async void SavePassword_Clicked(object sender, EventArgs e)
    {
        if (_isProcessing) return;

        try
        {
            _isProcessing = true;
            SetSaveEnabled(false);

            var viewModel = BindingContext as AppViewModel ?? AppViewModel.Instance;

            var email = !string.IsNullOrWhiteSpace(_email)
                ? _email
                : (PasswordResetSession.HasEmail ? PasswordResetSession.Email : viewModel?.Email?.Trim());

            email = email?.Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(email))
            {
                await this.ShowPopupAsync(new EmaileIsNotFound());
                return;
            }

            // ONLY what the user typed — no session/debug prefill
            var otp = NormalizeOtp(OtpField?.Text);

            if (string.IsNullOrWhiteSpace(otp) || otp.Length < 4)
            {
                await this.ShowPopupAsync(new CodeNotIncorrect(
                    "يرجى إدخال رمز التحقق المرسل إلى بريدك"));
                return;
            }

            var newPassword = PasswordField?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(newPassword))
                newPassword = viewModel?.Password?.Trim();

            var confirmPassword = ConfirmPasswordField?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(confirmPassword))
                confirmPassword = viewModel?.ConfirmPassword?.Trim();

            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                await this.ShowPopupAsync(new PasswordResetFailedPopup());
                return;
            }

            if (newPassword != confirmPassword)
            {
                await this.ShowPopupAsync(new Paswordmatch());
                return;
            }

            if (newPassword.Length < 8)
            {
                await this.ShowPopupAsync(new PasswordResetFailedPopup());
                return;
            }

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await this.ShowPopupAsync(new NoEnternetConacted());
                return;
            }

            Console.WriteLine(
                $"[EditPasswordVerification] reset email={email} otpLen={otp.Length}");

            bool success = await ResetPasswordAsync(email, otp, newPassword, confirmPassword);

            if (success)
            {
                if (viewModel != null)
                {
                    viewModel.Password = string.Empty;
                    viewModel.ConfirmPassword = string.Empty;
                    viewModel.OtpCode = string.Empty;
                }
                PasswordResetSession.Clear();

                await this.ShowPopupAsync(new PasswordResetSuccessPopup());

                _navigatingAway = true;
                await GoToLoginPageAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EditPasswordVerification] Save error: {ex}");
            if (!_navigatingAway)
                await this.ShowPopupAsync(new PasswordResetFailedPopup());
        }
        finally
        {
            _isProcessing = false;
            if (!_navigatingAway)
                SetSaveEnabled(true);
        }
    }

    /// <summary>
    /// Always return to Login after successful password reset (auth NavigationPage root).
    /// </summary>
    private static async Task GoToLoginPageAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                NavigationService.SetLoginContext();

                // Prefer popping the auth stack back to LoginPage
                if (Application.Current?.MainPage is NavigationPage navPage)
                {
                    while (navPage.Navigation.NavigationStack.Count > 1)
                        await navPage.PopAsync(false);

                    if (navPage.CurrentPage is LoginPage)
                    {
                        Console.WriteLine("[EditPasswordVerification] Returned to LoginPage via Pop");
                        return;
                    }
                }

                // Hard reset root to Login if stack pop wasn't enough
                var direction = App.GetSavedFlowDirection();
                App.SetRootPage(new NavigationPage(new LoginPage { FlowDirection = direction })
                {
                    FlowDirection = direction
                });
                Console.WriteLine("[EditPasswordVerification] Root reset to LoginPage");
            }
            catch (Exception navEx)
            {
                Console.WriteLine($"[EditPasswordVerification] GoToLogin failed: {navEx}");
                try
                {
                    var direction = App.GetSavedFlowDirection();
                    App.SetRootPage(new NavigationPage(new LoginPage { FlowDirection = direction })
                    {
                        FlowDirection = direction
                    });
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"[EditPasswordVerification] Login root fallback failed: {ex2.Message}");
                }
            }
        });
    }

    private void SetSaveEnabled(bool enabled)
    {
        try
        {
            // Avoid MaterialButton crash when handler already gone
            if (SaveButton?.Handler == null)
                return;
            SaveButton.IsEnabled = enabled;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EditPasswordVerification] SetSaveEnabled ignored: {ex.Message}");
        }
    }

    private async Task<bool> ResetPasswordAsync(
        string email,
        string otp,
        string password,
        string passwordConfirmation)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new Dictionary<string, string>
            {
                ["registration_method"] = "email",
                ["email"] = email,
                ["otp"] = otp,
                ["password"] = password,
                ["password_confirmation"] = passwordConfirmation
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "https://lookupfriseur.com/api/auth/reset-password",
                content);

            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine(
                $"[EditPasswordVerification] reset-password ({(int)response.StatusCode}): {body}");

            if (response.IsSuccessStatusCode)
                return true;

            var serverMessage = TryExtractMessage(body);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                body.Contains("USER_NOT_FOUND", StringComparison.OrdinalIgnoreCase))
            {
                await this.ShowPopupAsync(new EmaileIsNotFound());
            }
            else if (IsInvalidOtpResponse(body, response.StatusCode))
            {
                await this.ShowPopupAsync(new CodeNotIncorrect(
                    serverMessage ?? "رمز إعادة تعيين كلمة المرور غير صالح أو منتهي الصلاحية."));
            }
            else if ((int)response.StatusCode == 429)
            {
                await this.ShowPopupAsync(new WateResposeOTP());
            }
            else
            {
                await this.ShowPopupAsync(new PasswordResetFailedPopup());
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EditPasswordVerification] ResetPasswordAsync error: {ex}");
            await this.ShowPopupAsync(new PasswordResetFailedPopup());
            return false;
        }
    }

    private static string TryExtractMessage(string body)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("message", out var msg))
                return msg.GetString();
            if (doc.RootElement.TryGetProperty("error", out var err))
                return err.GetString();
        }
        catch { }
        return null;
    }

    private static bool IsInvalidOtpResponse(string body, System.Net.HttpStatusCode status)
    {
        if (status != System.Net.HttpStatusCode.UnprocessableEntity &&
            status != System.Net.HttpStatusCode.BadRequest)
            return false;

        if (string.IsNullOrWhiteSpace(body))
            return true;

        var lower = body.ToLowerInvariant();
        return lower.Contains("invalid_otp") ||
               lower.Contains("invalid or expired otp") ||
               lower.Contains("invalid otp") ||
               lower.Contains("expired otp") ||
               lower.Contains("\"otp\"") ||
               body.Contains("رمز", StringComparison.Ordinal);
    }

    private static string NormalizeOtp(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        var sb = new StringBuilder(raw.Length);
        foreach (var c in raw.Trim())
        {
            if (c >= '0' && c <= '9')
                sb.Append(c);
            else if (c >= '٠' && c <= '٩')
                sb.Append((char)('0' + (c - '٠')));
            else if (c >= '۰' && c <= '۹')
                sb.Append((char)('0' + (c - '۰')));
        }
        return sb.ToString();
    }
}
