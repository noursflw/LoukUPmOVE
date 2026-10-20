# ?? OneSignal - ÊÚáíãÇÊ ÇáÊØÈíŞ ÇáÓÑíÚÉ

## 1?? ÇáÎØæÉ ÇáÃæáì æÇáÃåã: ÊÍÏíË App ID

### ?? **ÇáãÔßáÉ:**
İí Çáãáİ `loukupm\services\OneSignalService.cs`¡ ÇáÜ App ID ãÇ ÒÇá:
```csharp
private static readonly string _appId = "YOUR-APP-ID";
```

### ? **ÇáÍá:**

#### Ã) ÇÍÕá Úáì App ID ãä OneSignal:
```
1. ÇĞåÈ Åáì https://onesignal.com
2. ÇÖÛØ "Log In"
3. ÇÏÎá ÈíÇäÇÊ ÍÓÇÈß
4. İí ÇáÜ Dashboard¡ ÇÎÊÑ ÊØÈíŞß
5. ÇÖÛØ Úáì "Settings" ãä ÇáŞÇÆãÉ
6. ÇÈÍË Úä "App ID"
7. ÇäÓÎ ÇáŞíãÉ (ãËá: 12345678-1234-1234-1234-123456789012)
```

#### È) ÍÏøË Çáãáİ:
```csharp
// İí loukupm\services\OneSignalService.cs - ÇáÓØÑ 8
private static readonly string _appId = "YOUR-REAL-APP-ID";  // ? ŞÏíã
private static readonly string _appId = "12345678-1234-1234-1234-123456789012";  // ? ÌÏíÏ
```

---

## 2?? ÇÎÊÈÑ ÇáÊØÈíŞ

### Úáì Android:
```
1. Build ÇáãÔÑæÚ (Ctrl+Shift+B)
2. ÔÛá ÇáÊØÈíŞ Úáì ÌåÇÒ Ãæ ãÍÇßí
3. ÃäÔÆ ÍÓÇÈ ÌÏíÏ
4. ÑÇŞÈ ÇáÜ Console İí Visual Studio
```

### ÇÈÍË Úä ÑÓÇáÉ ãËá:
```
? OneSignal initialized successfully
? User 123 registered with OneSignal
? Tag added: email = user@example.com
```

---

## 3?? ÊÍŞŞ ãä OneSignal Dashboard

```
1. ÇĞåÈ Åáì https://onesignal.com
2. ÇÏÎá Dashboard
3. ÇÖÛØ Úáì "Audience" Ãæ "Subscribers"
4. íÌÈ Ãä ÊÔæİ ÇáãÓÊÎÏã ÇáÌÏíÏ
5. ÊİÇÕíáå ÊÙåÑ ãÚ ÇáÜ Tags (email, signup_type, ÅáÎ)
```

---

## 4?? ÃÑÓá ÅÔÚÇÑ ÊÌÑíÈí

```
1. İí OneSignal Dashboard¡ ÇÖÛØ "New Message"
2. ÇÏÎá ÚäæÇä ÇáÅÔÚÇÑ
3. ÇÏÎá ãÍÊæì ÇáÅÔÚÇÑ
4. ÇÎÊÑ "Test to Device" Ãæ "Send to Segment"
5. ÇÖÛØ "Send Now"
6. íÌÈ Ãä ÊÓÊŞÈá ÇáÅÔÚÇÑ Úáì ÇáÌåÇÒ
```

---

## ?? ãÚÇáÌÉ ÇáãÔÇßá ÇáÔÇÆÚÉ

### ? ÇáãÔßáÉ: "App ID not configured"
**ÇáÓÈÈ**: ãÇ ÒÇá `_appId = "YOUR-APP-ID"`  
**ÇáÍá**: ÇÓÊÈÏáå ÈÇáŞíãÉ ÇáÍŞíŞíÉ

### ? ÇáãÔßáÉ: ÇáãÓÊÎÏã ãÇ ÙåÑ İí Dashboard
**ÇáÓÈÈ**: ŞÏ Êßæä ÇáÔÈßÉ ãŞØæÚÉ Ãæ åäÇß ÎØÃ İí ÇáÜ App ID  
**ÇáÍá**: 
1. ÊÍŞŞ ãä ÇáÜ Console ááÃÎØÇÁ
2. ÊÃßÏ ãä App ID ÕÍíÍ
3. ÌÑøÈ ãÑÉ ÃÎÑì

### ? ÇáãÔßáÉ: ÇáÅÔÚÇÑÇÊ ãÇ ÊÌí
**ÇáÓÈÈ**: ŞÏ Êßæä ÇáÅĞæäÇÊ ÛíÑ ãİÚáÉ  
**ÇáÍá**: İí ÇáÌåÇÒ¡ ÑæÍ Settings ? ÇáÊØÈíŞ ? Notifications ? İÚøá ÇáÅÔÚÇÑÇÊ

---

## ?? ãáÎÕ ÇáİÚÇáíÇÊ ÇáãÑÕæÏÉ

ÇáÊØÈíŞ íÓÌá ÌãíÚ ÇáÚãáíÇÊ İí ÇáÜ Console:

```
App.xaml.cs:
? OneSignal initialized
  
LoginPage.xaml.cs:
? User 123 registered with OneSignal
? Tag added: user_id = 123
? Tag added: email = user@example.com

SinginPage.xaml.cs:
? User 123 registered with OneSignal
? Tag added: signup_date = 2024-01-15

ProfilePage.xaml.cs:
? OneSignal logout completed
```

---

## ?? ÇáÊæŞíÊ ÇáãÊæŞÚ:

| ÇáÚãáíÉ | ÇáæŞÊ |
|--------|------|
| ÊÍÏíË App ID | 2 ÏŞíŞÉ |
| ÈäÇÁ ÇáãÔÑæÚ | 1 ÏŞíŞÉ |
| ÇÎÊÈÇÑ ÈÊÓÌíá ÌÏíÏ | 30 ËÇäíÉ |
| ÇáÊÍŞŞ ãä Dashboard | 2 ÏŞíŞÉ |
| **ÇáãÌãæÚ** | **~5 ÏŞÇÆŞ** ?? |

---

## ? Checklist ÇáÊÍÖíÑ:

- [ ] ŞÑÇÁÉ åĞå ÇáÊÚáíãÇÊ
- [ ] ÇáÍÕæá Úáì App ID ãä OneSignal
- [ ] ÊÍÏíË Çáãáİ `OneSignalService.cs`
- [ ] ÈäÇÁ ÇáãÔÑæÚ ÈÏæä ÃÎØÇÁ
- [ ] ÇÎÊÈÇÑ ÇáÊÓÌíá ÇáÌÏíÏ
- [ ] ÇáÊÍŞŞ ãä ÇáãÓÊÎÏã İí Dashboard
- [ ] ÅÑÓÇá ÅÔÚÇÑ ÊÌÑíÈí
- [ ] ÇáÊÃßÏ ãä ÇÓÊŞÈÇá ÇáÅÔÚÇÑ ?

---

## ?? ÏÚã:

ÅĞÇ æÇÌåÊ ãÔÇßá:

1. ÊÍŞŞ ãä ÇáÜ Console İí Visual Studio
2. ÇÈÍË Úä ÇáÃÎØÇÁ ÈÇááæä ÇáÃÍãÑ
3. ÇäÓÎ ÇáÎØÃ æÇÈÍË Úäå İí documentation OneSignal
4. ÊÃßÏ ãä App ID ÕÍíÍ

---

**ÂÎÑ ÊÍÏíË**: Çáíæã  
**ÇáÍÇáÉ**: ? ÌÇåÒ ááÇÓÊÎÏÇã
