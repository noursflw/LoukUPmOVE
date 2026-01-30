# ?? ÊßÇãá OTP ãÚ Verificationpage

## ?? ãÇ íÌÈ Úãáå İí Verificationpage:

ÚäÏãÇ íäÊŞá ÇáãÓÊÎÏã ãä RestPassword Åáì Verificationpage¡ íÌÈ Ãä:

### 1. ÇÓÊŞÈÇá ÇáÈÑíÏ ÇáÅáßÊÑæäí
```csharp
// İí Verificationpage.xaml.cs
private string _userEmail;

protected override void OnAppearing()
{
    base.OnAppearing();
    
    // ÇáÍÕæá Úáì ÇáÈÑíá ãä ViewModel
    var viewModel = BindingContext as AppViewModel;
    _userEmail = viewModel.Email; // ÇÓÊÎÏã äİÓ ÇáÈÑíá ÇáãÑÓá
}
```

### 2. ÇáÊÍŞŞ ãä ÑãÒ OTP
```csharp
// ÚäÏ ÅÏÎÇá ÇáÑãÒ ÇáßÇãá
private async Task VerifyOtpAsync(string code)
{
    try
    {
        using var client = new HttpClient();
        
        var payload = new 
        { 
            email = _userEmail,      // ÇáÈÑíá ÇáãÏÎá ÓÇÈŞÇğ
            code = code              // ÇáÑãÒ ÇáãÏÎá İí ÇáÕİÍÉ
        };
        
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync(
            "https://test.center-yazan.com/api/auth/verify-otp",
            content
        );
        
        if (response.IsSuccessStatusCode)
        {
            // ? OTP ÕÍíÍ - ÇáÇäÊŞÇá áÅÚÇÏÉ ÊÚííä ßáãÉ ÇáãÑæÑ
            await Navigation.PushAsync(new ResetPasswordPage(_userEmail));
        }
        else
        {
            // ? OTP ÛíÑ ÕÍíÍ
            await DisplayAlert("ÎØÃ", "ÇáÑãÒ ÛíÑ ÕÍíÍ", "ÍÓäÇğ");
        }
    }
    catch (Exception ex)
    {
        await DisplayAlert("ÎØÃ", "İÔá ÇáÊÍŞŞ", "ÍÓäÇğ");
    }
}
```

### 3. ÈÚÏ ÇáÊÍŞŞ - ÅÚÇÏÉ ÊÚííä ßáãÉ ÇáãÑæÑ
```csharp
// ResetPasswordPage.xaml.cs
public partial class ResetPasswordPage : ContentPage
{
    private string _email;
    
    public ResetPasswordPage(string email)
    {
        InitializeComponent();
        _email = email;
    }
    
    private async Task ResetPasswordAsync(string newPassword)
    {
        try
        {
            using var client = new HttpClient();
            
            var payload = new
            {
                email = _email,
                password = newPassword,
                password_confirmation = newPassword
            };
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync(
                "https://test.center-yazan.com/api/auth/reset-password",
                content
            );
            
            if (response.IsSuccessStatusCode)
            {
                // ? Êã ÊÍÏíË ßáãÉ ÇáãÑæÑ ÈäÌÇÍ
                await DisplayAlert("äÌÇÍ", "Êã ÊÍÏíË ßáãÉ ÇáãÑæÑ", "ÍÓäÇğ");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                await DisplayAlert("ÎØÃ", "İÔá ÊÍÏíË ßáãÉ ÇáãÑæÑ", "ÍÓäÇğ");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("ÎØÃ", "ÍÏË ÎØÃ", "ÍÓäÇğ");
        }
    }
}
```

---

## ?? ÎØæÇÊ ÇáÚãáíÉ ÇáßÇãáÉ:

```
1?? RestPassword
   ?
   ÇáãÓÊÎÏã íÏÎá ÇáÈÑíÏ ÇáÅáßÊÑæäí
   ?
   ÅÑÓÇá OTP Åáì ÇáÎÇÏã
   ?
   ? ÇáäÌÇÍ ? ÇáÇäÊŞÇá

2?? Verificationpage
   ?
   ÇáãÓÊÎÏã íÏÎá ÑãÒ OTP ãä ÇáÈÑíÏ
   ?
   ÇáÊÍŞŞ ãä ÇáÑãÒ ÚÈÑ API
   ?
   ? ÕÍíÍ ? ÇáÇäÊŞÇá

3?? ResetPasswordPage
   ?
   ÇáãÓÊÎÏã íÏÎá ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ
   ?
   ÍİÙ ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ
   ?
   ? ÇáäÌÇÍ ? ÇáÚæÏÉ áÊÓÌíá ÇáÏÎæá
```

---

## ?? ãÚÇííÑ Çá API:

### **1. ØáÈ OTP**
```
POST /api/auth/request-otp
Body: { "email": "user@example.com" }
Response: 200 OK
```

### **2. ÇáÊÍŞŞ ãä OTP**
```
POST /api/auth/verify-otp
Body: { "email": "user@example.com", "code": "123456" }
Response: 200 OK
```

### **3. ÅÚÇÏÉ ÊÚííä ßáãÉ ÇáãÑæÑ**
```
POST /api/auth/reset-password
Body: { 
  "email": "user@example.com",
  "password": "newpassword",
  "password_confirmation": "newpassword"
}
Response: 200 OK
```

---

## ? äŞÇØ ãåãÉ:

? ÇÍİÙ ÇáÈÑíá İí ãÊÛíÑ ÚäÏ ÇáÇäÊŞÇá
? ÇÓÊÎÏã äİÓ ÇáÈÑíá İí ÌãíÚ ÇáØáÈÇÊ
? ÊÍŞŞ ãä ÇáÇÊÕÇá İí ßá ÎØæÉ
? ÚÑøİ ÑÓÇÆá ÎØÃ æÇÖÍÉ
? ÃÚÏ ÇáãÍÇæáÉ ÊáŞÇÆíÇğ ÚäÏ ÇáİÔá

---

## ?? ÇáãáÎÕ:

```
RestPassword ??? OTP Request ??? Verificationpage
                                      ?
                              Verify OTP Code
                                      ?
                              ResetPasswordPage
                                      ?
                              Reset Password
                                      ?
                              LoginPage
```
