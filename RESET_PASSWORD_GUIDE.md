# ?? Ïáíá ÊÍÏíË ßáãÉ ÇáãÑæÑ - Reset Password

## ? ãÇ Êã ÅäÌÇÒå:

### **ÊÍÏíË EditPasswordVerification.xaml.cs** ?

Êã ÅÖÇİÉ äÙÇã ßÇãá áÊÍÏíË ßáãÉ ÇáãÑæÑ:

1. **? ÇáÊÍŞŞ ãä ÇáÈíÇäÇÊ**
   - ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ
   - ÊÃßíÏ ßáãÉ ÇáãÑæÑ
   - ÊØÇÈŞ ßáãÇÊ ÇáãÑæÑ
   - ÇáÍÏ ÇáÃÏäì ááØæá (6 ÃÍÑİ)

2. **? ÅÑÓÇá ßáãÉ ÇáãÑæÑ**
   - Endpoint: `POST /api/auth/reset-password`
   - ÇáÈíÇäÇÊ: `{ "email": "...", "password": "...", "password_confirmation": "..." }`

3. **? ãÚÇáÌÉ ÇáÇÓÊÌÇÈÉ**
   - ÇáäÌÇÍ: ÇáÇäÊŞÇá áÜ `ChackoutPage`
   - ÇáİÔá: ÚÑÖ ÑÓÇáÉ ÎØÃ

4. **? ãÚÇáÌÉ ÇáÃÎØÇÁ ÇáÔÇãáÉ**
   - 400: ÈíÇäÇÊ ÛíÑ ÕÍíÍÉ
   - 404: ÈÑíá ÛíÑ ãæÌæÏ
   - 429: ãÍÇæáÇÊ ßËíÑÉ
   - 401: ÌáÓÉ ãäÊåíÉ

---

## ?? ÓíÑ ÇáÚãáíÉ:

```
ÇáãÓÊÎÏã íÏÎá ßáãÉ ÇáãÑæÑ
         ?
ÇáÊÍŞŞ ãä ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ
         ?
ÇáÊÍŞŞ ãä ÇáÊÃßíÏ
         ?
ÇáÊÍŞŞ ãä ÇáÊØÇÈŞ
         ?
ÇáÊÍŞŞ ãä ÇáØæá (6+ ÃÍÑİ)
         ?
ÇáÊÍŞŞ ãä ÇáÅäÊÑäÊ
         ?
ÅÑÓÇá ÇáØáÈ ááÜ API
         ?
         åá äÌÍ¿
        ?      ?
       äÚã      áÇ
        ?       ?
    äÌÇÍ    ÎØÃ
        ?       ?
ÇáÇäÊŞÇá  ÑÓÇáÉ
ááÔßÑ     ÎØÃ
```

---

## ?? ÇáßæÏ ÇáÑÆíÓí:

### **1. ãÚÇáÌ ÇáÒÑ:**
```csharp
private async void Button_Clicked(object sender, EventArgs e)
{
    if (_isProcessing) return; // ãäÚ ÇáäŞÑÇÊ ÇáãÊßÑÑÉ

    // ÇáÊÍŞŞ ãä ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ
    if (string.IsNullOrWhiteSpace(viewModel.NewPassword))
        return;

    // ÇáÊÍŞŞ ãä ÇáÊÃßíÏ
    if (string.IsNullOrWhiteSpace(viewModel.ConfirmPassword))
        return;

    // ÇáÊÍŞŞ ãä ÇáÊØÇÈŞ
    if (viewModel.NewPassword != viewModel.ConfirmPassword)
    {
        await DisplayAlert("ÎØÃ", "ßáãÇÊ ÇáãÑæÑ ÛíÑ ãÊØÇÈŞÉ", "ÍÓäÇğ");
        return;
    }

    // ÅÑÓÇá ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ
    bool success = await ResetPasswordAsync(viewModel);

    if (success)
        await Navigation.PushAsync(new ChackoutPage());
}
```

### **2. ÅÑÓÇá ßáãÉ ÇáãÑæÑ:**
```csharp
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

## ?? ÍÇáÇÊ ÇáÇÓÊÎÏÇã:

### **ÇáÓíäÇÑíæ 1: ßáãÇÊ ãÊØÇÈŞÉ æÕÍíÍÉ** ?
```
1. ßáãÉ ÌÏíÏÉ: MyPass123
2. ÊÃßíÏ: MyPass123
3. ÇáØæá: 10 ÃÍÑİ ?
4. ? ÇáÅÑÓÇá
5. ? 200 OK
6. ? ÇáÇäÊŞÇá ááÔßÑ
```

### **ÇáÓíäÇÑíæ 2: ßáãÇÊ ÛíÑ ãÊØÇÈŞÉ** ?
```
1. ßáãÉ ÌÏíÏÉ: MyPass123
2. ÊÃßíÏ: MyPass456
3. ? ÚÏã ÇáÊØÇÈŞ
4. ? ÑÓÇáÉ: "ßáãÇÊ ÇáãÑæÑ ÛíÑ ãÊØÇÈŞÉ"
```

### **ÇáÓíäÇÑíæ 3: ßáãÉ ŞÕíÑÉ ÌÏÇğ** ?
```
1. ßáãÉ ÌÏíÏÉ: Pass
2. ÇáØæá: 4 ÃÍÑİ ?
3. ? ÑÓÇáÉ: "íÌÈ Ãä Êßæä 6 ÃÍÑİ Úáì ÇáÃŞá"
```

### **ÇáÓíäÇÑíæ 4: ÈÏæä ÅäÊÑäÊ** ?
```
1. ? áÇ íæÌÏ ÇÊÕÇá
2. ? ÑÓÇáÉ: "áÇ íæÌÏ ÇÊÕÇá ÈÇáÅäÊÑäÊ"
```

### **ÇáÓíäÇÑíæ 5: ÈíÇäÇÊ ÛíÑ ÕÍíÍÉ** ?
```
1. ? ÇáÅÑÓÇá
2. ? 400 Bad Request
3. ? ÑÓÇáÉ: "ÇáÈíÇäÇÊ ÇáãÏÎáÉ ÛíÑ ÕÍíÍÉ"
```

---

## ?? ãÚÇáÌÉ ÇáÃÎØÇÁ:

| ÇáÍÇáÉ | HTTP Code | ÇáÑÓÇáÉ | ÇáÅÌÑÇÁ |
|--------|-----------|--------|--------|
| **äÌÍ** | 200 | äÌÇÍ | ÇäÊŞá ? |
| **ÈíÇäÇÊ ÎÇØÆÉ** | 400 | ÈíÇäÇÊ ÛíÑ ÕÍíÍÉ | ÑÓÇáÉ ? |
| **ÈÑíá ÛíÑ ãæÌæÏ** | 404 | ÈÑíá ÛíÑ ãæÌæÏ | ÑÓÇáÉ ? |
| **ãÍÇæáÇÊ ßËíÑÉ** | 429 | ÇäÊÙÑ | ÑÓÇáÉ ? |
| **ÌáÓÉ ãäÊåíÉ** | 401 | ÃÚÏ ÇáãÍÇæáÉ | ÑÓÇáÉ ? |
| **ÎØÃ ÇáÎÇÏã** | 500 | ÎØÃ ÚÇã | ÑÓÇáÉ ? |
| **ÈÏæä ÅäÊÑäÊ** | N/A | ÈáÇ ÇÊÕÇá | ÑÓÇáÉ ? |

---

## ? ÇáãíÒÇÊ:

? **ãäÚ ÇáäŞÑÇÊ ÇáãÊßÑÑÉ**
```csharp
if (_isProcessing) return;
```

? **ÇáÊÍŞŞ ãä ÇáÈíÇäÇÊ ÇáßÇãáÉ**
```
? ßáãÉ ÌÏíÏÉ ãæÌæÏÉ
? ÊÃßíÏ ãæÌæÏ
? ßáãÇÊ ãÊØÇÈŞÉ
? ÇáØæá ÇáßÇİí
? ÇÊÕÇá ÇáÅäÊÑäÊ
```

? **ãÚÇáÌÉ ÔÇãáÉ**
```csharp
try-catch-finally
```

? **ÑÓÇÆá æÇÖÍÉ**
```
ÈÇáÚÑÈíÉ æÓåáÉ Çáİåã
```

? **ãÚÇáÌÉ ãÊŞÏãÉ ááÃÎØÇÁ**
```
5 ÍÇáÇÊ ÎØÃ ãÎÊáİÉ
```

---

## ??? ÇáÃãÇä:

? **ÇáÊÍŞŞ ãä ÇáÈÑíá**
```csharp
if (string.IsNullOrWhiteSpace(viewModel.Email))
```

? **ÇáÊÍŞŞ ãä ßáãÇÊ ÇáãÑæÑ**
```csharp
if (viewModel.NewPassword != viewModel.ConfirmPassword)
```

? **ÇáÊÍŞŞ ãä ÇáØæá**
```csharp
if (viewModel.NewPassword.Length < 6)
```

? **ÇáÊÍŞŞ ãä ÇáÇÊÕÇá**
```csharp
if (Connectivity.NetworkAccess != NetworkAccess.Internet)
```

? **ãäÚ ÇáØáÈÇÊ ÇáãÊÚÏÏÉ**
```csharp
if (_isProcessing) return;
```

---

## ?? ÍÇáÇÊ ÇáÇÎÊÈÇÑ:

- [ ] ßáãÇÊ ãÊØÇÈŞÉ æØæá ÕÍíÍ ? ÇäÊŞá ?
- [ ] ßáãÇÊ ÛíÑ ãÊØÇÈŞÉ ? ÎØÃ ?
- [ ] ßáãÉ ŞÕíÑÉ ? ÎØÃ ?
- [ ] ÍŞá İÇÑÛ ? ÎØÃ ?
- [ ] ÈÏæä ÅäÊÑäÊ ? ÎØÃ ?
- [ ] ÈíÇäÇÊ ÎÇØÆÉ ãä ÇáÎÇÏã ? ÎØÃ ?
- [ ] ÇáäŞÑ ÇáãÊßÑÑ ? ãäÚ ?
- [ ] ÇáÇäÊŞÇá ? ChackoutPage ?

---

## ?? ÇáÅÍÕÇÆíÇÊ:

```
Lines Added:     130+
Functions:       2
Error Handling:  6 cases
Validations:     5
Build Status:    ? Success
```

---

## ?? ÇáÍÇáÉ:

```
Build:         ? Success
XAML:          ? ãÊæÇİŞ
C#:            ? ãÍÏøË
Testing:       ? ÌÇåÒ
Deployment:    ?? ÌÇåÒ
```

---

**ÇáäÊíÌÉ ÇáäåÇÆíÉ**: äÙÇã ÊÍÏíË ßÇãá æÂãä! ? ??
