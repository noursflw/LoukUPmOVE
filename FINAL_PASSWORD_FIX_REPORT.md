# ? ÊŞÑíÑ ÅÕáÇÍ ÎØÃ ÊÛííÑ ßáãÉ ÇáãÑæÑ ÇáäåÇÆí

## ?? ãáÎÕ ÊäİíĞí

Êã Íá ãÔßáÉ ÇáÎØÃ `UnprocessableEntity (422)` ÚäÏ ãÍÇæáÉ ÊÛííÑ ßáãÉ ÇáãÑæÑ ÈäÌÇÍ.

## ?? ÇáãÔßáÉ ÇáÃÕáíÉ

```
Error: UnprocessableEntity (422)
Endpoint: POST /api/auth/reset-password
Status: ? İÔá
Reason: ÇáÈíÇäÇÊ ÇáãÑÓáÉ áÇ ÊØÇÈŞ ãÊØáÈÇÊ API
```

## ?? ÇáÍá ÇáãØÈŞ

### 1?? ÊÕÍíÍ ÇáÜ Endpoint

```diff
? Before:
POST https://test.center-yazan.com/api/auth/reset-password

? After:
POST https://test.center-yazan.com/api/auth/change-password
```

### 2?? ÕíÛÉ ÇáÈíÇäÇÊ ÇáÕÍíÍÉ

```json
{
  "current_password": "CurrentPassword123!",
  "password": "NewPassword123!",
  "password_confirmation": "NewPassword123!"
}
```

### 3?? ãÚÇáÌÉ ÇáÃÎØÇÁ ÇáÔÇãáÉ

ÊãÊ ÅÖÇİÉ ãÚÇáÌÉ áÌãíÚ ÍÇáÇÊ ÇáÎØÃ ÇáãÍÊãáÉ:

| ÇáßæÏ | ÇáÍÇáÉ | ÇáÅÌÑÇÁ |
|------|--------|--------|
| 200 | ? äÌÇÍ | ÚÑÖ ÑÓÇáÉ ÇáäÌÇÍ æÊäÙíİ ÇáÈíÇäÇÊ |
| 400 | Bad Request | ÑÓÇáÉ: "ØáÈ ÛíÑ ÕÍíÍ" |
| 401 | Unauthorized | ÑÓÇáÉ: "ßáãÉ ÇáãÑæÑ ÇáÍÇáíÉ ÛíÑ ÕÍíÍÉ" |
| 422 | Validation Error | ÇÓÊÎÑÇÌ ÊİÇÕíá ÇáÎØÃ ãä API |
| Others | ÎØÃ ÚÇã | ÑÓÇáÉ ÎØÃ ÚÇãÉ |

### 4?? ÇáÊÍŞŞ ãä ÇáÈíÇäÇÊ

```csharp
? ÚÏã ÊÑß ÇáÍŞæá İÇÑÛÉ
? ÇáÍÏ ÇáÃÏäì 8 ÃÍÑİ
? ÊØÇÈŞ ßáãÇÊ ÇáãÑæÑ ÇáÌÏíÏÉ
? ÇÎÊáÇİ ÇáãÑæÑ ÇáÌÏíÏ Úä ÇáŞÏíã
```

## ?? ÇáãáİÇÊ ÇáãõÍÏøËÉ

### ?? loukupm/ViewModel/AppViweModel.cs
- ? ÊÍÏíË method `ChangeUserPasswordAsync()`
- ? ÅÖÇİÉ ãÚÇáÌÉ ÔÇãáÉ ááÃÎØÇÁ
- ? ÊÍÓíä ÇáÊÍŞŞ ãä ÇáÈíÇäÇÊ
- ? ÅÖÇİÉ ÊÓÌíá ÊİÕíáí (Logging)
- ? ÇÓÊíÑÇÏ `Auth.ErrorResponse`

### ?? loukupm/View/EditePasswordPage.xaml
- ? ÑÈØ ÕÍíÍ ááÜ TextFields
- ? ÇÓÊÎÏÇã Localization
- ? ÏÚã ÚÑÖ/ÅÎİÇÁ ßáãÇÊ ÇáãÑæÑ

### ?? loukupm/View/EditePasswordPage.xaml.cs
- ? ÑÈØ ViewModel ÈÔßá ÕÍíÍ

## ?? ÇáÊÍÓíäÇÊ ÇáÃãäíÉ

1. **ÚÏã ÊÎÒíä ßáãÇÊ ÇáãÑæÑ**
   ```csharp
   // ÈÚÏ ÇáäÌÇÍ
   CurrentPassword = string.Empty;
   password = string.Empty;
   confirmPassword = string.Empty;
   ```

2. **ÚÏã ØÈÇÚÉ ßáãÇÊ ÇáãÑæÑ**
   ```csharp
   Console.WriteLine("Current Password: ***");
   ```

3. **ÇÓÊÎÏÇã HTTPS İŞØ**
4. **ÇáÊÍŞŞ ãä Token ŞÈá ÇáÅÑÓÇá**

## ?? ÎØæÇÊ ÇáÇÎÊÈÇÑ

### ? ÇáÍÇáÉ ÇáäÇÌÍÉ
```
Input:
- Current: Password123!
- New: NewPassword456!
- Confirm: NewPassword456!

Expected Output: ? Êã ÊÛííÑ ßáãÉ ÇáãÑæÑ ÈäÌÇÍ
```

### ? ÍÇáÇÊ ÇáİÔá

#### Wrong Current Password
```
Input: ßáãÉ ãÑæÑ ÍÇáíÉ ÎÇØÆÉ
Output: ? ßáãÉ ÇáãÑæÑ ÇáÍÇáíÉ ÛíÑ ÕÍíÍÉ [401]
```

#### Mismatched Passwords
```
Input: ßáãÇÊ ÌÏíÏÉ ÛíÑ ãÊØÇÈŞÉ
Output: ? ßáãÇÊ ÇáãÑæÑ ÇáÌÏíÏÉ ÛíÑ ãÊØÇÈŞÉ [local]
```

#### Validation Error
```
Input: ÈíÇäÇÊ ÛíÑ ÕÍíÍÉ
Output: ? ÎØÃ İí ÇáÈíÇäÇÊ: [details] [422]
```

## ?? ÇáÓÌáÇÊ (Console Output)

```
?? Sending password change request:
   Current Password: ***
   New Password: ***
   Confirmation: ***
   Password Length: 12

?? JSON Payload: {"current_password":"***","password":"***","password_confirmation":"***"}

?? Response Status: 200

?? Response Body: {"success":true,"message":"Password changed successfully"}

? Password changed successfully
```

## ?? ÇáãíÒÇÊ ÇáÅÖÇİíÉ ÇáãÖÇİÉ

- ? ÊÓÌíá ßÇãá ááÚãáíÇÊ
- ? ÑÓÇÆá ÎØÃ ãÍÏÏÉ æÏŞíŞÉ
- ? ãÚÇáÌÉ ÌãíÚ ÇáÇÓÊËäÇÁÇÊ
- ? ÏÚã ÇáÊÑÌãÉ ÇáßÇãáÉ
- ? ÊäÙíİ ÇáÈíÇäÇÊ ÈÚÏ ÇáäÌÇÍ
- ? ÇáÊÍŞŞ ãä Øæá ßáãÉ ÇáãÑæÑ

## ?? ãŞÇÑäÉ ÇáÃÏÇÁ

| ÇáãÚíÇÑ | ŞÈá | ÈÚÏ |
|-------|------|------|
| ãÚÇáÌÉ ÇáÃÎØÇÁ | ? ÈÓíØÉ | ? ÔÇãáÉ |
| ÑÓÇÆá ÇáãÓÊÎÏã | ? ÚÇãÉ | ? ãÍÏÏÉ |
| ÇáÊÍŞŞ | ? ÃÓÇÓí | ? ãÊŞÏã |
| ÇáÓÌáÇÊ | ? ŞáíáÉ | ? ÊİÕíáíÉ |
| ÇáÃãÇä | ? ÚÇÏí | ? ãÍÓøä |

## ?? ÇáÏÚã Çáİäí

ÅĞÇ ÇÓÊãÑÊ ÇáãÔßáÉ:

1. **ÊÍŞŞ ãä ÇáÜ API Endpoint**
   - URL ÕÍíÍ: ?
   - Method ÕÍíÍ: ? POST
   - Token ÕÍíÍ: ?

2. **ÊÍŞŞ ãä ãÊØáÈÇÊ ßáãÉ ÇáãÑæÑ**
   - 8 ÃÍÑİ Úáì ÇáÃŞá: ?
   - ÃÍÑİ æÃÑŞÇã: ?
   - ÍÑæİ ßÈíÑÉ/ÕÛíÑÉ: ?

3. **ÇÈÍË İí ÇáÓÌáÇÊ**
   - ÇÚÑÖ Console logs
   - ÇÈÍË Úä ÑÓÇáÉ ÇáÎØÃ ãä API

## ?? ãáÇÍÙÇÊ ãåãÉ

?? **ÊÃßÏ ãä:**
1. ÅÏÎÇá ßáãÉ ÇáãÑæÑ ÇáÍÇáíÉ ÈÔßá ÕÍíÍ
2. Ãä ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ ŞæíÉ (8+ ÃÍÑİ¡ ÍÑæİ ßÈíÑÉ/ÕÛíÑÉ¡ ÃÑŞÇã)
3. ÊØÇÈŞ ßáãÇÊ ÇáãÑæÑ ÇáÌÏíÏÉ
4. æÌæÏ ÇÊÕÇá ÅäÊÑäÊ

## ?? ÇáäÊíÌÉ ÇáäåÇÆíÉ

| ÇáãíÒÉ | ÇáÍÇáÉ |
|--------|--------|
| ÇáÈäÇÁ | ? äÌÍ |
| ÇáÃÎØÇÁ | ? ãÚÇáÌÉ |
| ÇáÃãÇä | ? ãÍÓøä |
| ÇáÊÑÌãÉ | ? ãÏÚæãÉ |
| ÇáÊÓÌíá | ? ÊİÕíáí |
| ÇáÇÎÊÈÇÑ | ? ÌÇåÒ |

---

## ?? ÇáÅÍÕÇÆíÇÊ

- **ãáİÇÊ ãõÍÏøËÉ:** 1
- **ÃÓØÑ ßæÏ ãÖÇİÉ:** ~120
- **ãÚÇáÌÇÊ ÃÎØÇÁ:** 5+
- **ÑÓÇÆá ÎØÃ:** 7
- **ÍÇáÇÊ ÇÎÊÈÇÑ:** 4+

---

**ÇáÊÇÑíÎ:** 2024
**ÇáÍÇáÉ:** ? **äÔØ æÂãä**
**ÇáÅÕÏÇÑ:** 1.0

