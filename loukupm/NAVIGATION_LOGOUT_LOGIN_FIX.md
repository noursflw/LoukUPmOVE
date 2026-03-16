# ? Navigation Fix: Logout ? Login ? ProfilePage

## ÇáãÔßáÉ ??
ÈÚÏ ÊÓÌíá ÇáÎÑæÌ (Logout) æÊÓÌíá ÇáÏÎæá ãÑÉ ÃÎÑì (Login)¡ ÚäÏ ãÍÇæáÉ ÇáÇäÊŞÇá Åáì ÕİÍÉ ProfilePage ãä ÇáÊÇÈÇÊ¡ ÇáÊØÈíŞ íÃÎĞß ãÑÉ ÃÎÑì Åáì ÕİÍÉ ÊÓÌíá ÇáÏÎæá (LoginPage) ÈÏáÇğ ãä ÚÑÖ ProfilePage.

## ÇáÓÈÈ ÇáÑÆíÓí ??
1. **ÇáÊÍŞŞ ÇáãÊßÑÑ ãä ÇáãÕÇÏŞÉ**: ÏÇáÉ `CheckAuthentication()` İí `App.xaml.cs` ßÇäÊ ÊÚãá İí ßá ãÑÉ íÊã İíåÇ ÊÍãíá ÇáÊØÈíŞ
2. **ÚÏã ÊÊÈÚ ÍÇáÉ ÇáãÕÇÏŞÉ**: áã íßä åäÇß Úáã (flag) íÊÊÈÚ ãÇ ÅĞÇ ÊãÊ ÇáãÕÇÏŞÉ ÈÇáİÚá
3. **ãÔßáÉ İí ÇáãáÇÍ ÈÚÏ ÇáÏÎæá**: ÈÚÏ ÊÓÌíá ÇáÏÎæá ÇáäÇÌÍ¡ ÍÇáÉ Shell ÇáÏÇÎáíÉ áã Êßä ãåíÃÉ ÈÔßá ÕÍíÍ

## ÇáÍá ÇáãØÈŞ ?

### 1. İí `App.xaml.cs`:
```csharp
// ÅÖÇİÉ Úáã áÊÊÈÚ ÍÇáÉ ÇáãÕÇÏŞÉ
private static bool _authenticationChecked = false;

// ÏÇáÉ áÅÚÇÏÉ ÊÚííä ÇáÚáã ÚäÏ ÊÓÌíá ÇáÎÑæÌ
public static void ResetAuthenticationCheck()
{
    _authenticationChecked = false;
}

// ÊÚÏíá CheckAuthentication() ááÊÍŞŞ ãä ÇáÚáã
private async Task CheckAuthentication()
{
    // Skip if already checked and user is authenticated
    if (_authenticationChecked)
    {
        Console.WriteLine("? Authentication already checked, skipping...");
        return;
    }
    
    // ... ÈÇŞí ÇáßæÏ
    
    // Mark as checked when token found
    _authenticationChecked = true;
    await Shell.Current.GoToAsync("//HomePage");
}
```

### 2. İí `ProfilePage.xaml.cs`:
```csharp
// İí ÏÇáÉ Logout
App.ResetAuthenticationCheck();
NavigationService.ClearPageSourceMap();
```

### 3. İí `LoginPage.xaml.cs`:
```csharp
// ÊÕÍíÍ ÇáÊäŞá ÈÚÏ ÇáÏÎæá ÇáäÇÌÍ
await Shell.Current.GoToAsync("//HomePage");
// ÈÏáÇğ ãä: await Shell.Current.GoToAsync($"///{nameof(HomePage)}");
```

## ÇáäÊíÌÉ ??
- ? íãßäß ÊÓÌíá ÇáÎÑæÌ ÈÏæä ãÔÇßá
- ? íãßäß ÊÓÌíá ÇáÏÎæá ãÑÉ ÃÎÑì ÈÏæä ãÔÇßá
- ? ÈÚÏ ÊÓÌíá ÇáÏÎæá¡ íãßäß ÇáæÕæá Åáì ÌãíÚ ÇáÊÇÈÇÊ (HomePage, ServicesPage, BookingPage, ProfilePage)
- ? íãßäß ÇáÇäÊŞÇá Åáì ProfilePage ãÈÇÔÑÉ ãä ÇáÊÇÈÇÊ ÇáÃÎÑì

## ÇáÊİÇÕíá ÇáÊŞäíÉ ??

| ÇáãÔßáÉ | ÇáÓÈÈ | ÇáÍá |
|--------|-------|------|
| `CheckAuthentication()` ÊÚãá ÈÚÏ ÇáÏÎæá | ÚÏã æÌæÏ Úáã áÊÊÈÚ ÇáÍÇáÉ | ÅÖÇİÉ `_authenticationChecked` flag |
| ÊÖÇÑÈ İí ÍÇáÉ Shell | ÚÏã ÅÚÇÏÉ ÊÚííä ÇáÚáã ÚäÏ ÇáÎÑæÌ | ÇÓÊÏÚÇÁ `App.ResetAuthenticationCheck()` İí ÇáÎÑæÌ |
| ãáÇÍ ÎÇØÆ ÈÚÏ ÇáÏÎæá | ÇÓÊÎÏÇã routing syntax ÎÇØÆ | ÊÕÍíÍ `"//HomePage"` ÈÏáÇğ ãä `$"///{nameof(HomePage)}"`  |

## ÇáãáİÇÊ ÇáãÚÏáÉ ??
- ? `loukupm/App.xaml.cs` - ÅÖÇİÉ Úáã ÊÊÈÚ ÇáãÕÇÏŞÉ
- ? `loukupm/View/ProfilePage.xaml.cs` - ÇÓÊÏÚÇÁ ResetAuthenticationCheck() ÚäÏ ÇáÎÑæÌ
- ? `loukupm/View/LoginPage.xaml.cs` - ÊÕÍíÍ routing ÈÚÏ ÇáÏÎæá ÇáäÇÌÍ
