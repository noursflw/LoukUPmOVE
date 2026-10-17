using CommunityToolkit.Maui.Views;
using loukupm.View.MassgingApp;
using loukupm.ViewModel;
using loukupm.Services;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace loukupm.View;

public partial class RestPassword : ContentPage
{
    private bool _isLoading;

    public RestPassword()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);
        BindingContext = AppViewModel.Instance;
    }

    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await NavigationService.HandleBackButton(NavigationService.ROUTE_REST_PASSWORD);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestPassword] Back button error: {ex.Message}");
            }
        });
        return true;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        if (_isLoading) return;

        try
        {
            _isLoading = true;
            if (SendOtpButton != null)
                SendOtpButton.IsVisible = false;

            var viewModel = BindingContext as AppViewModel ?? AppViewModel.Instance;
            if (viewModel == null)
            {
                await DisplayAlert("خطأ", "حدث خطأ في تحميل البيانات", "حسناً");
                return;
            }

            var email = EmailTextField?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(email))
                email = viewModel.Email?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                await this.ShowPopupAsync(new EroreInputEmaile());
                return;
            }

            email = email.Trim().ToLowerInvariant();
            viewModel.Email = email;
            viewModel.OtpCode = string.Empty;

            if (!IsValidEmail(email))
            {
                await this.ShowPopupAsync(new EroreInputEmaile());
                return;
            }

            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await this.ShowPopupAsync(new NoEnternetConacted());
                return;
            }

            ShowLoadingIndicator(true);

            var (success, alreadyShowedError) = await SendForgotPasswordAsync(email);

            if (success)
            {
                PasswordResetSession.Start(email);

                await this.ShowPopupAsync(new CompletSendEmail());

                // Guide §9: single screen — user types OTP + new password
                await Navigation.PushAsync(new EditPasswordVerification(email));
            }
            else if (!alreadyShowedError)
            {
                await this.ShowPopupAsync(new NoServerResponse());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RestPassword] Button_Clicked error: {ex}");
            await this.ShowPopupAsync(new NoServerResponse());
        }
        finally
        {
            _isLoading = false;
            ShowLoadingIndicator(false);
            if (SendOtpButton != null)
                SendOtpButton.IsVisible = true;
        }
    }

    /// <summary>
    /// POST /api/auth/forgot-password
    /// Requires: { email, registration_method }
    /// Never auto-fill OTP — user must enter the code from email.
    /// </summary>
    private async Task<(bool Success, bool AlreadyShowedError)> SendForgotPasswordAsync(string email)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                email,
                registration_method = "email"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"[RestPassword] forgot-password email={email}");

            var response = await client.PostAsync(
                "https://lookupfriseur.com/api/auth/forgot-password",
                content);

            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine(
                $"[RestPassword] forgot-password ({(int)response.StatusCode}): {body}");

            if (response.IsSuccessStatusCode)
                return (true, false);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                body.Contains("USER_NOT_FOUND", StringComparison.OrdinalIgnoreCase))
            {
                await this.ShowPopupAsync(new EmaileIsNotFound());
                return (false, true);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity ||
                response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                await this.ShowPopupAsync(new EroreInputEmaile());
                return (false, true);
            }

            if ((int)response.StatusCode == 429)
            {
                await this.ShowPopupAsync(new WateResposeOTP());
                return (false, true);
            }

            await this.ShowPopupAsync(new NoServerResponse());
            return (false, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RestPassword] SendForgotPasswordAsync error: {ex}");
            await this.ShowPopupAsync(new NoServerResponse());
            return (false, true);
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return string.Equals(addr.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private void ShowLoadingIndicator(bool show)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                if (LoadingIndicator != null)
                {
                    LoadingIndicator.IsVisible = show;
                    LoadingIndicator.IsRunning = show;
                }

                if (SendOtpButton != null)
                {
                    SendOtpButton.IsEnabled = !show;
                    SendOtpButton.Opacity = show ? 0.6 : 1.0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ShowLoadingIndicator: {ex.Message}");
            }
        });
    }
}

/// <summary>
/// Holds email + OTP across the password-reset flow.
/// </summary>
public static class PasswordResetSession
{
    public static string Email { get; private set; } = string.Empty;
    public static string Otp { get; private set; } = string.Empty;

    public static void Start(string email, string otp = null)
    {
        Email = email?.Trim() ?? string.Empty;
        Otp = string.Empty; // never auto-store server OTP — user must type it
    }

    public static void SetEmail(string email)
    {
        Email = email?.Trim() ?? string.Empty;
    }

    public static void SetOtp(string otp)
    {
        Otp = NormalizeDigits(otp);
    }

    public static void ClearOtp()
    {
        Otp = string.Empty;
    }

    public static void Clear()
    {
        Email = string.Empty;
        Otp = string.Empty;
    }

    public static bool HasOtp => !string.IsNullOrWhiteSpace(Otp);
    public static bool HasEmail => !string.IsNullOrWhiteSpace(Email);

    private static string NormalizeDigits(string raw)
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
