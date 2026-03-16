# Ïáíá ÅÕáÇÍ ãÔßáÉ ÊÛííÑ ßáãÉ ÇáãÑæÑ - Change Password Fix Guide

## ?? ÇáãÔßáÉ ÇáÃÓÇÓíÉ
**ÇáÎØÃ:** `UnprocessableEntity (422)`
- åĞÇ ÇáÎØÃ íÚäí Ãä ÇáÈíÇäÇÊ ÇáãÑÓáÉ áÇ ÊÊØÇÈŞ ÊãÇãÇğ ãÚ ãÊØáÈÇÊ API

## ? ÇáÍá ÇáãØÈŞ

### 1. **ÊÍÏíË Endpoint**
```csharp
// ? ÇáŞÏíã
"https://test.center-yazan.com/api/auth/reset-password"

// ? ÇáÌÏíÏ
"https://test.center-yazan.com/api/auth/change-password"
```

### 2. **ÕíÛÉ ÇáÈíÇäÇÊ ÇáãÑÓáÉ**
```json
{
  "current_password": "CurrentPassword123!",
  "password": "NewPassword123!",
  "password_confirmation": "NewPassword123!"
}
```

### 3. **ãÚÇáÌÉ ÇáÃÎØÇÁ ÇáãÍÓøäÉ**

| ÇáÎØÃ | ÇáßæÏ | ÇáÍá |
|------|------|-----|
| ßáãÉ ãÑæÑ ÛíÑ ÕÍíÍÉ | 401 (Unauthorized) | ÚÑÖ ÑÓÇáÉ: "ßáãÉ ÇáãÑæÑ ÇáÍÇáíÉ ÛíÑ ÕÍíÍÉ" |
| ÈíÇäÇÊ ÛíÑ ÕÍíÍÉ | 422 (Unprocessable Entity) | ÇÓÊÎÑÇÌ ÑÓÇÆá ÇáÎØÃ ÇáÊİÕíáíÉ ãä API |
| ØáÈ ÓíÁ | 400 (Bad Request) | ÇáÊÍŞŞ ãä ÕíÛÉ ÇáÈíÇäÇÊ |
| äÌÇÍ | 200 | ÊäÙíİ ÇáÍŞæá æÚÑÖ ÑÓÇáÉ ÇáäÌÇÍ |

### 4. **ÇáÊÍŞŞ ãä ÇáÈíÇäÇÊ ŞÈá ÇáÅÑÓÇá**

```csharp
// ÇáÊÍŞŞ ãä ÇáÍÏ ÇáÃÏäì áØæá ßáãÉ ÇáãÑæÑ
if (password.Length < 8)
{
    await Toast.Make("ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ íÌÈ Ãä Êßæä 8 ÃÍÑİ Úáì ÇáÃŞá").Show();
    return;
}

// ÇáÊÍŞŞ ãä ÚÏã ÊØÇÈŞ ßáãÇÊ ÇáãÑæÑ
if (password != confirmPassword)
{
    await Toast.Make("ßáãÇÊ ÇáãÑæÑ ÇáÌÏíÏÉ ÛíÑ ãÊØÇÈŞÉ").Show();
    return;
}

// ÇáÊÍŞŞ ãä ÇÎÊáÇİ ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ Úä ÇáÍÇáíÉ
if (password == CurrentPassword)
{
    await Toast.Make("ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ íÌÈ Ãä Êßæä ãÎÊáİÉ Úä ÇáÍÇáíÉ").Show();
    return;
}
```

### 5. **ÊÓÌíá ÇáÊİÇÕíá (Logging)**

```csharp
Console.WriteLine("?? Sending password change request:");
Console.WriteLine($"   Current Password: ***");
Console.WriteLine($"   New Password: ***");
Console.WriteLine($"   Confirmation: ***");
Console.WriteLine($"   Password Length: {password.Length}");

Console.WriteLine($"?? JSON Payload: {json}");
Console.WriteLine($"?? Response Status: {response.StatusCode}");
Console.WriteLine($"?? Response Body: {responseBody}");
```

## ?? ÇáãáİÇÊ ÇáãõÍÏøËÉ

### 1. **loukupm/ViewModel/AppViweModel.cs**
- ? ÊÍÏíË `ChangeUserPasswordAsync()` method
- ? ÅÖÇİÉ ãÚÇáÌÉ ÔÇãáÉ ááÃÎØÇÁ
- ? ÅÖÇİÉ ÇÓÊíÑÇÏ `Auth.ErrorResponse`
- ? ÊÍÓíä ÇáÊÍŞŞ ãä ÇáÈíÇäÇÊ

### 2. **loukupm/View/EditePasswordPage.xaml**
- ? ÑÈØ ÇáÍŞæá ÈÜ Bindings ÇáÕÍíÍÉ:
  - `CurrentPassword` ? ßáãÉ ÇáãÑæÑ ÇáÍÇáíÉ
  - `Password` ? ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ
  - `ConfirmPassword` ? ÊÃßíÏ ÇáãÑæÑ ÇáÌÏíÏÉ
- ? ÇÓÊÎÏÇã Localization (ÊÑÌãÉ)

### 3. **loukupm/View/EditePasswordPage.xaml.cs**
- ? ÑÈØ ÇáÕİÍÉ ÈÜ `AppViewModel` instance

## ?? ãÊØáÈÇÊ ßáãÉ ÇáãÑæÑ

ÍÓÈ ãÚÙã APIs ÇáÂãäÉ¡ ßáãÉ ÇáãÑæÑ íÌÈ Ãä:
- ? Êßæä 8 ÃÍÑİ Úáì ÇáÃŞá
- ? ÊÍÊæí Úáì ÃÍÑİ ßÈíÑÉ
- ? ÊÍÊæí Úáì ÃÍÑİ ÕÛíÑÉ
- ? ÊÍÊæí Úáì ÃÑŞÇã
- ? ÊÍÊæí Úáì ÃÍÑİ ÎÇÕÉ (Optional)

## ?? ÎØæÇÊ ÇáÇÎÊÈÇÑ

### 1. **ÇÎÊÈÇÑ ÕÍíÍ**
```
ßáãÉ ÇáãÑæÑ ÇáÍÇáíÉ: Password123!
ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ: NewPassword456!
ÊÃßíÏ ÇáãÑæÑ ÇáÌÏíÏÉ: NewPassword456!
?
ÇáäÊíÌÉ: ? Êã ÊÛííÑ ßáãÉ ÇáãÑæÑ ÈäÌÇÍ
```

### 2. **ÇÎÊÈÇÑ ÎØÃ - ÚÏã ÊØÇÈŞ**
```
ßáãÉ ÇáãÑæÑ ÇáÍÇáíÉ: Password123!
ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ: NewPassword456!
ÊÃßíÏ ÇáãÑæÑ ÇáÌÏíÏÉ: Different789!
?
ÇáäÊíÌÉ: ? ßáãÇÊ ÇáãÑæÑ ÇáÌÏíÏÉ ÛíÑ ãÊØÇÈŞÉ
```

### 3. **ÇÎÊÈÇÑ ÎØÃ - ßáãÉ ãÑæÑ ÍÇáíÉ ÎÇØÆÉ**
```
ßáãÉ ÇáãÑæÑ ÇáÍÇáíÉ: WrongPassword!
ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ: NewPassword456!
ÊÃßíÏ ÇáãÑæÑ ÇáÌÏíÏÉ: NewPassword456!
?
ÇáäÊíÌÉ: ? ßáãÉ ÇáãÑæÑ ÇáÍÇáíÉ ÛíÑ ÕÍíÍÉ [401]
```

## ?? ãÚáæãÇÊ ÇáÃãÇä

?? **ãáÇÍÙÇÊ ÃãÇä ãåãÉ:**

1. **áÇ ÊÍİÙ ßáãÇÊ ÇáãÑæÑ**
   - äÍä äÓÊÎÏã ÍŞæá `string` ãÄŞÊÉ
   - íÊã ãÓÍåÇ ÈÚÏ ÇáÅÑÓÇá ÇáäÇÌÍ

2. **ÇÓÊÎÏÇã HTTPS İŞØ**
   - ÇáÊØÈíŞ íÓÊÎÏã `https://` ÈÏáÇğ ãä `http://`

3. **ÅÒÇáÉ ÇáÈíÇäÇÊ ÇáÍÓÇÓÉ ãä ÇáÓÌáÇÊ**
   - äØÈÚ `***` ÈÏáÇğ ãä ßáãÇÊ ÇáãÑæÑ ÇáİÚáíÉ

4. **ÇáÊÍŞŞ ãä ÕÍÉ ÇáÜ Token**
   - íÊã ÇáÊÍŞŞ ãä æÌæÏ Token ŞÈá ÇáÅÑÓÇá
   - íÊã ÅÖÇİÉ Authorization Header

## ?? ÇáÏÚã Çáİäí

ÅĞÇ ÇÓÊãÑÊ ÇáãÔßáÉ:

1. **ÊÍŞŞ ãä ÇáÜ API Endpoint**
   - åá ÇáÜ URL ÕÍíÍ¿
   - åá ÇáÜ Method ÕÍíÍ (POST)¿

2. **ÊÍŞŞ ãä ãÊØáÈÇÊ ßáãÉ ÇáãÑæÑ**
   - åá ÊÍÊæí Úáì 8 ÃÍÑİ Úáì ÇáÃŞá¿
   - åá ÊÍÊæí Úáì ÃÍÑİ æÃÑŞÇã¿

3. **ÊÍŞŞ ãä ÇáÜ Token**
   - åá Token ÕÍíÍ¿
   - åá Token ãäÊåí ÇáÕáÇÍíÉ¿

4. **ÇÈÍË İí ÇáÓÌáÇÊ**
   - ÇÚÑÖ ÃÎÑÌ Console ááÊİÇÕíá ÇáßÇãáÉ
   - ÇÈÍË Úä ÑÓÇáÉ ÇáÎØÃ ãä API

## ? ÇáãíÒÇÊ ÇáÅÖÇİíÉ

- ? ÊÓÌíá ßÇãá ááÃÎØÇÁ
- ? ÑÓÇÆá ÎØÃ ãÍÏÏÉ ááãÓÊÎÏã
- ? ãÚÇáÌÉ ÌãíÚ ÍÇáÇÊ ÇáÎØÃ ÇáããßäÉ
- ? ÊäÙíİ ÇáÈíÇäÇÊ ÈÚÏ ÇáäÌÇÍ
- ? ÏÚã ÇáÊÑÌãÉ (Localization)

---
**ÂÎÑ ÊÍÏíË:** 2024
**ÇáÍÇáÉ:** ? äÔØ æÂãä
