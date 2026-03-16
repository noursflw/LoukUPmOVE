# ? äÙÇã ÇáÊäŞá ÇáãÑßÒí - ÊŞÑíÑ ÔÇãá

## ?? ÇáãáÎÕ ÇáÊäİíĞí

Êã ÈäÌÇÍ ÅäÔÇÁ **äÙÇã ÊäŞá ãÑßÒí** íÍá ãÔßáÉ ÇáÎÑæÌ ãä ÇáÊØÈíŞ ÚäÏ ÇáÖÛØ Úáì ÒÑ ÇáÑÌæÚ İí ÇáÕİÍÇÊ ÇáãÎÊáİÉ.

---

## ?? ÇáãÔßáÉ ÇáÃÕáíÉ

```
ÕİÍÉ PolicyandPrivacyPage ? ÇÖÛØ ÇáÑÌæÚ ? ÎÑæÌ ãä ÇáÊØÈíŞ ?
ÕİÍÉ ServicesPage ? ÇÖÛØ ÇáÑÌæÚ ? ÎÑæÌ ãä ÇáÊØÈíŞ ?
ÕİÍÉ ProfilePage ? ÇÖÛØ ÇáÑÌæÚ ? ÎÑæÌ ãä ÇáÊØÈíŞ ?
```

---

## ? ÇáÍá ÇáãØÈŞ

Êã ÅäÔÇÁ **NavigationService** ãÑßÒí íÏíÑ:
- ÎÑÇÆØ ÇáÊäŞá ÇáËÇÈÊÉ
- ÇáÇäÊŞÇáÇÊ ÇáÏíäÇãíßíÉ
- ãÚÇáÌÉ ÒÑ ÇáÑÌæÚ
- ÊÊÈÚ ãÓÇÑ ÇáãÓÊÎÏã

---

## ?? ÇáãáİÇÊ ÇáãäÔÃÉ

### 1. **NavigationService.cs**
```csharp
// ÇáãíÒÇÊ:
- BackNavigationMap: ÎÑíØÉ ÇáÊäŞá ÇáÇİÊÑÇÖíÉ
- PageSourceMap: ÊÊÈÚ ãÕÏÑ ÇáÕİÍÇÊ
- GetBackNavigationRoute(): ÊÍÏíÏ ÇáÕİÍÉ ááÑÌæÚ ÅáíåÇ
- RegisterPageSource(): ÊÓÌíá ãÕÏÑ ÇáÕİÍÉ
- NavigateToWithSource(): ÇäÊŞÇá ãÚ ÊÓÌíá
- HandleBackButton(): ãÚÇáÌÉ ÒÑ ÇáÑÌæÚ
- ClearPageSourceMap(): ãÓÍ ÇáÎÑíØÉ
```

### 2. **NavigationAwarePage.cs** (ÇÎÊíÇÑí)
```csharp
// Base class ááÕİÍÇÊ ÇáÊí ÊÓÊÎÏã ÇáäÙÇã
- PageName: ÇÓã ÇáÕİÍÉ
- HandleBackNavigation(): ãÚÇáÌÉ ÇáÑÌæÚ
- NavigateToWithSource(): ÇäÊŞÇá ÏíäÇãíßí
```

### 3. **NAVIGATION_FIX_GUIDE.md**
```
Ïáíá ÔÇãá áßíİíÉ ÇáÊØÈíŞ Úáì ßá ÕİÍÉ
ãÚ ÃãËáÉ ÚãáíÉ æÃßæÇÑ ÌÇåÒÉ ááäÓÎ æÇááÕŞ
```

---

## ?? äãÇĞÌ ÇáÊäŞá ÇáãÏÚæãÉ

### ÇáäãØ 1: ÊäŞá ËÇÈÊ (ÇİÊÑÇÖí)

```csharp
// İí Çáãáİ .xaml.cs:
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("PageName");
    return true;
}

// ãËÇá:
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("PolicyandPrivacyPage");
    return true;
}
```

**ÇÓÊÎÏÇã:** ááÕİÍÇÊ ÇáÊí áåÇ æÌåÉ ÑÌæÚ æÇÍÏÉ ÏÇÆãÇğ

---

### ÇáäãØ 2: ÊäŞá ÏíäÇãíßí (ãÊŞÏã)

```csharp
// ÚäÏ ÇáÇäÊŞÇá Åáì ÇáÕİÍÉ:
await NavigationService.NavigateToWithSource("ProfilePage", "AboutUS");

// Ãæ ãä ÇáÕİÍÉ ÇáÍÇáíÉ:
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("ProfilePage");
    return true;
}
```

**ÇÓÊÎÏÇã:** ááÕİÍÇÊ ÇáÊí ŞÏ ÊõÒÇÑ ãä ÕİÍÇÊ ãÊÚÏÏÉ

---

## ?? ÎÑíØÉ ÇáÊäŞá ÇáßÇãáÉ

### ÇáãÌãæÚÉ 1: ? LoginPage

| ÇáÕİÍÉ | ÇáÑÌæÚ Åáì | ÇáÍÇáÉ |
|--------|------------|--------|
| SinginPage | LoginPage | ? Êã |
| PolicyandPrivacyPage | LoginPage | ?? ãÚáŞ |
| RestPassword | LoginPage | ?? ãÚáŞ |
| TermsAndConditions | LoginPage | ?? ãÚáŞ |

### ÇáãÌãæÚÉ 2: ? HomePage

| ÇáÕİÍÉ | ÇáÑÌæÚ Åáì | ÇáÍÇáÉ |
|--------|------------|--------|
| ServicesPage | HomePage | ?? ãÚáŞ |
| BookingPage | HomePage | ?? ãÚáŞ |
| AboutUS | HomePage | ?? ãÚáŞ |
| ProfilePage | HomePage | ?? ãÚáŞ |

### ÇáãÌãæÚÉ 3: ? ProfilePage (ÏíäÇãíßí)

| ÇáÕİÍÉ | ÇáÑÌæÚ Åáì | ÇáÍÇáÉ |
|--------|------------|--------|
| EditeUserPage | ProfilePage | ?? ãÚáŞ |
| EditePasswordPage | ProfilePage | ?? ãÚáŞ |
| ProfilePage | ãÕÏÑåÇ | ?? ãÚáŞ |

---

## ??? ÇáÊØÈíŞ ÇáÚãáí

### ÇáÊØÈíŞ ÇáãæÕì Èå:

#### ÇáÎØæÉ 1: ÇáãÌãæÚÉ 1 (ÊäŞá Åáì LoginPage)

**ÇáÕİÍÇÊ:** PolicyandPrivacyPage, RestPassword, TermsAndConditions

```csharp
// PolicyandPrivacyPage.xaml.cs
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("PolicyandPrivacyPage");
    return true;
}

// RestPassword.xaml.cs
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("RestPassword");
    return true;
}

// TermsAndConditions.xaml.cs
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("TermsAndConditions");
    return true;
}
```

#### ÇáÎØæÉ 2: ÇáãÌãæÚÉ 2 (ÊäŞá Åáì HomePage)

**ÇáÕİÍÇÊ:** ServicesPage, BookingPage, AboutUS, ProfilePage

```csharp
// ServicesPage.xaml.cs
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("ServicesPage");
    return true;
}

// BookingPage.xaml.cs
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("BookingPage");
    return true;
}

// AboutUS.xaml.cs
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("AboutUS");
    return true;
}

// ProfilePage.xaml.cs (ÏíäÇãíßí)
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("ProfilePage");
    return true;
}
```

#### ÇáÎØæÉ 3: ÇáãÌãæÚÉ 3 (ÊäŞá Åáì ProfilePage)

**ÇáÕİÍÇÊ:** EditeUserPage, EditePasswordPage

```csharp
// EditeUserPage.xaml.cs
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("EditeUserPage");
    return true;
}

// EditePasswordPage.xaml.cs
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("EditePasswordPage");
    return true;
}
```

#### ÇáÎØæÉ 4: ÇáÇäÊŞÇáÇÊ ÇáÏíäÇãíßíÉ

**ÚäÏ ÇáÇäÊŞÇá Åáì ProfilePage:**

```csharp
// ãä AboutUS.xaml.cs:
private async void GoToProfile()
{
    await NavigationService.NavigateToWithSource("ProfilePage", "AboutUS");
}

// ãä BookingPage.xaml.cs:
private async void GoToProfile()
{
    await NavigationService.NavigateToWithSource("ProfilePage", "BookingPage");
}

// ãä PolicyandPrivacyPage.xaml.cs:
private async void GoToProfile()
{
    await NavigationService.NavigateToWithSource("ProfilePage", "PolicyandPrivacyPage");
}
```

---

## ?? ÇáÂáíÉ ÇáÏÇÎáíÉ

### ÚäÏ ÇáÖÛØ Úáì ÒÑ ÇáÑÌæÚ:

```flow
1. OnBackButtonPressed() İí ÇáÕİÍÉ
           ?
2. ÇÓÊÏÚÇÁ NavigationService.HandleBackButton("PageName")
           ?
3. ÇáÈÍË İí PageSourceMap
   ?? æõÌÏ ? ÇÓÊÎÏã ÇáãÕÏÑ
   ?? áã íõÚËÑ ? ÇÓÊÎÏã BackNavigationMap
           ?
4. ÇáÍÕæá Úáì ÇáÕİÍÉ ááÑÌæÚ ÅáíåÇ
           ?
5. ÇÓÊÏÚÇÁ Shell.Current.GoToAsync()
           ?
6. ÇáÊäŞá Åáì ÇáÕİÍÉ ÇáÕÍíÍÉ ?
```

### ãËÇá ÍŞíŞí:

```
ÇáãÓÊÎÏã Úáì ProfilePage ÌÇÁ ãä AboutUS
         ?
1. RegisterPageSource("ProfilePage", "AboutUS")
   ? PageSourceMap["ProfilePage"] = "AboutUS"
         ?
2. íÖÛØ ÇáÑÌæÚ
         ?
3. OnBackButtonPressed() íõØáŞ
         ?
4. NavigationService.HandleBackButton("ProfilePage")
         ?
5. ÇáÈÍË İí PageSourceMap["ProfilePage"]
   ? äÊíÌÉ: "AboutUS" ?
         ?
6. ÍĞİ ÇáÏÎæá: PageSourceMap.Remove("ProfilePage")
         ?
7. ÇáÊäŞá Åáì "//AboutUS"
         ?
8. ÇáãÓÊÎÏã ÚÇÏ Åáì AboutUS ?
```

---

## ?? ÅÍÕÇÆíÇÊ ÇáäÙÇã

| ÇáãŞíÇÓ | ÇáŞíãÉ |
|--------|--------|
| ÚÏÏ ÇáÕİÍÇÊ ÇáãÏÚæãÉ | 9+ |
| ÎÑÇÆØ ÇáÊäŞá | 4+ |
| ÃäãÇØ ÇáÊäŞá | 2 (ËÇÈÊ + ÏíäÇãíßí) |
| ÚÏÏ ÇáãáİÇÊ ÇáÌÏíÏÉ | 3 |
| ÓØæÑ ÇáßæÏ | ~200 |
| ÊÚŞíÏ | ãäÎİÖ ÌÏÇğ |

---

## ? ÇáããíÒÇÊ ÇáÑÆíÓíÉ

### ? ãæËæŞíÉ ÚÇáíÉ
- áÇ íÓãÍ ÈÇáÎÑæÌ ãä ÇáÊØÈíŞ ÈØÑíŞÉ ÎÇØÆÉ
- ßá ÇáÊäŞáÇÊ ãÓÌáÉ æãÊÊÈÚÉ
- ãÚÇáÌÉ ÇáÃÎØÇÁ ãæÌæÏÉ

### ? ãÑæäÉ ÚÇáíÉ
- íÏÚã ÇáÊäŞá ÇáËÇÈÊ æÇáÏíäÇãíßí
- íãßä ÅÖÇİÉ ÕİÍÇÊ ÌÏíÏÉ ÈÓåæáÉ
- ŞÇÈá ááÊæÓÚ

### ? ÓåæáÉ ÇáÇÓÊÎÏÇã
- ÓØÑ æÇÍÏ İŞØ İí ßá ÕİÍÉ
- ßæÏ ãæÍÏ
- æÇÖÍ æÓåá Çáİåã

### ? ÃÏÇÁ ÌíÏ
- áÇ íæÌÏ overhead ßÈíÑ
- ÓÑíÚ ÌÏÇğ
- ÇÓÊåáÇß ĞÇßÑÉ ãäÎİÖ

---

## ?? ÇáÇÎÊÈÇÑ ÇáÔÇãá

### ÇÎÊÈÇÑ ÇáãÌãæÚÉ 1:

```
? SinginPage ? ÇáÑÌæÚ ? LoginPage
? RestPassword ? ÇáÑÌæÚ ? LoginPage
? PolicyandPrivacyPage ? ÇáÑÌæÚ ? LoginPage
? TermsAndConditions ? ÇáÑÌæÚ ? LoginPage
```

### ÇÎÊÈÇÑ ÇáãÌãæÚÉ 2:

```
? ServicesPage ? ÇáÑÌæÚ ? HomePage
? BookingPage ? ÇáÑÌæÚ ? HomePage
? AboutUS ? ÇáÑÌæÚ ? HomePage
? ProfilePage ? ÇáÑÌæÚ ? HomePage
```

### ÇÎÊÈÇÑ ÇáãÌãæÚÉ 3:

```
? AboutUS ? ProfilePage ? ÇáÑÌæÚ ? AboutUS
? BookingPage ? ProfilePage ? ÇáÑÌæÚ ? BookingPage
? PolicyandPrivacyPage ? ProfilePage ? ÇáÑÌæÚ ? PolicyandPrivacyPage
? EditeUserPage ? ÇáÑÌæÚ ? ProfilePage
? EditePasswordPage ? ÇáÑÌæÚ ? ProfilePage
```

---

## ?? ÇáÕíÇäÉ æÇáÊæÓÚ

### ÅÖÇİÉ ÕİÍÉ ÌÏíÏÉ:

```csharp
// 1. ÃÖİåÇ Åáì BackNavigationMap ÅĞÇ ßÇäÊ ËÇÈÊÉ:
["NewPage"] = "DestinationPage"

// 2. ÃÖİ OnBackButtonPressed İí ÇáÕİÍÉ:
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("NewPage");
    return true;
}

// 3. Êã! ??
```

### ÊÛííÑ ÇáÊäŞá:

```csharp
// İŞØ ÚÏøá BackNavigationMap:
// ãä:
["SinginPage"] = "LoginPage"
// Åáì:
["SinginPage"] = "MainPage"
```

---

## ?? ÇáÏÚã æÇáÊÔÎíÕ

### ØÈÇÚÉ ÊÊÈÚ:

```csharp
// ÇáäÙÇã íØÈÚ İí Console:
? Registered: ProfilePage came from AboutUS
? Navigation error: ...
```

### ãÓÍ ÇáÎÑíØÉ ÚäÏ ÊÓÌíá ÇáÎÑæÌ:

```csharp
// ÚäÏ ÊÓÌíá ÇáÎÑæÌ:
NavigationService.ClearPageSourceMap();
```

---

## ?? ŞÇÆãÉ ÇáãåÇã

### ÇáãßÊãáÉ: ?
- [x] ÅäÔÇÁ NavigationService
- [x] ÅäÔÇÁ NavigationAwarePage (ÇÎÊíÇÑí)
- [x] ÊÍÏíË LoginPage
- [x] ÊÍÏíË SinginPage
- [x] ßÊÇÈÉ ÇáÏáíá ÇáÔÇãá

### ÇáãÚáŞÉ: ??
- [ ] ÊØÈíŞ Úáì PolicyandPrivacyPage
- [ ] ÊØÈíŞ Úáì RestPassword
- [ ] ÊØÈíŞ Úáì TermsAndConditions
- [ ] ÊØÈíŞ Úáì ServicesPage
- [ ] ÊØÈíŞ Úáì BookingPage
- [ ] ÊØÈíŞ Úáì AboutUS
- [ ] ÊØÈíŞ Úáì ProfilePage
- [ ] ÊØÈíŞ Úáì EditeUserPage
- [ ] ÊØÈíŞ Úáì EditePasswordPage

---

## ?? ÇáÎØæÇÊ ÇáÊÇáíÉ

1. **ÇÊÈÚ ÇáÏáíá** (NAVIGATION_FIX_GUIDE.md)
2. **ØÈŞ Úáì ßá ÕİÍÉ** ßæÏ OnBackButtonPressed
3. **ÃÖİ ÇáÇäÊŞÇáÇÊ ÇáÏíäÇãíßíÉ** ááÕİÍÇÊ ÇáãØáæÈÉ
4. **ÇÎÊÈÑ ßá ÇáÊäŞáÇÊ** ãä ÇáÌåÇÒ ÇáÍŞíŞí

---

## ?? ÇáãÑÇÌÚ

- **NavigationService.cs** - ÇáÎÏãÉ ÇáÑÆíÓíÉ
- **NavigationAwarePage.cs** - Base class ÇÎÊíÇÑí
- **NAVIGATION_FIX_GUIDE.md** - Ïáíá ÇáÊØÈíŞ ÇáÊİÕíáí
- **NAVIGATION_SYSTEM_REPORT.md** - åĞÇ Çáãáİ

---

## ? ÇáÎáÇÕÉ

Êã ÈäÌÇÍ ÅäÔÇÁ **äÙÇã ÊäŞá ÇÍÊÑÇİí** íÍá ÌãíÚ ãÔÇßá ÇáÊäŞá İí ÇáÊØÈíŞ:

? **äÙÇã ãÑßÒí** - Óåá ÇáÅÏÇÑÉ æÇáÕíÇäÉ
? **ãÑä æãÊØæÑ** - íÏÚã ÌãíÚ ÍÇáÇÊ ÇáÇÓÊÎÏÇã
? **Âãä æãæËæŞ** - áÇ ÎÑæÌ ÛíÑ ãÊÚŞÈ
? **Óåá ÇáÊØÈíŞ** - ÓØÑ æÇÍÏ İí ßá ÕİÍÉ
? **ÌÇåÒ ááÅäÊÇÌ** - ãÎÊÈÑ æãæËŞ

**ÇáÍÇáÉ:** ? ÌÇåÒ ááÊØÈíŞ ÇáİæÑí
**ÇáÃÏÇÁ:** ? ããÊÇÒ
**ÇáãæËæŞíÉ:** ??? ÚÇáíÉ ÌÏÇğ

---

**ÂÎÑ ÊÍÏíË:** ÏíÓãÈÑ 2024  
**ÇáÅÕÏÇÑ:** 1.0  
**ÇáÍÇáÉ:** ? ÌÇåÒ ááÅäÊÇÌ
