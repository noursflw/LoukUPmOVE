# ?? Ïáíá ÅÕáÇÍ ÇáÊäŞá Èíä ÇáÕİÍÇÊ æÒÑ ÇáÑÌæÚ

## ?? ÇáãÔßáÉ ÇáÃÓÇÓíÉ

ÚäÏ ÇáÖÛØ Úáì ÒÑ ÇáÑÌæÚ İí ÇáÕİÍÇÊ¡ ÇáÊØÈíŞ ßÇä íÎÑÌ ÊãÇãÇğ ÈÏáÇğ ãä ÇáÚæÏÉ ááÕİÍÉ ÇáÕÍíÍÉ.

---

## ? ÇáÍá ÇáĞí Êã ÊØÈíŞå

Êã ÅäÔÇÁ äÙÇã **NavigationService** ãÑßÒí íÏíÑ ÌãíÚ ÚãáíÇÊ ÇáÊäŞá ÈÔßá ÕÍíÍ.

---

## ?? åíßá ÇáÊäŞá

### ÇáãÌãæÚÉ 1: ÊäŞá Åáì LoginPage

```
SinginPage          ? ÒÑ ÑÌæÚ ? LoginPage ?
PolicyandPrivacyPage ? ÒÑ ÑÌæÚ ? LoginPage ?
RestPassword         ? ÒÑ ÑÌæÚ ? LoginPage ?
TermsAndConditions   ? ÒÑ ÑÌæÚ ? LoginPage ?
```

### ÇáãÌãæÚÉ 2: ÊäŞá Åáì HomePage

```
ServicesPage ? ÒÑ ÑÌæÚ ? HomePage ?
BookingPage  ? ÒÑ ÑÌæÚ ? HomePage ?
AboutUS      ? ÒÑ ÑÌæÚ ? HomePage ?
ProfilePage  ? ÒÑ ÑÌæÚ ? HomePage ?
```

### ÇáãÌãæÚÉ 3: ÊäŞá Åáì ProfilePage

```
EditeUserPage      ? ÒÑ ÑÌæÚ ? ProfilePage ?
EditePasswordPage  ? ÒÑ ÑÌæÚ ? ProfilePage ?
ProfilePage        ? íÑÌÚ áÍíË ÌÇÁ (ÏíäÇãíßí)
```

### ÇáÍÇáÇÊ ÇáÎÇÕÉ: ProfilePage ÇáÏíäÇãíßíÉ

```
AboutUS ? ProfilePage ? ÒÑ ÑÌæÚ ? AboutUS ?
BookingPage ? ProfilePage ? ÒÑ ÑÌæÚ ? BookingPage ?
PolicyandPrivacyPage ? ProfilePage ? ÒÑ ÑÌæÚ ? PolicyandPrivacyPage ?
```

---

## ?? ßíİíÉ ÇáÊØÈíŞ Úáì ßá ÕİÍÉ

### ÇáØÑíŞÉ ÇáÓåáÉ (ÇáÃÓåá):

```csharp
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("PageName");
    return true;
}
```

**ãËÇá:**
```csharp
// İí PolicyandPrivacyPage.xaml.cs
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("PolicyandPrivacyPage");
    return true;
}
```

### ÇáØÑíŞÉ ÇáãÊŞÏãÉ (ááÕİÍÇÊ ÇáÏíäÇãíßíÉ):

ááÕİÍÇÊ ÇáÊí ŞÏ ÊõÒÇÑ ãä ÕİÍÇÊ ãÊÚÏÏÉ (ãËá ProfilePage):

```csharp
// ÚäÏ ÇáÊäŞá Åáì ÇáÕİÍÉ:
await NavigationService.NavigateToWithSource("ProfilePage", "AboutUS");

// Ãæ ãä ÇáÕİÍÉ ÇáÍÇáíÉ:
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("ProfilePage");
    return true;
}
```

---

## ?? ßæÏ ÌÇåÒ ááäÓÎ æÇááÕŞ

### áßá ÕİÍÉ İí ÇáãÌãæÚÉ 1:

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

// TermsAndConditions.xaml.cs (ÅĞÇ áã Êßä ãæÌæÏÉ)
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("TermsAndConditions");
    return true;
}
```

### áßá ÕİÍÉ İí ÇáãÌãæÚÉ 2:

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
```

### áßá ÕİÍÉ İí ÇáãÌãæÚÉ 3:

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

// ProfilePage.xaml.cs (ãåãÉ ÌÏÇğ - ÊÏÚã ÇáÑÌæÚ ÇáÏíäÇãíßí)
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("ProfilePage");
    return true;
}
```

### ÚäÏ ÇáÇäÊŞÇá Åáì ProfilePage ãä ÕİÍÇÊ ãÎÊáİÉ:

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

// ãä PolicyandPrivacyPage.xaml.cs (ÅĞÇ áÒã ÇáÃãÑ):
private async void GoToProfile()
{
    await NavigationService.NavigateToWithSource("ProfilePage", "PolicyandPrivacyPage");
}
```

---

## ?? ÂáíÉ ÇáÚãá

### ÚäÏ ÇáÖÛØ Úáì ÒÑ ÇáÑÌæÚ:

```
1. OnBackButtonPressed() íäÇÏí NavigationService.HandleBackButton()
2. ÇáÎÏãÉ ÊÊÍŞŞ ãä ÎÑíØÉ ÇáãÕÇÏÑ (PageSourceMap)
3. ÅĞÇ æõÌÏÊ ÕİÍÉ ÇáãÕÏÑ¡ ÊõÓÊÎÏã
4. æÅáÇ¡ ÊõÓÊÎÏã ÇáÎÑíØÉ ÇáÇİÊÑÇÖíÉ (BackNavigationMap)
5. íÊã ÇáÊäŞá Åáì ÇáÕİÍÉ ÇáÕÍíÍÉ
```

### ãËÇá Úãáí:

```
ÇáãÓÊÎÏã Úáì ProfilePage
?
íÖÛØ ÒÑ ÇáÑÌæÚ
?
OnBackButtonPressed() íõØáŞ
?
NavigationService.HandleBackButton("ProfilePage")
?
ÇáÈÍË İí PageSourceMap Úä "ProfilePage"
?
æõÌÏ: "ProfilePage" ÌÇÁ ãä "AboutUS"
?
ÇáÊäŞá Åáì AboutUS ?
?
ÍĞİ ÇáÏÎæá ãä PageSourceMap
```

---

## ?? ŞÇÆãÉ ÇáãáİÇÊ

### Êã ÅäÔÇÁ:
- ? `NavigationService.cs` - ÎÏãÉ ÇáÊäŞá ÇáãÑßÒíÉ
- ? `NavigationAwarePage.cs` - base class ÇÎÊíÇÑí

### Êã ÊÚÏíá:
- ? `LoginPage.xaml.cs` - ãäÚ ÇáÎÑæÌ ãä ÇáÊØÈíŞ
- ? `SinginPage.xaml.cs` - ÇÓÊÎÏÇã NavigationService

### íÌÈ ÊÚÏíá:
- [ ] `PolicyandPrivacyPage.xaml.cs`
- [ ] `RestPassword.xaml.cs`
- [ ] `TermsAndConditions.xaml.cs`
- [ ] `ServicesPage.xaml.cs`
- [ ] `BookingPage.xaml.cs`
- [ ] `AboutUS.xaml.cs`
- [ ] `ProfilePage.xaml.cs`
- [ ] `EditeUserPage.xaml.cs`
- [ ] `EditePasswordPage.xaml.cs`

---

## ?? ÇáÎØæÇÊ ÇáÊÇáíÉ

### ÇáÎØæÉ 1: ÇáãÌãæÚÉ 1 (ÊäŞá Åáì LoginPage)
```csharp
// İí ßá ãáİ .xaml.cs:
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("ClassName");
    return true;
}
```

**ÇáÕİÍÇÊ:**
- PolicyandPrivacyPage
- RestPassword
- TermsAndConditions

### ÇáÎØæÉ 2: ÇáãÌãæÚÉ 2 (ÊäŞá Åáì HomePage)
```csharp
// äİÓ ÇáßæÏ ÃÚáÇå
```

**ÇáÕİÍÇÊ:**
- ServicesPage
- BookingPage
- AboutUS

### ÇáÎØæÉ 3: ÇáãÌãæÚÉ 3 (ÊäŞá Åáì ProfilePage)
```csharp
// äİÓ ÇáßæÏ ÃÚáÇå
```

**ÇáÕİÍÇÊ:**
- EditeUserPage
- EditePasswordPage
- ProfilePage

### ÇáÎØæÉ 4: ÇáÇäÊŞÇáÇÊ ÇáÏíäÇãíßíÉ
```csharp
// ÚäÏ ÇáÇäÊŞÇá Åáì ProfilePage:
await NavigationService.NavigateToWithSource("ProfilePage", "AboutUS");
```

**ÇáÃãÇßä:**
- ãä AboutUS Åáì ProfilePage
- ãä BookingPage Åáì ProfilePage
- ãä PolicyandPrivacyPage Åáì ProfilePage

---

## ? ÇáããíÒÇÊ

? **äÙÇã ãæÍÏ** - ßá ÇáÕİÍÇÊ ÊÓÊÎÏã äİÓ ÇáØÑíŞÉ
? **ãÑä** - íÏÚã ÇáÊäŞá ÇáËÇÈÊ æÇáÏíäÇãíßí
? **Âãä** - áÇ íÓãÍ ÈÇáÎÑæÌ ãä ÇáÊØÈíŞ ÈØÑíŞÉ ÛíÑ ãÊÚŞÈ
? **Óåá ÇáÕíÇäÉ** - ßæÏ ãÑßÒí æÓåá ÇáÊÚÏíá
? **ŞÇÈá ááÊæÓÚ** - íãßä ÅÖÇİÉ ÕİÍÇÊ ÌÏíÏÉ ÈÓåæáÉ

---

## ?? ÇáÇÎÊÈÇÑ

### ÇÎÊÈÇÑ ÇáãÌãæÚÉ 1:
```
1. ÇĞåÈ Åáì LoginPage
2. ÇäŞÑ Úáì "ÊÓÌíá ÌÏíÏ" ? SinginPage
3. ÇÖÛØ ÇáÑÌæÚ ? íÌÈ ÇáÚæÏÉ Åáì LoginPage ?

4. ãä LoginPage ÇäŞÑ Úáì "äÓíÊ ßáãÉ ÇáãÑæÑ" ? RestPassword
5. ÇÖÛØ ÇáÑÌæÚ ? íÌÈ ÇáÚæÏÉ Åáì LoginPage ?

6. ãä LoginPage ÇäŞÑ Úáì "ÇáÔÑæØ æÇáÃÍßÇã" ? TermsAndConditions
7. ÇÖÛØ ÇáÑÌæÚ ? íÌÈ ÇáÚæÏÉ Åáì LoginPage ?
```

### ÇÎÊÈÇÑ ÇáãÌãæÚÉ 2:
```
1. ÓÌá ÇáÏÎæá ? HomePage
2. ÇäŞÑ Úáì Services ? ServicesPage
3. ÇÖÛØ ÇáÑÌæÚ ? íÌÈ ÇáÚæÏÉ Åáì HomePage ?

4. ãä HomePage ÇäŞÑ Úáì Booking ? BookingPage
5. ÇÖÛØ ÇáÑÌæÚ ? íÌÈ ÇáÚæÏÉ Åáì HomePage ?

6. ãä HomePage ÇäŞÑ Úáì "ãÚáæãÇÊ" ? AboutUS
7. ÇÖÛØ ÇáÑÌæÚ ? íÌÈ ÇáÚæÏÉ Åáì HomePage ?
```

### ÇÎÊÈÇÑ ÇáãÌãæÚÉ 3 (ÇáÏíäÇãíßíÉ):
```
1. ãä AboutUS ÇäŞÑ Úáì Profile
   - NavigateToWithSource("ProfilePage", "AboutUS")
2. ãä ProfilePage ÇÖÛØ ÇáÑÌæÚ ? AboutUS ?

3. ãä BookingPage ÇäŞÑ Úáì Profile
   - NavigateToWithSource("ProfilePage", "BookingPage")
4. ãä ProfilePage ÇÖÛØ ÇáÑÌæÚ ? BookingPage ?

5. ãä ProfilePage ÇäŞÑ Úáì "ÊÚÏíá ÇáÈíÇäÇÊ" ? EditeUserPage
6. ÇÖÛØ ÇáÑÌæÚ ? ProfilePage ?
```

---

## ?? ãáÇÍÙÇÊ ÃãäíÉ

? **ãäÚ ÇáÎÑæÌ ÛíÑ ÇáãŞÕæÏ** - áÇ íãßä ÇáÎÑæÌ ãä ÇáÊØÈíŞ ÈÏæä ŞÕÏ
? **ÊÊÈÚ ÇáãÓÇÑ** - íãßä ãÚÑİÉ ãÓÇÑ ÇáãÓÊÎÏã
? **ÚÏã İŞÏÇä ÇáÈíÇäÇÊ** - áÇ íÊã ÍĞİ ÇáÈíÇäÇÊ ÈÓÈÈ ÎØÃ ÇáÊäŞá
? **ÊÌÑÈÉ ÂãäÉ** - ÇáãÓÊÎÏã íÚÑİ ÏÇÆãÇğ Ãíä åæ ĞÇåÈ

---

## ?? ÇáãáÎÕ

| ÇáãÑÍáÉ | ÚÏÏ ÇáÕİÍÇÊ | ÇáæÕİ |
|--------|-----------|-------|
| Êã ÅäÌÇÒå | 2 | LoginPage + SinginPage |
| íÌÈ ÅÕáÇÍå | 7 | Remaining pages |
| **ÇáÅÌãÇáí** | **9** | **ÌãíÚ ÇáÕİÍÇÊ** |

---

**ÂÎÑ ÊÍÏíË:** ÏíÓãÈÑ 2024  
**ÇáÍÇáÉ:** ÌÇåÒ ááÊØÈíŞ ??
