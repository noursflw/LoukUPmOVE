# ?? ����� OTP �� Verificationpage

## ?? �� ��� ���� �� Verificationpage:

����� ����� �������� �� RestPassword ��� Verificationpage� ��� ��:

### 1. ������� ������ ����������
```csharp
// �� Verificationpage.xaml.cs
private string _userEmail;

protected override void OnAppearing()
{
    base.OnAppearing();
    
    // ������ ��� ������ �� ViewModel
    var viewModel = BindingContext as AppViewModel;
    _userEmail = viewModel.Email; // ������ ��� ������ ������
}
```

### 2. ������ �� ��� OTP
```csharp
// ��� ����� ����� ������
private async Task VerifyOtpAsync(string code)
{
    try
    {
        using var client = new HttpClient();
        
        var payload = new 
        { 
            email = _userEmail,      // ������ ������ ������
            code = code              // ����� ������ �� ������
        };
        
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await client.PostAsync(
            "https://lookupfriseur.com/api/auth/verify-otp",
            content
        );
        
        if (response.IsSuccessStatusCode)
        {
            // ? OTP ���� - �������� ������ ����� ���� ������
            await Navigation.PushAsync(new ResetPasswordPage(_userEmail));
        }
        else
        {
            // ? OTP ��� ����
            await DisplayAlert("���", "����� ��� ����", "�����");
        }
    }
    catch (Exception ex)
    {
        await DisplayAlert("���", "��� ������", "�����");
    }
}
```

### 3. ��� ������ - ����� ����� ���� ������
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
                "https://lookupfriseur.com/api/auth/reset-password",
                content
            );
            
            if (response.IsSuccessStatusCode)
            {
                // ? �� ����� ���� ������ �����
                await DisplayAlert("����", "�� ����� ���� ������", "�����");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                await DisplayAlert("���", "��� ����� ���� ������", "�����");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("���", "��� ���", "�����");
        }
    }
}
```

---

## ?? ����� ������� �������:

```
1?? RestPassword
   ?
   �������� ���� ������ ����������
   ?
   ����� OTP ��� ������
   ?
   ? ������ ? ��������

2?? Verificationpage
   ?
   �������� ���� ��� OTP �� ������
   ?
   ������ �� ����� ��� API
   ?
   ? ���� ? ��������

3?? ResetPasswordPage
   ?
   �������� ���� ���� ������ �������
   ?
   ��� ���� ������ �������
   ?
   ? ������ ? ������ ������ ������
```

---

## ?? ������ �� API:

### **1. ��� OTP**
```
POST /api/auth/request-otp
Body: { "email": "user@example.com" }
Response: 200 OK
```

### **2. ������ �� OTP**
```
POST /api/auth/verify-otp
Body: { "email": "user@example.com", "code": "123456" }
Response: 200 OK
```

### **3. ����� ����� ���� ������**
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

## ? ���� ����:

? ���� ������ �� ����� ��� ��������
? ������ ��� ������ �� ���� �������
? ���� �� ������� �� �� ����
? ���� ����� ��� �����
? ��� �������� �������� ��� �����

---

## ?? ������:

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
