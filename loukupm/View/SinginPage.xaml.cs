using loukupm.Model;
using loukupm.services;
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
        string name = RegisterNameEntry.Text?.Trim();
        string email = RegisterEmailEntry.Text?.Trim();
        string password = RegisterPasswordEntry.Text;
        string password_confirmation = RegisterConfirmPasswordEntry.Text;

        // ✅ تحقق من الإدخالات
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(password_confirmation))
        {
            await DisplayAlert("خطأ في الإدخال", "يرجى تعبئة جميع الحقول المطلوبة", "موافق");
            return;
        }

        // ✅ تحقق من صيغة البريد الإلكتروني
        if (!IsValidEmail(email))
        {
            await DisplayAlert("خطأ في البريد الإلكتروني", "يرجى إدخال بريد إلكتروني صحيح", "موافق");
            return;
        }

        // ✅ تحقق من كلمة المرور
        if (password != password_confirmation)
        {
            await DisplayAlert("خطأ في كلمة المرور", "كلمتا المرور غير متطابقتين", "موافق");
            return;
        }

        if (password.Length < 6)
        {
            await DisplayAlert("خطأ في كلمة المرور", "كلمة المرور يجب أن تكون 6 أحرف على الأقل", "موافق");
            return;
        }

        // ✅ تحقق من الاتصال بالإنترنت
        if (Connectivity.NetworkAccess != NetworkAccess.Internet)
        {
            await DisplayAlert("خطأ في الاتصال", "لا يوجد اتصال بالإنترنت. يرجى التحقق من اتصالك", "موافق");
            return;
        }

        var registerData = new RegisterRequest
        {
            Name = name,
            Email = email,
            Password = password,
            password_confirmation = password_confirmation
        };

        try
        {
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

                // ✅ تخزين التوكن بطريقة آمنة
                if (!string.IsNullOrEmpty(registerResponse?.Token))
                    await SecureStorage.SetAsync("auth_token", registerResponse.Token);

                await DisplayAlert("تم التسجيل", "مرحباً بك في التطبيق 🎉", "موافق");
                await Shell.Current.GoToAsync("//HomePage");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
            {
                var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(result,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                string errorMessage = "يرجى تصحيح الأخطاء التالية:\n";
                if (errorResponse?.Errors != null)
                {
                    foreach (var error in errorResponse.Errors)
                    {
                        string fieldName = GetArabicFieldName(error.Key);
                        errorMessage += $"• {fieldName}: {string.Join(", ", error.Value)}\n";
                    }
                }
                else
                {
                    errorMessage = errorResponse?.Message ?? "حدث خطأ في إنشاء الحساب";
                }
                await DisplayAlert("خطأ في إنشاء الحساب", errorMessage, "موافق");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                await DisplayAlert("خطأ في إنشاء الحساب", "البريد الإلكتروني مستخدم بالفعل", "موافق");
            }
            else
            {
                await DisplayAlert("خطأ في الاتصال",
                    $"حدث خطأ في الاتصال بالخادم (رمز الخطأ: {(int)response.StatusCode})", "موافق");
            }
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("خطأ في الاتصال", "تعذر الاتصال بالخادم. حاول مرة أخرى", "موافق");
        }
        catch (TaskCanceledException)
        {
            await DisplayAlert("خطأ في الاتصال", "انتهت مهلة الاتصال. حاول لاحقاً", "موافق");
        }
        catch (Exception)
        {
            await DisplayAlert("خطأ غير متوقع", "حدث خطأ غير متوقع. حاول مرة أخرى", "موافق");
        }
    }

    private string GetArabicFieldName(string fieldName)
    {
        return fieldName.ToLower() switch
        {
            "email" => "البريد الإلكتروني",
            "password" => "كلمة المرور",
            "name" => "الاسم",
            "password_confirmation" => "تأكيد كلمة المرور",
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
