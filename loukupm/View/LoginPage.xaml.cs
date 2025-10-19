using CommunityToolkit.Maui.Views;
using loukupm.Model;
using loukupm.services;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static loukupm.Model.Auth;

namespace loukupm.View;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new SinginPage());
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new RestPassword());
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//MainPage");
        return true;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim();
        string password = PasswordEntry.Text;

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

        try
        {
           

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };

            HttpResponseMessage response = null;
            string result = null;

           
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    var json = JsonSerializer.Serialize(loginData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await client.PostAsync("https://test.center-yazan.com/api/auth/login", content);
                    result = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                        break;

                    if (i < 2)
                        await Task.Delay(10 * (i + 1)); 
                }
                catch (HttpRequestException)
                {
                    if (i == 2) throw;
                    await Task.Delay(10 * (i + 1));
                }
            }

            if (response == null)
            {
                await DisplayAlert("Œÿ√", "·„ Ì „ «·Õ’Ê· ⁄·Ï «” Ã«»… „‰ «·Œ«œ„", "„Ê«›ﬁ");
                return;
            }

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(result,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (loginResponse?.User == null)
                {
                    await DisplayAlert("Œÿ√ ›Ì «·«” Ã«»…", "ÕœÀ Œÿ√ ›Ì „⁄«·Ã… »Ì«‰«  «·„” Œœ„", "„Ê«›ﬁ");
                    return;
                }

                // ?  Œ“Ì‰ «· Êﬂ‰ »√„«‰
                if (!string.IsNullOrEmpty(loginResponse.Token))
                    await SecureStorage.SetAsync("auth_token", loginResponse.Token);

                await DisplayAlert(" „  ”ÃÌ· «·œŒÊ·", $"„—Õ»« {loginResponse.User.Name} ??", "„Ê«›ﬁ");
                await Shell.Current.GoToAsync("//HomePage");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                  var popup = new NoEqaulData();
                 await this.ShowPopupAsync(popup);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(result,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                string errorMessage = "Ì—ÃÏ  ’ÕÌÕ «·√Œÿ«¡ «· «·Ì…:\n";
                if (errorResponse?.Errors != null)
                {
                    foreach (var error in errorResponse.Errors)
                    {
                        string fieldName = GetArabicFieldName(error.Key);
                        errorMessage += $"ï {fieldName}: {string.Join(", ", error.Value)}\n";
                    }
                }
                else
                {
                    errorMessage = errorResponse?.Message ?? "ÕœÀ Œÿ√ ›Ì  ”ÃÌ· «·œŒÊ·";
                }
                await DisplayAlert("Œÿ√ ›Ì  ”ÃÌ· «·œŒÊ·", errorMessage, "„Ê«›ﬁ");
            }
            else
            {
                await DisplayAlert("Œÿ√ ›Ì «·« ’«·",
                    $"ÕœÀ Œÿ√ ›Ì «·« ’«· »«·Œ«œ„ (—„“ «·Œÿ√: {(int)response.StatusCode})", "„Ê«›ﬁ");
            }
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Œÿ√ ›Ì «·« ’«·", " ⁄–— «·« ’«· »«·Œ«œ„. Õ«Ê· „—… √Œ—Ï ·«Õﬁ«", "„Ê«›ﬁ");
        }
        catch (TaskCanceledException)
        {
            await DisplayAlert("„Â·… «·« ’«·", "«‰ Â  „Â·… «·« ’«· »«·Œ«œ„. Õ«Ê· „Ãœœ«", "„Ê«›ﬁ");
        }
        catch (Exception)
        {
            await DisplayAlert("Œÿ√ €Ì— „ Êﬁ⁄", "ÕœÀ Œÿ√ €Ì— „ Êﬁ⁄. Ì—ÃÏ «·„Õ«Ê·… „—… √Œ—Ï", "„Ê«›ﬁ");
        }
    }

    private string GetArabicFieldName(string fieldName)
    {
        return fieldName.ToLower() switch
        {
            "email" => "«·»—Ìœ «·≈·ﬂ —Ê‰Ì",
            "password" => "ﬂ·„… «·„—Ê—",
            "name" => "«·«”„",
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
}
