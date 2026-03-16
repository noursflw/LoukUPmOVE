# ?? Debug Script - ›Õ’ Õ«·… «·„·«Õ

≈–« √—œ  ›Õ’ „« ÌÕœÀ »«·÷»ÿ √À‰«¡ «· ÿ»Ìﬁ° √÷› «·ﬂÊœ «· «·Ì:

## Option 1: ›Õ’ «·‹ Flags ›Ì App.xaml.cs

```csharp
// ›Ì App.xaml.cs - √÷› Â–Â «·œ«·… «·⁄«„…:
public static string GetAuthenticationState()
{
    return $"_appJustStarted={_appJustStarted}, _authenticationChecked={_authenticationChecked}";
}
```

À„ ›Ì √Ì „ﬂ«‰  —Ìœ ›Õ’ «·Õ«·…:
```csharp
Console.WriteLine($"?? Auth State: {App.GetAuthenticationState()}");
```

---

## Option 2:   »⁄ ﬂ«„· ·‹ CheckAuthentication

```csharp
// «” »œ· œ«·… CheckAuthentication »Â–« «·ﬂÊœ «·„Ê”¯⁄:
private async Task CheckAuthentication()
{
    try
    {
        Console.WriteLine($"?? [CheckAuthentication START] _appJustStarted={_appJustStarted}");
        
        if (!_appJustStarted)
        {
            Console.WriteLine("?? Skipping auth check - app already initialized");
            return;
        }

        Console.WriteLine("?? Waiting 600ms for UI to load...");
        await Task.Delay(600);

        string token = string.Empty;
        try
        {
            token = await SecureStorage.GetAsync("auth_token");
            Console.WriteLine($"?? Token from SecureStorage: {(string.IsNullOrEmpty(token) ? "EMPTY" : "EXISTS")}");
        }
        catch (Exception storageEx)
        {
            Console.WriteLine($"[SecureStorage ERROR]: {storageEx.Message}");
        }

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("?? No token found ? Navigating to LoginPage");
            _appJustStarted = false;
            await Shell.Current.GoToAsync("LoginPage");
            Console.WriteLine("? Navigated to LoginPage");
        }
        else
        {
            Console.WriteLine("? Token found ? Navigating to HomePage");
            _authenticationChecked = true;
            _appJustStarted = false;
            await Shell.Current.GoToAsync("//HomePage");
            Console.WriteLine("? Navigated to HomePage");
        }
        
        Console.WriteLine($"?? [CheckAuthentication END] State: {GetAuthenticationState()}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[App CheckAuthentication ERROR]: {ex.GetType().Name}: {ex.Message}");
        await Shell.Current.GoToAsync("LoginPage");
    }
}

// √÷› Â–Â «·œ«·… ·⁄—÷ «·Õ«·…:
private static string GetAuthenticationState()
{
    return $"_appJustStarted={_appJustStarted}, _authenticationChecked={_authenticationChecked}";
}
```

---

## Option 3:   »⁄ ﬂ«„· ·‹ MassegBoxLogout

```csharp
// «” »œ· Button_Clicked »Â–« «·ﬂÊœ «·„Ê”¯⁄:
private async void Button_Clicked(object sender, EventArgs e)
{
    try
    {
        Console.WriteLine($"?? [LogoutFlow START]");
        
        Console.WriteLine("?? Step 1: Close popup");
        Close(true);
        
        Console.WriteLine("?? Step 2: Wait 300ms");
        await Task.Delay(300);
        
        Console.WriteLine("?? Step 3: Navigate to LoginPage");
        await Shell.Current.GoToAsync("LoginPage", animate: false);
        
        Console.WriteLine($"?? [LogoutFlow END] Successfully navigated to LoginPage");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? [LogoutFlow ERROR] {ex.GetType().Name}: {ex.Message}");
        Console.WriteLine($"StackTrace: {ex.StackTrace}");
    }
}
```

---

## Option 4:   »⁄ ﬂ«„· ·‹ ProfilePage Logout

```csharp
// «” »œ· Button_Clicked_8 »Â–« «·ﬂÊœ «·„Ê”¯⁄:
private async void Button_Clicked_8(object sender, EventArgs e)
{
    try
    {
        Console.WriteLine($"?? [LogoutButton START]");
        
        Console.WriteLine("1?? Calling OneSignalService.Logout()");
        OneSignalService.Logout();
        
        Console.WriteLine("2?? Removing auth_token");
        SecureStorage.Remove("auth_token");
        
        Console.WriteLine("3?? Removing refresh_token");
        SecureStorage.Remove("refresh_token");
        
        Console.WriteLine("4?? Clearing PageSourceMap");
        NavigationService.ClearPageSourceMap();
        
        Console.WriteLine("5?? Resetting authentication check");
        App.ResetAuthenticationCheck();
        
        Console.WriteLine("6?? Showing logout popup");
        var popup = new MassegBoxLogout();
        await this.ShowPopupAsync(popup);
        
        Console.WriteLine($"?? [LogoutButton END]");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? [LogoutButton ERROR] {ex.GetType().Name}: {ex.Message}");
    }
}
```

---

## Option 5: Console Output Monitor

√÷› Â–« «·ﬂÊœ ›Ì LoginPage ·  »⁄ «·œŒÊ·:

```csharp
// ›Ì OnLoginClicked - »⁄œ «·œŒÊ· «·‰«ÃÕ:
if (response.IsSuccessStatusCode)
{
    Console.WriteLine("?? [LoginSuccess] Deserializing response");
    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(result,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    Console.WriteLine("?? [LoginSuccess] Saving tokens");
    if (!string.IsNullOrEmpty(loginResponse?.Token))
        await SecureStorage.SetAsync("auth_token", loginResponse.Token);

    if (!string.IsNullOrEmpty(loginResponse?.Refresh_Token))
        await SecureStorage.SetAsync("refresh_token", loginResponse.Refresh_Token);
    
    Console.WriteLine("?? [LoginSuccess] Showing success popup");
    var popup = new CompletedLogin();
    await this.ShowPopupAsync(popup);
    
    Console.WriteLine("?? [LoginSuccess] Navigating to HomePage");
    await Shell.Current.GoToAsync("//HomePage");
    
    Console.WriteLine("? [LoginSuccess] Navigation to HomePage completed");

    // OneSignal setup...
    if (loginResponse?.User != null)
    {
        var userId = loginResponse.User.Id.ToString();
        Console.WriteLine($"?? [LoginSuccess] OneSignal login for user {userId}");
        OneSignal.Login(userId);
        OneSignal.User.AddTag("user_no", userId);
    }
}
```

---

## Expected Console Output

### «·”Ì‰«—ÌÊ 1: › Õ «· ÿ»Ìﬁ »œÊ‰ Token
```
?? [CheckAuthentication START] _appJustStarted=True
?? Waiting 600ms for UI to load...
?? Token from SecureStorage: EMPTY
?? No token found ? Navigating to LoginPage
? Navigated to LoginPage
?? [CheckAuthentication END] State: _appJustStarted=False, _authenticationChecked=False
```

### «·”Ì‰«—ÌÊ 2:  ”ÃÌ· œŒÊ· ‰«ÃÕ
```
?? [LoginSuccess] Deserializing response
?? [LoginSuccess] Saving tokens
?? [LoginSuccess] Showing success popup
?? [LoginSuccess] Navigating to HomePage
? [LoginSuccess] Navigation to HomePage completed
?? [LoginSuccess] OneSignal login for user 123
```

### «·”Ì‰«—ÌÊ 3:  ”ÃÌ· Œ—ÊÃ
```
?? [LogoutButton START]
1?? Calling OneSignalService.Logout()
2?? Removing auth_token
3?? Removing refresh_token
4?? Clearing PageSourceMap
5?? Resetting authentication check
?? Authentication check reset
6?? Showing logout popup
?? [LogoutButton END]
```

### «·”Ì‰«—ÌÊ 4: «·÷€ÿ ⁄·Ï  √ﬂÌœ «·Œ—ÊÃ
```
?? [LogoutFlow START]
?? Step 1: Close popup
?? Step 2: Wait 300ms
?? Step 3: Navigate to LoginPage
? Successfully navigated to LoginPage
?? [LogoutFlow END]
```

### «·”Ì‰«—ÌÊ 5: œŒÊ· »⁄œ «·Œ—ÊÃ
```
?? [CheckAuthentication START] _appJustStarted=False
?? Skipping auth check - app already initialized
?? [LoginSuccess] Deserializing response
?? [LoginSuccess] Saving tokens
?? [LoginSuccess] Showing success popup
?? [LoginSuccess] Navigating to HomePage
? [LoginSuccess] Navigation to HomePage completed
?? [LoginSuccess] OneSignal login for user 123
```

---

## ﬂÌ›Ì… «·«” Œœ«„

1. «Œ — √Õœ «·‹ Options √⁄·«Â
2. «‰”Œ «·ﬂÊœ
3. ÷⁄Â ›Ì «·„·› «·„‰«”»
4. ‘€¯· «· ÿ»Ìﬁ
5. «› Õ Debug Output: `Debug ? Windows ? Output`
6. ﬁ„ »ŒÿÊ«  «·«Œ »«—
7. «‰”Œ ﬂ· «·—”«∆· „‰ Console
8. «ﬁ«—‰Â« „⁄ Expected Output

---

## ⁄·«„«  «·Œÿ√ ??

≈–« —√Ì :
- `? [LogoutFlow ERROR]` ? „‘ﬂ·… ›Ì «·„·«Õ √À‰«¡ «·Œ—ÊÃ
- `? [LogoutButton ERROR]` ? „‘ﬂ·… ›Ì „⁄«·Ã «·Œ—ÊÃ
- `[SecureStorage ERROR]` ? „‘ﬂ·… ›Ì «·—„Ê“ «·„Õ›ÊŸ…
- `[App CheckAuthentication ERROR]` ? „‘ﬂ·… ›Ì «·›Õ’ «·√Ê·Ì

«ﬁ—√ «·—”«·… «·œﬁÌﬁ… Ê√—”·Â« ··„ÿÊ—Ì‰.
