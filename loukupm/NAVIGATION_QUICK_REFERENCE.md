# ?? Quick Reference - Navigation Service Usage

## Why Your Navigation Failed in Release Mode

**The Problem:**
- Debug mode uses reflection to find routes ? Works with string literals
- Release mode doesn't have reflection metadata ? String literals fail silently
- You weren't registering all your pages properly

**The Solution:**
- Type-safe constants instead of strings
- Complete route registration (17 pages now, not 5)
- Centralized validation

---

## All Available Routes

```csharp
// ============ AUTH PAGES ============
NavigationService.ROUTE_LOGIN              // LoginPage
NavigationService.ROUTE_SIGNIN             // SinginPage
NavigationService.ROUTE_MAIN_PAGE          // MainPage

// ============ TABBAR PAGES ============
NavigationService.ROUTE_HOME               // HomePage
NavigationService.ROUTE_SERVICES           // ServicesPage
NavigationService.ROUTE_BOOKING            // BookingPage
NavigationService.ROUTE_PROFILE            // ProfilePage

// ============ HIDDEN PAGES ============
NavigationService.ROUTE_TERM_BOOKING       // TerminbuchenPage
NavigationService.ROUTE_PAYMENT            // Paymentgetway

// ============ POLICY/TERMS ============
NavigationService.ROUTE_POLICY_PRIVACY     // PolicyandPrivacyPage
NavigationService.ROUTE_REST_PASSWORD      // RestPassword
NavigationService.ROUTE_TERMS_CONDITIONS   // TermsAndConditions

// ============ PROFILE SECTION ============
NavigationService.ROUTE_EDIT_USER          // EditeUserPage
NavigationService.ROUTE_EDIT_PASSWORD      // EditePasswordPage
NavigationService.ROUTE_ABOUT_US           // AboutUS
NavigationService.ROUTE_NOTIFICATION       // NotifictionPage
NavigationService.ROUTE_SETTING            // SettingPage
```

---

## Navigation Rules

### Rule 1: TabBar Pages vs Hidden Pages

**TabBar Pages** (use `NavigateToTabBarPage()`):
- HomePage, ServicesPage, BookingPage, ProfilePage
- These are inside the TabBar, visible in the app structure

```csharp
// ? CORRECT
await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_HOME);

// ? WRONG
await NavigationService.NavigateToPage(NavigationService.ROUTE_HOME);
```

**Hidden Pages** (use `NavigateToPage()`):
- Everything else (EditUserPage, TermsAndConditions, etc.)
- These appear as modals/overlays

```csharp
// ? CORRECT
await NavigationService.NavigateToPage(NavigationService.ROUTE_EDIT_USER);

// ? WRONG
await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_EDIT_USER);
```

---

## Common Navigation Patterns

### Pattern 1: Button Click ? Navigate

```csharp
private async void OnEditProfileClicked(object sender, EventArgs e)
{
    // If EditUserPage can only be accessed from ProfilePage:
    await NavigationService.NavigateToPage(NavigationService.ROUTE_EDIT_USER);
}
```

### Pattern 2: Back Button

```csharp
protected override bool OnBackButtonPressed()
{
    // NavigationService handles the back logic automatically
    _ = NavigationService.HandleBackButton(NavigationService.ROUTE_EDIT_USER);
    return true;
}
```

### Pattern 3: Multi-Source Navigation

When a page can be accessed from multiple places:

```csharp
// Register where we came from
NavigationService.RegisterPageSource(
    NavigationService.ROUTE_EDIT_USER, 
    NavigationService.ROUTE_PROFILE  // or ROUTE_HOME, etc.
);

// Then navigate
await NavigationService.NavigateToPage(NavigationService.ROUTE_EDIT_USER);

// When user presses back, it automatically goes to the registered source
```

### Pattern 4: Logout Flow

```csharp
private async void OnLogoutClicked(object sender, EventArgs e)
{
    // Clear any pending navigation states
    NavigationService.ClearPageSourceMap();
    
    // Clear local data
    SecureStorage.Remove("auth_token");
    SecureStorage.Remove("refresh_token");
    
    // Navigate to login with cleared stack
    await ShellNavigationManager.NavigateToLoginAndClear();
}
```

### Pattern 5: Login Flow

```csharp
private async void OnLoginSuccess(object sender, EventArgs e)
{
    // Save token
    await SecureStorage.SetAsync("auth_token", token);
    
    // Navigate home with cleared stack
    await ShellNavigationManager.NavigateToHomeAndClear();
}
```

---

## Complete Example: ProfilePage

```csharp
using loukupm.Services;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    // Edit user button
    private async void EditUserClicked(object sender, EventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_EDIT_USER);
    }

    // Edit password button
    private async void EditPasswordClicked(object sender, EventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_EDIT_PASSWORD);
    }

    // View settings
    private async void SettingsClicked(object sender, EventArgs e)
    {
        await NavigationService.NavigateToPage(NavigationService.ROUTE_SETTING);
    }

    // Back button
    protected override bool OnBackButtonPressed()
    {
        // Stay on ProfilePage if user presses back
        NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_PROFILE);
        return true;
    }

    // Logout button
    private async void LogoutClicked(object sender, EventArgs e)
    {
        NavigationService.ClearPageSourceMap();
        SecureStorage.Remove("auth_token");
        await ShellNavigationManager.NavigateToLoginAndClear();
    }
}
```

---

## Debugging Tips

### Enable Navigation Logging

In your Release build, check the Debug Output window for messages like:

```
? [AppShell] All routes registered successfully
? [AppShell] Navigation validation PASSED
?? [Navigation] Navigating to TabBar page: HomePage
?? [Navigation] Navigating to page: EditUserPage
?? [Navigation] Back from EditUserPage to ProfilePage
? [Navigation] INVALID ROUTE: 'BadPageName' - Not registered
```

### Common Issues

**Issue:** Route not found error
```
? [Navigation] INVALID ROUTE: 'MyPage' - Not registered
```
**Fix:** Add route to AppShell.xaml.cs
```csharp
Routing.RegisterRoute(NavigationService.ROUTE_MYPAGE, typeof(MyPage));
```

**Issue:** Navigation appears to do nothing
**Fix:** Check if using correct method (TabBar vs Page)
```csharp
// If it's a TabBar page, use this:
await NavigationService.NavigateToTabBarPage(route);

// If it's hidden, use this:
await NavigationService.NavigateToPage(route);
```

**Issue:** Back button not working
**Fix:** Add mapping in `BackNavigationMap` in NavigationService.cs
```csharp
[ROUTE_YOUR_PAGE] = ROUTE_BACK_TO_PAGE,
```

---

## Adding a New Page

If you create a new page:

1. **Create the page file** (e.g., `NewPage.xaml` and `NewPage.xaml.cs`)

2. **Add to NavigationService.cs constants:**
```csharp
public const string ROUTE_NEW_PAGE = "NewPage";
```

3. **Add to AppShell.xaml.cs route registration:**
```csharp
Routing.RegisterRoute(NavigationService.ROUTE_NEW_PAGE, typeof(NewPage));
```

4. **Use it with NavigationService:**
```csharp
// If it's a hidden page:
await NavigationService.NavigateToPage(NavigationService.ROUTE_NEW_PAGE);

// If it's a TabBar page:
await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_NEW_PAGE);
```

---

## Copy-Paste Templates

### Template 1: Simple Navigation
```csharp
private async void ButtonClicked(object sender, EventArgs e)
{
    await NavigationService.NavigateToPage(NavigationService.ROUTE_TARGET_PAGE);
}
```

### Template 2: Back Button
```csharp
protected override bool OnBackButtonPressed()
{
    _ = NavigationService.HandleBackButton(NavigationService.ROUTE_CURRENT_PAGE);
    return true;
}
```

### Template 3: Dynamic Back Navigation
```csharp
private async void NavigateFromMultipleSources()
{
    NavigationService.RegisterPageSource(
        NavigationService.ROUTE_TARGET_PAGE,
        NavigationService.ROUTE_SOURCE_PAGE
    );
    await NavigationService.NavigateToPage(NavigationService.ROUTE_TARGET_PAGE);
}
```

### Template 4: Logout
```csharp
private async void OnLogout(object sender, EventArgs e)
{
    NavigationService.ClearPageSourceMap();
    SecureStorage.Remove("auth_token");
    SecureStorage.Remove("refresh_token");
    await ShellNavigationManager.NavigateToLoginAndClear();
}
```

---

## Why This Works in Release Mode

? **Explicit registration** - Routes are registered, no reflection needed
? **Constants** - Can't typo a route name
? **Validation** - Invalid routes caught at runtime with clear errors
? **Consistent** - Same code path in Debug and Release
? **Maintainable** - All routes in one place (NavigationService.cs)
? **Type-safe** - IDE can help autocomplete route names

---

## Files Modified

- ? `NavigationService.cs` - Complete rewrite
- ? `AppShell.xaml.cs` - Full route registration
- ? `App.xaml.cs` - Uses NavigationService
- ? `LoginPage.xaml.cs` - All navigation updated
- ? `SinginPage.xaml.cs` - All navigation updated
- ? `ProfilePage.xaml.cs` - All navigation updated
- ? `HomePage.xaml.cs` - All navigation updated

---

**Build Status:** ? SUCCESS  
**Status:** PRODUCTION READY  
**Tested in:** Debug & Release modes
