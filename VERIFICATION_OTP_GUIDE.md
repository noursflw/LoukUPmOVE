# ?? Ïáíá ÇáÊÍŞŞ ãä OTP - Verification Page

## ? ãÇ Êã ÅäÌÇÒå:

### **ÊÍÏíË Verificationpage.xaml.cs** ?

Êã ÅÖÇİÉ äÙÇã ßÇãá ááÊÍŞŞ ãä ÑãÒ OTP:

1. **? ÅÑÓÇá ÇáÑãÒ ááÊÍŞŞ**
   - Endpoint: `POST /api/auth/verify-otp`
   - ÇáÈíÇäÇÊ: `{ "email": "...", "code": "1234" }`

2. **? ãÚÇáÌÉ ÇáÇÓÊÌÇÈÉ**
   - ÇáäÌÇÍ: ÇáÇäÊŞÇá áÜ `EditPasswordVerification`
   - ÇáİÔá: ÚÑÖ ÑÓÇáÉ ÎØÃ

3. **? ãÚÇáÌÉ ÇáÃÎØÇÁ**
   - 400: ÇáÑãÒ ÛíÑ ÕÍíÍ
   - 404: ÇáÈÑíá ÛíÑ ãæÌæÏ
   - 429: ãÍÇæáÇÊ ßËíÑÉ
   - ÈÏæä ÅäÊÑäÊ

---

## ?? ÓíÑ ÇáÚãáíÉ:

```
ÇáãÓÊÎÏã íÏÎá ÇáÑãÒ ÇáÑÈÇÚí
         ?
ÇáÊÍŞŞ ãä ãáÁ ÌãíÚ ÇáÎÇäÇÊ
         ?
ÇáÊÍŞŞ ãä ÇáÇÊÕÇá ÈÇáÅäÊÑäÊ
         ?
ÅÑÓÇá ÇáÑãÒ + ÇáÈÑíá Åáì API
         ?
         åá ÕÍíÍ¿
        ?      ?
       äÚã      áÇ
        ?       ?
    äÌÇÍ    ÎØÃ
        ?       ?
ÇáÇäÊŞÇá  ãÓÍ ÇáÎÇäÇÊ
ááÊÍÑíÑ   æÅÙåÇÑ ÇáÎØÃ
```

---

## ?? ÇáßæÏ ÇáÑÆíÓí:

### **1. ãÚÇáÌ ÇáÒÑ:**
```csharp
private async void ConfirmCode_Clicked(object sender, EventArgs e)
{
    if (_isVerifying) return; // ãäÚ ÇáäŞÑÇÊ ÇáãÊßÑÑÉ

    // ÇáÊÍŞŞ ãä ãáÁ ÌãíÚ ÇáÎÇäÇÊ
    if (fields.Any(f => string.IsNullOrWhiteSpace(f.Text)))
    {
        HighlightEmptyFields();
        return;
    }

    // ÈäÇÁ ÇáÑãÒ
    string code = string.Concat(fields.Select(f => f.Text));

    // ÅÑÓÇá ááÊÍŞŞ
    bool isValid = await VerifyOtpAsync(code);

    if (isValid)
    {
        await Navigation.PushAsync(new EditPasswordVerification());
    }
    else
    {
        // ãÓÍ æÅÙåÇÑ ÇáÎØÃ
        foreach (var field in fields)
            field.Text = string.Empty;
    }
}
```

### **2. ÅÑÓÇá ÇáÑãÒ:**
```csharp
private async Task<bool> VerifyOtpAsync(string code)
{
    var payload = new
    {
        email = viewModel.Email,  // ãä ViewModel
        code = code                 // ÇáÑãÒ ÇáãÏÎá
    };

    var response = await client.PostAsync(
        "https://test.center-yazan.com/api/auth/verify-otp",
        content
    );

    if (response.IsSuccessStatusCode)
        return true;

    return false;
}
```

---

## ?? ÍÇáÇÊ ÇáÇÓÊÎÏÇã:

### **ÇáÓíäÇÑíæ 1: ÑãÒ ÕÍíÍ** ?
```
1. ÇáãÓÊÎÏã íÏÎá: 1234
2. ? ÇáÑãÒ ßÇãá
3. ? ÇáÅäÊÑäÊ ãÊÕá
4. ? ÅÑÓÇá ááÎÇÏã
5. ? 200 OK
6. ? ÇáÇäÊŞÇá ááÊÍÑíÑ
```

### **ÇáÓíäÇÑíæ 2: ÎÇäÇÊ İÇÑÛÉ** ?
```
1. ÇáãÓÊÎÏã íÏÎá: 12__
2. ? ÇáÎÇäÇÊ ÇáİÇÑÛÉ ÊÊÍæá áÍãÑÇÁ
3. ? ÑÓÇáÉ: "ÃÏÎá ÇáÑãÒ ÇáßÇãá"
```

### **ÇáÓíäÇÑíæ 3: ÑãÒ ÛíÑ ÕÍíÍ** ?
```
1. ÇáãÓÊÎÏã íÏÎá: 5678
2. ? ÇáÎÇäÇÊ ßÇãáÉ
3. ? ÇáÅäÊÑäÊ ãÊÕá
4. ? ÅÑÓÇá ááÎÇÏã
5. ? 400 Bad Request
6. ? ÇáÎÇäÇÊ ÊãÓÍ æÊÊÍæá áÍãÑÇÁ
7. ? ÑÓÇáÉ: "ÇáÑãÒ ÛíÑ ÕÍíÍ"
```

### **ÇáÓíäÇÑíæ 4: ÈÏæä ÅäÊÑäÊ** ?
```
1. ? áÇ íæÌÏ ÇÊÕÇá
2. ? ÑÓÇáÉ: "áÇ íæÌÏ ÇÊÕÇá"
```

### **ÇáÓíäÇÑíæ 5: ãÍÇæáÇÊ ßËíÑÉ** ?
```
1. ÇáãÓÊÎÏã íÍÇæá ãÑÇÊ ßËíÑÉ
2. ? 429 Too Many Requests
3. ? ÑÓÇáÉ: "ÍÇæáÊ ãÑÇÊ ßËíÑÉ¡ ÇäÊÙÑ"
```

---

## ?? ãÚÇáÌÉ ÇáÃÎØÇÁ:

| ÇáÍÇáÉ | HTTP Code | ÇáÑÓÇáÉ | ÇáÅÌÑÇÁ |
|--------|-----------|--------|--------|
| **ÑãÒ ÕÍíÍ** | 200 | äÌÇÍ | ÇäÊŞá |
| **ÑãÒ ÎÇØÆ** | 400 | ÛíÑ ÕÍíÍ | ÇãÓÍ |
| **ÈÑíá ÛíÑ ãæÌæÏ** | 404 | ÛíÑ ãæÌæÏ | ÑÓÇáÉ |
| **ãÍÇæáÇÊ ßËíÑÉ** | 429 | ÇäÊÙÑ | ÑÓÇáÉ |
| **ÎØÃ ÇáÎÇÏã** | 500 | ÎØÃ ÚÇã | ÑÓÇáÉ |
| **ÈÏæä ÅäÊÑäÊ** | N/A | ÈáÇ ÇÊÕÇá | ÑÓÇáÉ |

---

## ? ÇáãíÒÇÊ:

? **ãäÚ ÇáäŞÑÇÊ ÇáãÊßÑÑÉ**
```csharp
if (_isVerifying) return;
```

? **ÊãííÒ ÇáÎÇäÇÊ ÇáİÇÑÛÉ**
```csharp
f.BackgroundColor = Colors.Red;
```

? **ãÓÍ ÇáÎÇäÇÊ ÚäÏ ÇáÎØÃ**
```csharp
field.Text = string.Empty;
```

? **ÇáÊÑßíÒ ÇáÊáŞÇÆí**
```csharp
Digit1.Focus();
```

? **ãÚÇáÌÉ ÔÇãáÉ**
```csharp
try-catch-finally
```

---

## ??? ÇáÃãÇä:

? **ÇáÊÍŞŞ ãä ÇáÈÑíá**
```csharp
if (string.IsNullOrWhiteSpace(viewModel.Email))
```

? **ÇáÊÍŞŞ ãä ÇáÇÊÕÇá**
```csharp
if (Connectivity.NetworkAccess != NetworkAccess.Internet)
```

? **ãäÚ ÇáØáÈÇÊ ÇáãÊÚÏÏÉ**
```csharp
if (_isVerifying) return;
```

? **ãÚÇáÌÉ ÇáÇÓÊËäÇÁÇÊ**
```csharp
catch (Exception ex)
```

---

## ?? ÍÇáÇÊ ÇáÇÎÊÈÇÑ:

- [ ] ÑãÒ ßÇãá ÕÍíÍ ? ÇäÊŞá ?
- [ ] ÎÇäÇÊ İÇÑÛÉ ? ÊÍãíÑ ?
- [ ] ÑãÒ ÎÇØÆ ? ãÓÍ æÎØÃ ?
- [ ] ÈÏæä ÅäÊÑäÊ ? ÎØÃ ?
- [ ] ÇáäŞÑ ÇáãÊßÑÑ ? ãäÚ ?
- [ ] ÇáÇäÊŞÇá ? EditPasswordVerification ?

---

## ?? ÇáÅÍÕÇÆíÇÊ:

```
Lines Added:     100+
Functions:       2
Error Handling:  5 cases
Safety Checks:   4
Build Status:    ? Success
```

---

## ?? ÇáÍÇáÉ:

```
Build:         ? Success
XAML:          ? ãÍÏøË
C#:            ? ãÍÏøË
Testing:       ? ÌÇåÒ
Deployment:    ?? ÌÇåÒ
```

---

**ÇáäÊíÌÉ ÇáäåÇÆíÉ**: äÙÇã ÊÍŞŞ Âãä æßÇãá! ? ??
