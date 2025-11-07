
using CommunityToolkit.Maui.Views;
using loukupm.Model;
using loukupm.services;
using loukupm.View.MassgingApp;
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

    //private async void OnLoginClicked(object sender, EventArgs e)
    //{

    //    await Task.WhenAll(
    //      RegisterButton.RotateTo(10),
    //      RegisterButton.RotateTo(-10),
    //      RegisterButton.ScaleTo(0.5, 100, Easing.Linear),
    //      RegisterButton.ScaleTo(0, 150, Easing.CubicIn));
    //    // 🔸 تعطيل الزر وإظهار المؤشر
    //    RegisterButton.IsVisible = false;
    //    RegisterButton.Text = "";
    //    LoadingIndicator.IsVisible = true;
    //    LoadingIndicator.IsRunning = true;

    //    string email = EmailEntry.Text?.Trim();
    //    string password = PasswordEntry.Text;


    //    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    //    {
    //        var popup = new DisplayAlretCoustm();
    //        await this.ShowPopupAsync(popup);
    //        return;
    //    }

    //    if (!IsValidEmail(email))
    //    {
    //        var popup = new EroreInputEmaile();
    //        await this.ShowPopupAsync(popup);
    //        return;
    //    }

    //    if (Connectivity.NetworkAccess != NetworkAccess.Internet)
    //    {
    //        var popup = new NoEnternetConacted();
    //        await this.ShowPopupAsync(popup);
    //        return;
    //    }

    //    var loginData = new LoginRequest
    //    {
    //        Email = email,
    //        Password = password
    //    };

    //    try
    //    {


    //        var handler = new HttpClientHandler
    //        {
    //            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    //        };

    //        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };

    //        HttpResponseMessage response = null;
    //        string result = null;


    //        try
    //        {
    //            var json = JsonSerializer.Serialize(loginData);
    //            var content = new StringContent(json, Encoding.UTF8, "application/json");
    //            response = await client.PostAsync("https://test.center-yazan.com/api/auth/login", content);
    //            result = await response.Content.ReadAsStringAsync();


    //        }
    //        catch
    //        {

    //            if (response == null)
    //            {
    //                var popup = new NoServerResponse();
    //                await this.ShowPopupAsync(popup);
    //                return;
    //            }
    //        }

    //        if (response.IsSuccessStatusCode)
    //        {
    //            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(result,
    //                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    //            if (loginResponse?.User == null)
    //            {
    //                await DisplayAlert("خطأ في الاستجابة", "حدث خطأ في معالجة بيانات المستخدم", "موافق");
    //                return;
    //            }

    //            // ? تخزين التوكن بأمان
    //            if (!string.IsNullOrEmpty(loginResponse.Token))
    //                await SecureStorage.SetAsync("auth_token", loginResponse.Token);
    //            if (!string.IsNullOrEmpty(loginResponse.Refresh_Token))
    //                await SecureStorage.SetAsync("refresh_token", loginResponse.Refresh_Token);


    //            var popup = new CompletedLogin();
    //            await this.ShowPopupAsync(popup);
    //            await Shell.Current.GoToAsync("//HomePage");
    //        }
    //        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    //        {
    //              var popup = new NoEqaulData();
    //             await this.ShowPopupAsync(popup);
    //        }
    //        else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
    //        {
    //            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(result,
    //                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    //            string errorMessage = "يرجى تصحيح الأخطاء التالية:\n";
    //            if (errorResponse?.Errors != null)
    //            {
    //                foreach (var error in errorResponse.Errors)
    //                {
    //                    string fieldName = GetArabicFieldName(error.Key);
    //                    errorMessage += $"• {fieldName}: {string.Join(", ", error.Value)}\n";
    //                }
    //            }
    //            else
    //            {
    //                errorMessage = errorResponse?.Message ?? "حدث خطأ في تسجيل الدخول";
    //            }
    //            await DisplayAlert("خطأ في تسجيل الدخول", errorMessage, "موافق");
    //        }
    //        else
    //        {
    //            var popup = new NoServerResponse();
    //            await this.ShowPopupAsync(popup);
    //        }
    //    }
    //    catch (HttpRequestException)
    //    {
    //        var popup = new NoServerResponse();
    //        await this.ShowPopupAsync(popup);
    //    }
    //    catch (TaskCanceledException)
    //    {
    //        var popup = new NoServerResponse();
    //        await this.ShowPopupAsync(popup);
    //    }
    //    catch (Exception)
    //    {
    //        var popup = new NoServerResponse();
    //        await this.ShowPopupAsync(popup);
    //    }
    //    finally
    //    {
    //        // 🔸 إعادة الزر لحالته الطبيعية
    //        LoadingIndicator.IsRunning = false;
    //        LoadingIndicator.IsVisible = false;
    //        RegisterButton.Text = "إنشاء حساب"; // أو نفس الترجمة
    //        RegisterButton.IsVisible = true;
    //        RegisterButton.Scale = 0;
    //        await RegisterButton.RotateTo(0);
    //        await RegisterButton.ScaleTo(1.1, 150, Easing.CubicOut);
    //        await RegisterButton.ScaleTo(1.0, 100, Easing.Linear);
    //    }


    //}
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // ✅ منع الضغط المزدوج مباشرة
        if (!RegisterButton.IsEnabled)
            return;

        RegisterButton.IsEnabled = false;

        // 🎬 أنيميشن ضغط الزر + إخفاء ناعم
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

        try
        {
            // ✅ التحقق من الإدخالات بعد إخفاء الزر (حتى المستخدم ما يضغط مرتين)
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

                if (!string.IsNullOrEmpty(loginResponse?.Token))
                    await SecureStorage.SetAsync("auth_token", loginResponse.Token);

                if (!string.IsNullOrEmpty(loginResponse?.Refresh_Token))
                    await SecureStorage.SetAsync("refresh_token", loginResponse.Refresh_Token);

                var popup = new CompletedLogin();
                await this.ShowPopupAsync(popup);
                await Shell.Current.GoToAsync("//HomePage");
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
            // 🎯 إعادة الحالة الطبيعية دائمًا
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

            RegisterButton.IsEnabled = true; // ✅ تفعيل الزر من جديد
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

    // لإيقاف الحركة (عندما تغادر الصفحة أو تنتهي العملية)
    void StopGoogleButtonAnimation()
    {
        isGoogleButtonAnimating = false;
        GoogleButton.RotateTo(0, 300, Easing.CubicOut); // يرجع للوضع الطبيعي
    }

    // مثلاً تبدأ الأنيميشن في OnAppearing()
    protected override void OnAppearing()
    {
        base.OnAppearing();
        isGoogleButtonAnimating = true;
        StartGoogleButtonAnimation();
    }

    // وتوقفها في OnDisappearing()
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopGoogleButtonAnimation();
    }

}
