using CommunityToolkit.Maui.Views;
using loukupm.Model;
using loukupm.services;
using loukupm.View.MassgingApp;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static loukupm.Model.Auth;

namespace loukupm.View;

public partial class SinginPage : ContentPage
{
    public SinginPage()
    {
        InitializeComponent();
    }

    private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("LoginPage");
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("LoginPage");
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
}
