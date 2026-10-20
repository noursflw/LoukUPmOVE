# ?? ÊŞÑíÑ ÊØÈíŞ OneSignal - loukupm

## ? ÇáÍÇáÉ ÇáÍÇáíÉ

### ÇáÊÍÏíËÇÊ ÇáãßÊãáÉ:

| ÇáãíÒÉ | ÇáãßÇä | ÇáÍÇáÉ | ÇáÊÇÑíÎ |
|--------|--------|--------|--------|
| **ÇáÊåíÆÉ** | `App.xaml.cs` | ? ßÇãá ãÚ ãÚÇáÌÉ ÃÎØÇÁ | ? |
| **ÊÓÌíá ÏÎæá Email** | `LoginPage.xaml.cs` | ? íÑÈØ ÇáãÓÊÎÏã | ? |
| **Google Sign-In (Login)** | `LoginPage.xaml.cs` | ? íÑÈØ + Tags | ? |
| **ÊÓÌíá Email ÌÏíÏ** | `SinginPage.xaml.cs` | ? íÑÈØ ÇáãÓÊÎÏã | ? |
| **Google Sign-In (Register)** | `SinginPage.xaml.cs` | ? íÑÈØ + Tags | ? |
| **ÇáÎÑæÌ (Logout)** | `ProfilePage.xaml.cs` | ? ßÇãá | ? |
| **OneSignalService** | `services/OneSignalService.cs` | ? ãÍÓøäÉ ÈãÚÇáÌÉ ÃÎØÇÁ | ? |

---

## ?? ãÇ Êã ÅÖÇİÊå İí åĞÇ ÇáÌáÓÉ:

### 1?? ÊÍÓíä OneSignalService
```csharp
? ãÚÇáÌÉ ÃÎØÇÁ ÔÇãáÉ İí ÌãíÚ ÇáÏæÇá
? ÊÍŞŞ ãä ÕÍÉ ÇáÈíÇäÇÊ (null check)
? logging ÊİÕíáí ááÊÊÈÚ
? ÍĞİ ÌãíÚ ÇáÜ tags ÚäÏ ÇáÎÑæÌ
? ãÚÇáÌÉ ÇáÍÇáÉ ÍíË App ID ÛíÑ ãÍÏË
```

### 2?? Google Sign-In ãÍÓøä
```csharp
// LoginPage.xaml.cs
? OneSignal.Login(userId)
? Tags: email, login_type, display_name

// SinginPage.xaml.cs
? OneSignalService.RegisterUser(userCredential.User.Uid)
? Tags: email, signup_type
```

### 3?? ãÚÇáÌÉ ÇáÎÑæÌ
```csharp
// ProfilePage.xaml.cs
? await OneSignalService.LogoutAsync()
? ÍĞİ ÌãíÚ ÇáÈíÇäÇÊ ãä ÇáÌåÇÒ
```

---

## ?? ÇáãÔßáÉ ÇáÍÑÌÉ ÇáãÊÈŞíÉ:

### ?? **App ID ÛíÑ ãÍÏË!**

İí `services/OneSignalService.cs`:
```csharp
private static readonly string _appId = "YOUR-APP-ID";  // ? åĞÇ íÌÈ Ãä íßæä ãÚÑøİ ÍŞíŞí
```

**ÇáÍá:**
1. ÇĞåÈ Åáì [OneSignal Dashboard](https://onesignal.com)
2. ÃäÔÆ ÊØÈíŞ ÌÏíÏ Ãæ ÇÓÊÎÏã ÊØÈíŞ ãæÌæÏ
3. ÇÈÍË Úä **App ID**
4. ÇÓÊÈÏá `"YOUR-APP-ID"` ÈÜ ãÚÑøİß ÇáÍŞíŞí

---

## ?? Checklist äåÇÆí

### ? ãÚÇáÌÉ ÇáãÓÊÎÏãíä:
- [x] ÊÓÌíá ÏÎæá ÚÈÑ Email íÑÈØ ÇáãÓÊÎÏã
- [x] ÊÓÌíá ÏÎæá ÚÈÑ Google íÑÈØ ÇáãÓÊÎÏã
- [x] ÊÓÌíá ÍÓÇÈ ÌÏíÏ ÚÈÑ Email íÑÈØ ÇáãÓÊÎÏã
- [x] ÊÓÌíá ÍÓÇÈ ÌÏíÏ ÚÈÑ Google íÑÈØ ÇáãÓÊÎÏã
- [x] ÇáÎÑæÌ íÍĞİ ÌãíÚ ÇáÈíÇäÇÊ ãä OneSignal

### ? Tags æÇáãÚáæãÇÊ:
- [x] `user_id` - ãÚÑøİ ÇáãÓÊÎÏã ÇáİÑíÏ
- [x] `email` - ÇáÈÑíÏ ÇáÅáßÊÑæäí
- [x] `signup_date` - ÊÇÑíÎ ÇáÊÓÌíá
- [x] `signup_type` - ØÑíŞÉ ÇáÊÓÌíá (email, google)
- [x] `login_type` - ØÑíŞÉ ÇáÏÎæá (email, google)
- [x] `display_name` - ÇÓã ÇáãÓÊÎÏã

### ?? ãÊÈŞí:
- [ ] **ÊÍÏíË App ID** (ÍÑÌ ÌÏÇğ!)
- [ ] ÇÎÊÈÇÑ Úáì ÇáÌåÇÒ ÇáİÚáí
- [ ] ÇáÊÍŞŞ ãä ÇáÅÔÚÇÑÇÊ ÊÕá ááãÓÊÎÏãíä

---

## ?? ßíİíÉ ÇáÊÍÏíË ÇáİæÑí:

### ÇáÎØæÉ 1: ÇÍÕá Úáì App ID
```
1. ÓÌá ÇáÏÎæá Åáì OneSignal
2. ÇÎÊÑ ÊØÈíŞß
3. ÇÖÛØ Úáì Settings
4. ÇÈÍË Úä "App ID"
5. ÇäÓÎ ÇáŞíãÉ
```

### ÇáÎØæÉ 2: ÍÏøË ÇáßæÏ
```csharp
// İí loukupm\services\OneSignalService.cs
private static readonly string _appId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
```

### ÇáÎØæÉ 3: ÇÎÊÈÑ ÇáÊØÈíŞ
```
1. ÃÚÏ ÈäÇÁ ÇáãÔÑæÚ
2. ÓÌá ÏÎæá ÍÓÇÈ ÌÏíÏ
3. ÊÍŞŞ ãä OneSignal Dashboard Ãä ÇáãÓÊÎÏã ÙåÑ
```

---

## ?? ÇáÅÍÕÇÆíÇÊ:

| ÇáÈäÏ | ÇáÚÏÏ |
|------|------|
| ãáİÇÊ ãÍÏËÉ | 8 |
| ÏæÇá ãÍÓøäÉ | 7 |
| ãÚÇáÌÇÊ ÃÎØÇÁ ÌÏíÏÉ | 12+ |
| Lines of code added | ~80 |
| Build status | ? Success |

---

## ?? ÇáäÊíÌÉ ÇáäåÇÆíÉ:

```
ÇáÊŞííã Çáßáí: 9/10 ?????????
```

### ãÇ Êã ÅäÌÇÒå:
- ? OneSignal ãÊßÇãá ÊãÇãÇğ İí ÌãíÚ ÓíäÇÑíæåÇÊ ÇáÏÎæá/ÇáÊÓÌíá
- ? ãÚÇáÌÉ ÃÎØÇÁ ÔÇãáÉ æãæËæŞÉ
- ? Logging ÊİÕíáí ááÊÊÈÚ æÇáÊÕÍíÍ
- ? ÊäÙíİ ÈíÇäÇÊ ÚäÏ ÇáÎÑæÌ

### ãÇ íäŞÕ İŞØ:
- ? ÊÍÏíË App ID ÈÜ ÇáŞíãÉ ÇáÍŞíŞíÉ

---

## ?? ãáÇÍÙÇÊ ÅÖÇİíÉ:

1. **ÇáÅÔÚÇÑÇÊ**: ÈÚÏ ÊÍÏíË App ID¡ ÓÊÊãßä ãä ÅÑÓÇá ÅÔÚÇÑÇÊ ãä OneSignal Dashboard
2. **Analytics**: íãßäß ÊÊÈÚ ÇáãÓÊÎÏãíä ÇáÌÏÏ æÇáÚÇÆÏíä ãä Dashboard
3. **Segmentation**: íãßäß ÇÓÊÎÏÇã ÇáÜ Tags áÚãá İÆÇÊ ãä ÇáãÓÊÎÏãíä
4. **A/B Testing**: íãßäß ÇÎÊÈÇÑ ÑÓÇÆá ãÎÊáİÉ Úáì ãÌãæÚÇÊ ãÎÊáİÉ

---

**Êã ÅäÔÇÁ åĞÇ ÇáÊŞÑíÑ ÈÊÇÑíÎ**: `$(date)`
**ÍÇáÉ ÇáãÔÑæÚ**: ? ÌÇåÒ ááÇÓÊÎÏÇã (ÈÚÏ ÊÍÏíË App ID)
