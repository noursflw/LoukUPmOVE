# ? ãáÎÕ ÓÑíÚ - ÇáÊÍŞŞ ãä OTP

## ? ãÇ Êã ÅäÌÇÒå:

### **Verificationpage.xaml.cs** ?

```csharp
// 1. ãÚÇáÌ ÇáÒÑ
private async void ConfirmCode_Clicked(object sender, EventArgs e)
{
    // ãäÚ ÇáäŞÑÇÊ ÇáãÊßÑÑÉ
    if (_isVerifying) return;
    
    // ÇáÊÍŞŞ ãä ÇáÎÇäÇÊ
    // ÅÑÓÇá ÇáÑãÒ
    // ÇáÊÚÇãá ãÚ ÇáäÊíÌÉ
}

// 2. ÅÑÓÇá ÇáÑãÒ
private async Task<bool> VerifyOtpAsync(string code)
{
    var payload = new { email = viewModel.Email, code = code };
    var response = await client.PostAsync(
        "https://test.center-yazan.com/api/auth/verify-otp",
        content
    );
    
    return response.IsSuccessStatusCode;
}
```

---

## ?? ÇáÍÇáÇÊ:

| ÇáÓíäÇÑíæ | ÇáäÊíÌÉ |
|---------|--------|
| ÑãÒ ÕÍíÍ | ? ÇäÊŞá |
| ÎÇäÇÊ İÇÑÛÉ | ? ÊÍãíÑ |
| ÑãÒ ÎÇØÆ | ? ãÓÍ |
| ÈÏæä ÅäÊÑäÊ | ? ÎØÃ |
| äŞÑ ãÊßÑÑ | ? ãäÚ |

---

## ?? ÇáÅÌÑÇÁÇÊ:

```
? ÕÍíÍ ? EditPasswordVerification
? ÎØÃ ? ãÓÍ + ÑÓÇáÉ
```

---

## ? ÇáÍÇáÉ:

```
Build:       ? äÌÍ
Updates:     ? ßÇãá
Testing:     ? ÌÇåÒ
Deployment:  ?? ÌÇåÒ
```

---

**ÌÇåÒ ááÇÓÊÎÏÇã ÇáİæÑí! ??**
