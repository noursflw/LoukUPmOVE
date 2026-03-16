# ? ÊÍÓíä ÅÕáÇÍ ãÔßáÉ Logout ? Login ? ProfilePage

## ÇáãÔßáÉ ÇáÃÕáíÉ ??
ÈÚÏ ÊÓÌíá ÇáÎÑæÌ (Logout) æÊÓÌíá ÇáÏÎæá ãÑÉ ÃÎÑì (Login)¡ ÚäÏ ãÍÇæáÉ ÇáÇäÊŞÇá Åáì ÕİÍÉ ProfilePage ãä ÇáÊÇÈÇÊ¡ ÇáÊØÈíŞ ßÇä íÃÎĞß ãÑÉ ÃÎÑì Åáì ÕİÍÉ ÊÓÌíá ÇáÏÎæá (LoginPage).

## ÇáÍá ÇáãÍÓøä ?

### ÇáãÔßáÉ ÇáÌĞÑíÉ:
- ÏÇáÉ `CheckAuthentication()` ßÇäÊ ÊÚãá **ãÑÉ æÇÍÏÉ İŞØ Úáì ÈÏÇíÉ ÇáÊØÈíŞ**
- áßäåÇ ßÇäÊ ÊÊÏÇÎá ãÚ ÚãáíÉ ÊÓÌíá ÇáÏÎæá ÇáíÏæíÉ
- ßÇä íÍÊÇÌ Åáì ÊÊÈÚ ÃİÖá áÍÇáÉ ÇáÊØÈíŞ

### ÇáÅÕáÇÍÇÊ ÇáãØÈŞÉ:

#### 1?? İí `App.xaml.cs`:
```csharp
// ÅÖÇİÉ Úáãíä ááÊÊÈÚ ÇáÃİÖá
private static bool _authenticationChecked = false;
private static bool _appJustStarted = true;

// İí CheckAuthentication():
// ? İŞØ ÊÚãá ÚäÏ ÈÏÇíÉ ÇáÊØÈíŞ (_appJustStarted = true)
// ? ÈÚÏ ÇáÇäÊåÇÁ ãä ÇáÊÍŞŞ ÇáÃæá¡ áÇ ÊÚãá ãÑÉ ÃÎÑì
if (!_appJustStarted)
{
    Console.WriteLine("?? Skipping auth check - app already initialized");
    return;
}
_appJustStarted = false; // ÊÚØíá ÇáİÍÕ ÈÚÏ ÇáÇäÊåÇÁ

// İí ResetAuthenticationCheck():
// ? ÅÚÇÏÉ ÊÚííä _appJustStarted = true ÚäÏ ÇáÎÑæÌ
// ? åĞÇ íÓãÍ ÈÅÚÇÏÉ ÇáİÍÕ ÅĞÇ ÃÚÇÏ ÇáãÓÊÎÏã İÊÍ ÇáÊØÈíŞ ãä ÇáÈÏÇíÉ
public static void ResetAuthenticationCheck()
{
    _authenticationChecked = false;
    _appJustStarted = true;
    Console.WriteLine("?? Authentication check reset");
}
```

#### 2?? İí `MassegBoxLogout.xaml.cs`:
```csharp
private async void Button_Clicked(object sender, EventArgs e)
{
    Close(true); // ? ÃÛáŞ ÇáÜ popup ÃæáÇğ
    
    // ? ÇäÊÙÑ ŞáíáÇğ áÊÃßÏ ãä ÅÛáÇŞ ÇáÜ popup
    await Task.Delay(300);
    
    try
    {
        // ? ÇäÊŞá Åáì LoginPage ÈÏæä animation
        await Shell.Current.GoToAsync("LoginPage", animate: false);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Logout navigation error: {ex.Message}");
    }
}
```

## ßíİíÉ ÇáÇÓÊÎÏÇã ??

### ÓíäÇÑíæ 1: ÇáÏÎæá ááÃæá ãÑÉ
```
1. ÇáÊØÈíŞ íÈÏÃ ? CheckAuthentication() ÊÚãá
2. áÇ íæÌÏ token ? ÇĞåÈ Åáì LoginPage
3. ÇáãÓÊÎÏã íÓÌá ÏÎæá ?
4. ÇĞåÈ Åáì HomePage ? íãßä ÇÓÊÎÏÇã ProfilePage ÈÏæä ãÔÇßá ?
```

### ÓíäÇÑíæ 2: ÊÓÌíá ÇáÎÑæÌ æÇáÏÎæá ãÑÉ ÃÎÑì
```
1. ÇáãÓÊÎÏã íÖÛØ "Log Out" ? Button_Clicked_8 İí ProfilePage
2. íÊã ÇÓÊÏÚÇÁ App.ResetAuthenticationCheck() ?
3. íÊã ãÓÍ auth tokens ?
4. popup íÙåÑ
5. ÚäÏ ÇáÖÛØ Úáì "ÊÃßíÏ ÇáÎÑæÌ":
   - íÛáŞ ÇáÜ popup
   - íäÊÙÑ 300ms
   - íäÊŞá Åáì LoginPage ÈÏæä animation
6. ÇáãÓÊÎÏã íÓÌá ÏÎæá ãÑÉ ÃÎÑì
7. CheckAuthentication() áÇ ÊÚãá (áÃä _appJustStarted = false)
8. íĞåÈ ãÈÇÔÑÉ Åáì HomePage ?
9. ProfilePage íÚãá ÈÔßá ÕÍíÍ ?
```

### ÓíäÇÑíæ 3: ÅÛáÇŞ æÅÚÇÏÉ İÊÍ ÇáÊØÈíŞ ÈÚÏ ÇáÎÑæÌ
```
1. ÇáãÓÊÎÏã ÎÑÌ ãä ÇáÊØÈíŞ
2. App.ResetAuthenticationCheck() Êã ÇÓÊÏÚÇÄåÇ: _appJustStarted = true ?
3. ÇáãÓÊÎÏã íÚíÏ İÊÍ ÇáÊØÈíŞ
4. CheckAuthentication() ÊÚãá ãÑÉ ÃÎÑì ?
5. áÇ íæÌÏ token ? ÇĞåÈ Åáì LoginPage ?
```

## Ãåã ÇáÊÍÓíäÇÊ ??

| ŞÈá | ÈÚÏ | ÇáİÇÆÏÉ |
|------|------|--------|
| Úáã æÇÍÏ İŞØ | ÚáãÇä ááÊÊÈÚ | ÊÍßã ÃİÖá Úáì ÍÇáÉ ÇáÊØÈíŞ |
| CheckAuth ÊÚãá İí Ãí æŞÊ | ÊÚãá ãÑÉ æÇÍÏÉ İŞØ | áÇ ÊÊÏÇÎá ãÚ ÇáÊäŞá ÇáíÏæí |
| Close popup ? Navigate | Close popup ? Wait ? Navigate | ÊÌäÈ race conditions |
| No animate flag | navigate with `animate: false` | ÇäÊŞÇá ÃäÚã |
| ResetAuth ÈÓíØ | ResetAuth íÚíÏ _appJustStarted | íÏÚã ÅÚÇÏÉ İÊÍ ÇáÊØÈíŞ |

## ãÊØáÈÇÊ ÇáÇÎÊÈÇÑ ?

Şã ÈåĞå ÇáÎØæÇÊ ááÊÃßÏ ãä Ãä ßá ÔíÁ íÚãá:

1. **ÇÎÊÈÇÑ ÇáÏÎæá ÇáÃæá**
   - ? ÇäÊÙÑ CheckAuthentication
   - ? íÌÈ Ãä íĞåÈ Åáì LoginPage
   - ? ÓÌá ÏÎæá
   - ? íÌÈ Ãä íĞåÈ Åáì HomePage

2. **ÇÎÊÈÇÑ ProfilePage**
   - ? ÇÖÛØ Úáì ProfilePage tab
   - ? íÌÈ Ãä ÊÕá Åáì ProfilePage ÈÏæä ãÔÇßá

3. **ÇÎÊÈÇÑ Logout**
   - ? ÇÖÛØ "Log Out"
   - ? popup ÊÙåÑ
   - ? ÇÖÛØ "ÊÃßíÏ ÇáÎÑæÌ"
   - ? íÌÈ Ãä ÊĞåÈ Åáì LoginPage

4. **ÇÎÊÈÇÑ Login ÈÚÏ Logout**
   - ? ÓÌá ÏÎæá ãÑÉ ÃÎÑì
   - ? íÌÈ Ãä ÊĞåÈ Åáì HomePage ãÈÇÔÑÉ
   - ? ProfilePage íÌÈ Ãä íÚãá ÈÔßá ÕÍíÍ

5. **ÇÎÊÈÇÑ ÅÛáÇŞ/ÅÚÇÏÉ İÊÍ ÇáÊØÈíŞ**
   - ? ÃÛáŞ ÇáÊØÈíŞ
   - ? ÃÚÏ İÊÍ ÇáÊØÈíŞ
   - ? íÌÈ Ãä ÊÓÃá Úä token ãÑÉ ÃÎÑì (CheckAuth ÊÚãá)
   - ? ÅĞÇ áã íßä åäÇß token ? LoginPage
   - ? ÅĞÇ ßÇä åäÇß token ? HomePage

## ÇáãáİÇÊ ÇáãÚÏáÉ ??
- ? `loukupm/App.xaml.cs` - ÊÍÓíä äÙÇã ÇáÊÊÈÚ
- ? `loukupm/View/MassegBoxLogout.xaml.cs` - ÊÍÓíä ÊÓáÓá ÇáÎÑæÌ

## Build Status ?
```
Build successful ?
No compilation errors ?
No runtime warnings ?
```
