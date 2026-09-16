# ? Ïáíá ÇáÊØÈíŞ ÇáÓÑíÚ - äÓÎ æÇááÕŞ

## ?? ØÑíŞÉ ÓÑíÚÉ ÌÏÇğ

ßá ãÇ ÊÍÊÇÌ åæ äÓÎ åĞÇ ÇáßæÏ İí ßá ãáİ `xaml.cs`:

```csharp
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("ClassName");
    return true;
}
```

**İŞØ ÛíøÑ "ClassName" ÈÇÓã ÇáÕİÍÉ ÇáİÚáí!**

---

## ?? ÇáÕİÍÇÊ æÃßæÇÏåÇ ÇáÌÇåÒÉ

### ÇáãÌãæÚÉ 1: ÊäŞá Åáì LoginPage

#### 1?? PolicyandPrivacyPage.xaml.cs
```csharp
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("PolicyandPrivacyPage");
    return true;
}
```

#### 2?? RestPassword.xaml.cs
```csharp
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("RestPassword");
    return true;
}
```

#### 3?? TermsAndConditions.xaml.cs
```csharp
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("TermsAndConditions");
    return true;
}
```

---

### ÇáãÌãæÚÉ 2: ÊäŞá Åáì HomePage

#### 4?? ServicesPage.xaml.cs
```csharp
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("ServicesPage");
    return true;
}
```

#### 5?? BookingPage.xaml.cs
```csharp
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("BookingPage");
    return true;
}
```

#### 6?? AboutUS.xaml.cs
```csharp
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("AboutUS");
    return true;
}
```

#### 7?? ProfilePage.xaml.cs
```csharp
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("ProfilePage");
    return true;
}
```

---

### ÇáãÌãæÚÉ 3: ÊäŞá Åáì ProfilePage

#### 8?? EditeUserPage.xaml.cs
```csharp
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("EditeUserPage");
    return true;
}
```

#### 9?? EditePasswordPage.xaml.cs
```csharp
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("EditePasswordPage");
    return true;
}
```

---

## ?? ÇáÇäÊŞÇáÇÊ ÇáÏíäÇãíßíÉ (ProfilePage)

**ÃÖİåÇ İí ÇáÃãÇßä ÇáÊí ÊäÊŞá ãäåÇ Åáì ProfilePage:**

### ãä AboutUS Åáì ProfilePage:
```csharp
private async void GoToProfile()
{
    await NavigationService.NavigateToWithSource("ProfilePage", "AboutUS");
}
```

### ãä BookingPage Åáì ProfilePage:
```csharp
private async void GoToProfile()
{
    await NavigationService.NavigateToWithSource("ProfilePage", "BookingPage");
}
```

### ãä PolicyandPrivacyPage Åáì ProfilePage:
```csharp
private async void GoToProfile()
{
    await NavigationService.NavigateToWithSource("ProfilePage", "PolicyandPrivacyPage");
}
```

---

## ?? ÎØæÇÊ ÇáÊØÈíŞ

### ÇáØÑíŞÉ 1: ÇÈÍË Úä OnBackButtonPressed

```csharp
// ÇÈÍË Úä åĞÇ İí ßá ãáİ .xaml.cs:
protected override bool OnBackButtonPressed()
{
    // ßæÏ ŞÏíã åäÇ
}
```

### ÇáØÑíŞÉ 2: ÇÓÊÈÏáå ÈÜ:

```csharp
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton("PageName");
    return true;
}
```

### ÇáØÑíŞÉ 3: ÇÓÊÈÏá "PageName" ÈÇÓã ÇáÕİÍÉ ÇáİÚáí

---

## ? ŞÇÆãÉ ÇáÊÍŞŞ

- [ ] PolicyandPrivacyPage - ÊÍÏíË ÇáßæÏ
- [ ] RestPassword - ÊÍÏíË ÇáßæÏ
- [ ] TermsAndConditions - ÊÍÏíË ÇáßæÏ
- [ ] ServicesPage - ÊÍÏíË ÇáßæÏ
- [ ] BookingPage - ÊÍÏíË ÇáßæÏ
- [ ] AboutUS - ÊÍÏíË ÇáßæÏ
- [ ] ProfilePage - ÊÍÏíË ÇáßæÏ
- [ ] EditeUserPage - ÊÍÏíË ÇáßæÏ
- [ ] EditePasswordPage - ÊÍÏíË ÇáßæÏ
- [ ] ÃÖİÊ ÇáÇäÊŞÇáÇÊ ÇáÏíäÇãíßíÉ (ÇÎÊíÇÑí)
- [ ] ÇÎÊÈÑÊ ßá ÇáÕİÍÇÊ

---

## ?? ÇáÇÎÊÈÇÑ ÇáÓÑíÚ

```
1. ÇÖÛØ ÇáÑÌæÚ ãä PolicyandPrivacyPage ? åá ÊÚæÏ áÜ LoginPage? ?
2. ÇÖÛØ ÇáÑÌæÚ ãä ServicesPage ? åá ÊÚæÏ áÜ HomePage? ?
3. ÇÖÛØ ÇáÑÌæÚ ãä EditeUserPage ? åá ÊÚæÏ áÜ ProfilePage? ?
4. ãä AboutUS ? ProfilePage ? ÇáÑÌæÚ ? åá ÊÚæÏ áÜ AboutUS? ?
```

---

## ?? ãáÇÍÙÇÊ ãåãÉ

?? **áÇ ÊäÓó:**
- ÇÓÊÈÏá "ClassName" ÈÇÓã ÇáÕİÍÉ ÇáİÚáí
- ÇÓÊÎÏã äİÓ ÇáÃÍÑİ ÇáßÈíÑÉ æÇáÕÛíÑÉ
- ÇÎÊÈÑ ãä ÌåÇÒß ÇáİÚáí
- ÊÃßÏ ãä ÈäÇÁ ÇáßæÏ ÈäÌÇÍ

---

## ?? ÅĞÇ æÇÌåÊ ãÔßáÉ

### ÇáãÔßáÉ: ÇáÕİÍÉ ÊÎÑÌ ãä ÇáÊØÈíŞ
**ÇáÍá:** ÊÃßÏ ãä ÇÓã ÇáÕİÍÉ ÕÍíÍ

### ÇáãÔßáÉ: ÇáÕİÍÉ áÇ ÊÚæÏ áÃí ãßÇä
**ÇáÍá:** ÊÍŞŞ ãä BackNavigationMap İí NavigationService

### ÇáãÔßáÉ: ProfilePage ÊÚæÏ áãßÇä ÎÇØÆ
**ÇáÍá:** ÇÓÊÎÏã NavigateToWithSource ÚäÏ ÇáÇäÊŞÇá

---

**ÇáæŞÊ ÇáãÊæŞÚ ááÊØÈíŞ:** 10 ÏŞÇÆŞ ??  
**ÏÑÌÉ ÇáÕÚæÈÉ:** Óåá ÌÏÇğ ??  
**ÇáäÊíÌÉ:** ÊØÈíŞ ÈÏæä ãÔÇßá ÊäŞá ?
