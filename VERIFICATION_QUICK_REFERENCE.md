# ? ���� ���� - ������ �� OTP

## ? �� �� ������:

### **Verificationpage.xaml.cs** ?

```csharp
// 1. ����� ����
private async void ConfirmCode_Clicked(object sender, EventArgs e)
{
    // ��� ������� ��������
    if (_isVerifying) return;
    
    // ������ �� �������
    // ����� �����
    // ������� �� �������
}

// 2. ����� �����
private async Task<bool> VerifyOtpAsync(string code)
{
    var payload = new { email = viewModel.Email, code = code };
    var response = await client.PostAsync(
        "https://lookupfriseur.com/api/auth/verify-otp",
        content
    );
    
    return response.IsSuccessStatusCode;
}
```

---

## ?? �������:

| ��������� | ������� |
|---------|--------|
| ��� ���� | ? ����� |
| ����� ����� | ? ����� |
| ��� ���� | ? ��� |
| ���� ������ | ? ��� |
| ��� ����� | ? ��� |

---

## ?? ���������:

```
? ���� ? EditPasswordVerification
? ��� ? ��� + �����
```

---

## ? ������:

```
Build:       ? ���
Updates:     ? ����
Testing:     ? ����
Deployment:  ?? ����
```

---

**���� ��������� ������! ??**
