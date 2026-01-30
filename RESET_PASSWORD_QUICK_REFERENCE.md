# ? ãáÎÕ ÓÑíÚ - ÊÍÏíË ßáãÉ ÇáãÑæÑ

## ? ãÇ Êã ÅäÌÇÒå:

### **EditPasswordVerification.xaml.cs** ?

```csharp
// 1. ãÚÇáÌ ÇáÒÑ
private async void Button_Clicked(object sender, EventArgs e)
{
    // ãäÚ ÇáäŞÑÇÊ ÇáãÊßÑÑÉ
    // ÇáÊÍŞŞ ãä ÇáÈíÇäÇÊ
    // ÅÑÓÇá ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ
    // ÇáÇäÊŞÇá ÚäÏ ÇáäÌÇÍ
}

// 2. ÅÑÓÇá ßáãÉ ÇáãÑæÑ
private async Task<bool> ResetPasswordAsync(AppViewModel viewModel)
{
    var payload = new 
    { 
        email = viewModel.Email,
        password = viewModel.NewPassword,
        password_confirmation = viewModel.ConfirmPassword
    };
    
    var response = await client.PostAsync(
        "https://test.center-yazan.com/api/auth/reset-password",
        content
    );
    
    return response.IsSuccessStatusCode;
}
```

---

## ?? ÇáÊÍŞŞ ãä ÇáÈíÇäÇÊ:

| ÇáãÊØáÈ | ÇáİÍÕ |
|--------|-------|
| **ßáãÉ ÌÏíÏÉ** | ? ãæÌæÏÉ |
| **ÊÃßíÏ** | ? ãæÌæÏ |
| **ÇáÊØÇÈŞ** | ? ãÊØÇÈŞÉ |
| **ÇáØæá** | ? 6+ ÃÍÑİ |
| **ÇáÅäÊÑäÊ** | ? ãÊÕá |

---

## ?? ÇáÍÇáÇÊ:

| ÇáÓíäÇÑíæ | ÇáäÊíÌÉ |
|---------|--------|
| ÕÍíÍ | ? ÇäÊŞá |
| ÛíÑ ãÊØÇÈŞ | ? ÎØÃ |
| ŞÕíÑ ÌÏÇğ | ? ÎØÃ |
| ÍŞá İÇÑÛ | ? ÎØÃ |
| ÈÏæä ÅäÊÑäÊ | ? ÎØÃ |
| ÎØÃ ÎÇÏã | ? ÎØÃ |

---

## ?? ÇáÅÌÑÇÁ:

```
? ÕÍíÍ ? ChackoutPage
? ÎØÃ ? ÑÓÇáÉ ÎØÃ
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
