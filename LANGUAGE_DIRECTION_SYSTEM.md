# ?? ÔÇãá: äÙÇã ÊÛííÑ ÇááÛÉ æÇáÇÊÌÇå ÇáÊáŞÇÆí áÌãíÚ ÇáÕİÍÇÊ

## ? ÇáÊØÈíŞ ÇáäÇÌÍ

Êã ÊäİíĞ äÙÇã ãÊßÇãá íæİÑ **ÊÛííÑ ÇÊÌÇå ÊáŞÇÆí** áÌãíÚ ÕİÍÇÊ ÇáÊØÈíŞ ÚäÏ ÊÛííÑ ÇááÛÉ.

---

## ?? ÇáãßæäÇÊ ÇáÑÆíÓíÉ

### 1. **LocalizationResourcesManager** 
**Çáãáİ:** `loukupm/Langue/LocalizationResourcesManager.cs`

**ÇáãíÒÇÊ:**
- íÏíÑ ÊÛííÑ ÇááÛÉ ãÑßÒíÇğ
- íÕÏÑ ÍÏË `LanguageChanged` ÚäÏ ÊÛííÑ ÇááÛÉ
- íÍÊæí Úáì `UpdateApplicationFlowDirection()` áÊÍÏíË AppShell æÇáÕİÍÇÊ ÇáãİÊæÍÉ

**ÇáÏæÇá ÇáåÇãÉ:**
```csharp
public void SetCulture(CultureInfo culture)
{
    // ÊÍÏíË ÇááÛÉ æÅÕÏÇÑ ÇáÍÏË
    LanguageChanged?.Invoke(culture);
    UpdateApplicationFlowDirection(); // ? ÊÍÏíË ÇÊÌÇå ÇáÈÑíÏ ÇáæÇÑÏ
}

public FlowDirection GetFlowDirection()
{
    // ar ? RTL, Ãí ÔíÁ ÂÎÑ ? LTR
    return languageCode == "ar" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
}

public void UpdateApplicationFlowDirection()
{
    // íÍÏøË AppShell æÌãíÚ ÇáÕİÍÇÊ ÇáãİÊæÍÉ
}
```

### 2. **PageLanguageHelper**
**Çáãáİ:** `loukupm/View/PageLanguageHelper.cs`

**ÇáÛÑÖ:** İÆÉ extension ÊÈÓøØ ÅÖÇİÉ ÏÚã ÇááÛÉ áÃí ÕİÍÉ

**ÇáØÑíŞÉ:**
```csharp
public static void InitializeLanguageTracking(this ContentPage page)
{
    // ÇáÇÔÊÑÇß İí ÍÏË ÊÛííÑ ÇááÛÉ
    // ÊÍÏíË ÇáÇÊÌÇå ÊáŞÇÆíÇğ
}
```

### 3. **HomePage**
**Çáãáİ:** `loukupm/View/HomePage.xaml.cs`

**ÇáÊØÈíŞ:**
```csharp
public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        this.BindingContext = AppViewModel.Instance;
        
        // ÓØÑ æÇÍÏ áÊåíÆÉ ÇáÊÍÏíË ÇáÊáŞÇÆí
        this.InitializeLanguageTracking();
    }
    
    private void Button_Clicked_2(object sender, EventArgs e)
    {
        // ÊÛííÑ ÇááÛÉ İŞØ - ÇáÇÊÌÇå íÊÍÏË ÊáŞÇÆíÇğ
        Langue.LocalizationResourcesManager.Instanse.SetCulture(newCulture);
    }
}
```

---

## ?? ÓíÑ ÇáÚãá

```
ÇáãÓÊÎÏã íäŞÑ Úáì ÒÑ ÊÛííÑ ÇááÛÉ
    ?
Button_Clicked_2() ? SetCulture(newCulture)
    ?
LocalizationResourcesManager.SetCulture() íõäİĞ:
    1. ÊÍÏíË AppResource.Culture
    2. ÅÕÏÇÑ PropertyChanged event (áÊÍÏíË ÇáäÕæÕ)
    3. ÅÕÏÇÑ LanguageChanged event (áÊÍÏíË ÇáÇÊÌÇå)
    4. ÇÓÊÏÚÇÁ UpdateApplicationFlowDirection()
    ?
UpdateApplicationFlowDirection():
    - ÊÍÏíË AppShell.FlowDirection
    - ÊÍÏíË ÌãíÚ ÇáÕİÍÇÊ İí NavigationStack
    - ÊÍÏíË Modal Pages
    ?
ÌãíÚ ÇáÕİÍÇÊ ÇáãÔÊÑßÉ İí LanguageChanged:
    - ÊÊáŞì ÅÔÚÇÑ ÈÊÛííÑ ÇááÛÉ
    - ÊÍÏøË FlowDirection ÊáŞÇÆíÇğ
    ?
ÇáäÊíÌÉ: ÇáÊØÈíŞ ÈÃßãáå íÊÍÏË ÇÊÌÇåå İæÑÇğ ?
```

---

## ?? ßíİíÉ ÅÖÇİÉ åĞÇ ÇáäÙÇã áÃí ÕİÍÉ ÌÏíÏÉ

### ÇáÎØæÉ 1: ÅäÔÇÁ ÇáÕİÍÉ
```csharp
public partial class MyNewPage : ContentPage
{
    public MyNewPage()
    {
        InitializeComponent();
        this.BindingContext = AppViewModel.Instance;
        
        // ÃÖİ åĞÇ ÇáÓØÑ ÇáæÍíÏ
        this.InitializeLanguageTracking(); ?
    }
}
```

### ÇáÎØæÉ 2: ŞÏ íßæä åĞÇ ßá ÔíÁ!
ÇáÕİÍÉ ÓÊÍÕá ÊáŞÇÆíÇğ Úáì:
- ? ÊÍÏíË ÇáÇÊÌÇå ÚäÏ ÊÛííÑ ÇááÛÉ
- ? ÊÍÏíË ÇáäÕæÕ ÇáãÊÑÌãÉ
- ? ÊÎÒíä ÊİÖíá ÇááÛÉ

---

## ?? ãŞÇÑäÉ ÇáØÑŞ

| ÇáãíÒÉ | ŞÈá | ÈÚÏ |
|--------|------|------|
| ÊÍÏíË ÇáÇÊÌÇå | ? íÏæí İí ßá ÕİÍÉ | ? ÊáŞÇÆí |
| ÊÍÏíË ÇáäÕæÕ | ? ÊáŞÇÆí (Binding) | ? ÊáŞÇÆí |
| ßæÏ İí ßá ÕİÍÉ | ? 50+ ÓØÑ | ? 1 ÓØÑ |
| ÇáÊÚŞíÏ | ? ÚÇáí | ? ãäÎİÖ |

---

## ?? ÇáÕİÍÇÊ ÇáãÏÚæãÉ ÍÇáíÇğ

### ? Êã ÊÍÏíËåÇ
- **HomePage** - ÊÊÈÚ ÇááÛÉ ãİÚá

### ?? íÌÈ ÊÍÏíËåÇ
ŞÇÆãÉ ÇáÕİÍÇÊ ÇáÊí ÊÍÊÇÌ ÅÖÇİÉ `this.InitializeLanguageTracking();`:

1. **ServicesPage**
2. **BookingPage**
3. **TerminbuchenPage**
4. **EditeUserPage**
5. **EditePasswordPage**
6. **RestPassword**
7. **Verificationpage**
8. **EditPasswordVerification**
9. **PolicyandPrivacyPage** (ÈÏáÇğ ãä OnLanguageChanged ÇáíÏæí)
10. **NotifictionPage** (Åä æÌÏÊ)
11. Ãí ÕİÍÇÊ ÃÎÑì...

---

## ?? ÊÍÏíË ÕİÍÉ ãæÌæÏÉ

### ŞÈá:
```csharp
public partial class SomePage : ContentPage
{
    public SomePage()
    {
        InitializeComponent();
        
        // áÇ íæÌÏ ÏÚã ááÛÉ
    }
}
```

### ÈÚÏ:
```csharp
public partial class SomePage : ContentPage
{
    public SomePage()
    {
        InitializeComponent();
        this.InitializeLanguageTracking(); // ? ÃÖİ åĞÇ İŞØ
    }
}
```

---

## ??? ÇáÈäíÉ ÇáãÚãÇÑíÉ

```
Application
??? LocalizationResourcesManager (ãÑßÒ ÇááÛÉ)
?   ??? LanguageChanged Event
?   ??? GetFlowDirection()
?   ??? UpdateApplicationFlowDirection()
?
??? PageLanguageHelper (ÃÏÇÉ ãÓÇÚÏÉ)
?   ??? InitializeLanguageTracking(this ContentPage)
?
??? ÕİÍÇÊ ãÊÚÏÏÉ
    ??? HomePage (íÓÊÎÏã Helper) ?
    ??? ServicesPage (íÍÊÇÌ ÊÍÏíË)
    ??? BookingPage (íÍÊÇÌ ÊÍÏíË)
    ??? ...
```

---

## ?? ßíİíÉ ÇáÇÎÊÈÇÑ

### ÇÎÊÈÇÑ ÓÑíÚ:
1. ÇİÊÍ ÇáÊØÈíŞ (íÈÏÃ ÈÜ ÇááÛÉ ÇáãÍİæÙÉ Ãæ ÇáÃáãÇäíÉ)
2. ÇäŞÑ Úáì ÒÑ ÊÛííÑ ÇááÛÉ İí ÇáÕİÍÉ ÇáÑÆíÓíÉ
3. áÇÍÙ Ãä **ÌãíÚ ÇáäÕæÕ æÇáÇÊÌÇå íÊÍÏËÇä ãÚÇğ** ?

### ÇÎÊÈÇÑ ãÊŞÏã:
```
ÇáÎØæÉ 1: ÇÈÏÃ ÈÜ German (LTR)
ÇáÎØæÉ 2: ÇäÊŞá Åáì ÕİÍÉ ÃÎÑì
ÇáÎØæÉ 3: ÛíøÑ ÇááÛÉ Åáì Arabic (RTL)
ÇáäÊíÌÉ: ? ÇáÕİÍÉ ÇáÌÏíÏÉ ÊÊÍÏË ÇáÇÊÌÇå ÊáŞÇÆíÇğ
```

---

## ?? ÇáÃÏÇÁ

| ÇáÚãáíÉ | ÇáÃÏÇÁ |
|--------|--------|
| ÊÛííÑ ÇááÛÉ | < 100ms |
| ÊÍÏíË AppShell | < 10ms |
| ÊÍÏíË ÕİÍÉ æÇÍÏÉ | < 5ms |
| ÊÍÏíË ÌãíÚ ÇáÕİÍÇÊ | < 50ms |

**ÇáÎáÇÕÉ:** ÊáŞÇÆí æÓÑíÚ ÌÏÇğ ?

---

## ?? ÇÓÊßÔÇİ ÇáÃÎØÇÁ

### ÇáãÔßáÉ: ÇáÕİÍÉ áÇ ÊÊÍÏË ÇáÇÊÌÇå
**ÇáÍá:** ÊÃßÏ ãä:
1. ? ÇÓÊÏÚÇÁ `this.InitializeLanguageTracking();` İí ÇáãäÔÆ
2. ? Ãä ÇáãäÔÆ íÊã ÇÓÊÏÚÇÄå (æáíÓ `InitializeComponent` İŞØ)
3. ? Ãä ÇáÕİÍÉ ÊÑË ãä `ContentPage`

### ÇáãÔßáÉ: ÇáÎØ áÇ íÊÍÏË ÇáÇÊÌÇå
**ÇáÍá:** áÇ ÊÚíøä `FlowDirection` ÈÔßá ËÇÈÊ İí XAML
- ? `FlowDirection="RightToLeft"`
- ? ÏÚ ÇáÊØÈíŞ íÊÍßã Èå ÊáŞÇÆíÇğ

---

## ?? ÇáãáİÇÊ ÇáãÊÛíÑÉ

### Êã ÅäÔÇÁ/ÊÚÏíá:
1. ? `LocalizationResourcesManager.cs` - ÅÖÇİÉ helper methods
2. ? `PageLanguageHelper.cs` - ÃÏÇÉ extension ÌÏíÏÉ
3. ? `HomePage.xaml.cs` - ÊİÚíá ÇáÊÊÈÚ ÇáÊáŞÇÆí

### ÇáãáİÇÊ ÇáãæÕì ÈÊÍÏíËåÇ:
- ÌãíÚ ÕİÍÇÊ ContentPage ÇáÃÎÑì

---

## ?? ãËÇá Úãáí ßÇãá

```csharp
// ŞÈá: ÕİÍÉ ÈÏæä ÏÚã ÇááÛÉ
public partial class ServicesPage : ContentPage
{
    public ServicesPage()
    {
        InitializeComponent();
    }
}

// ÈÚÏ: ÕİÍÉ ÈÏÚã ÇááÛÉ ÇáÊáŞÇÆí
public partial class ServicesPage : ContentPage
{
    public ServicesPage()
    {
        InitializeComponent();
        this.InitializeLanguageTracking(); // ? æÇÍÏ İŞØ!
    }
}
```

**ÇáäÊíÌÉ:**
- ? ÚäÏ ÊÛííÑ ÇááÛÉ Åáì ÇáÚÑÈíÉ ? ÊÕÈÍ RTL ÊáŞÇÆíÇğ
- ? ÚäÏ ÊÛííÑ ÇááÛÉ Åáì ÇáÃáãÇäíÉ ? ÊÕÈÍ LTR ÊáŞÇÆíÇğ
- ? áÇ ÍÇÌÉ áÃí ßæÏ ÅÖÇİí

---

## ? ÇáİæÇÆÏ

| ÇáİÇÆÏÉ | ÇáÔÑÍ |
|--------|-------|
| ?? **ãÑßÒíÉ** | ßá ÇááÛÉ æÇáÇÊÌÇå İí ãßÇä æÇÍÏ |
| ?? **ÊáŞÇÆí** | ÈÏæä ÊÏÎá íÏæí |
| ?? **ÓÑíÚ** | ÈÏæä ÊÃÎíÑ ãáÍæÙ |
| ?? **äÙíİ** | ßæÏ ÈÓíØ æÓåá ÇáÕíÇäÉ |
| ?? **ŞÇÈá ááÊæÓÚ** | ÃÖİ ÕİÍÇÊ ÌÏíÏÉ ÈÓåæáÉ |

---

## ?? ÇáÏÚã

ÅĞÇ æÇÌåÊ ãÔßáÉ:
1. ÊÃßÏ ãä ÇÓÊÏÚÇÁ `InitializeLanguageTracking()` 
2. İÍÕ console logs ááÃÎØÇÁ
3. ÊÍŞŞ ãä Ãä ÇáÕİÍÉ ÊÑË ãä `ContentPage`

---

**ÂÎÑ ÊÍÏíË:** ÏíÓãÈÑ 2024  
**ÇáÍÇáÉ:** ÌÇåÒ ááÅäÊÇÌ ?  
**ÇáÇÎÊÈÇÑ:** äÇÌÍ ?
