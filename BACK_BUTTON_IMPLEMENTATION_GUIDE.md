# Back Button Handler - Implementation Pattern

## Quick Reference Guide

### For All Pages

Every page must implement back button handling by delegating to the centralized `NavigationService.HandleBackButton()`.

---

## Pattern 1: Tab Bar Pages

**Applies to:** HomePage, BookingPage, ServicesPage, ProfilePage

```csharp
namespace loukupm.View;
using loukupm.Services;

public partial class BookingPage : ContentPage
{
    public BookingPage()
    {
        InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        // Tab Bar page: Delegate to centralized back button logic
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(NavigationService.ROUTE_BOOKING);
        });
        return true;
    }
}
```

**What happens:**
- HomePage: Returns false → OS exits app
- Any other TabBar page: Navigates to //HomePage

---

## Pattern 2: Profile Flow Pages

**Applies to:** RestPassword, SettingPage, EditeUserPage, EditePasswordPage

```csharp
namespace loukupm.View;
using loukupm.Services;

public partial class EditePasswordPage : ContentPage
{
    public EditePasswordPage()
    {
        InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(NavigationService.ROUTE_EDIT_PASSWORD);
        });
        return true;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        // Any "Back" button in the UI also uses centralized handler
        await NavigationService.HandleBackButton(NavigationService.ROUTE_EDIT_PASSWORD);
    }
}
```

**What happens:**
- ALWAYS navigates to //ProfilePage
- Never uses stack pop (..)

---

## Pattern 3: General Subpages

**Applies to:** TerminbuchenPage, Paymentgetway, TermsAndConditions, PolicyandPrivacyPage, etc.

```csharp
namespace loukupm.View;
using loukupm.Services;

public partial class TerminbuchenPage : ContentPage
{
    public TerminbuchenPage()
    {
        InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(NavigationService.ROUTE_TERM_BOOKING);
        });
        return true;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        // Any "Back" button in the UI also uses centralized handler
        await NavigationService.HandleBackButton(NavigationService.ROUTE_TERM_BOOKING);
    }
}
```

**What happens:**
- Pops exactly one level (..) from the navigation stack

---

## NavigationService.HandleBackButton() - Logic Flow

```
Input: currentPage (route name)
│
├─ Is it a TabBar page?
│  ├─ YES: Is it HomePage?
│  │       ├─ YES → return false (allow OS exit)
│  │       └─ NO → navigate to //HomePage
│  └─ NO: Continue to next check
│
├─ Is it a Profile Flow page?
│  ├─ YES → navigate to //ProfilePage
│  └─ NO: Continue to next check
│
└─ Default: Pop one level (..)

Output: true (handled) or false (let OS handle)
```

---

## Key Implementation Rules

### ✅ DO:

1. **Always use MainThread.BeginInvokeOnMainThread()**
   ```csharp
   MainThread.BeginInvokeOnMainThread(async () =>
   {
       await NavigationService.HandleBackButton(route);
   });
   ```

2. **Always return true from OnBackButtonPressed()**
   ```csharp
   protected override bool OnBackButtonPressed()
   {
       // ... handler code ...
       return true; // Always true - centralized logic handles it
   }
   ```

3. **Use the correct route constant**
   ```csharp
   await NavigationService.HandleBackButton(NavigationService.ROUTE_EDIT_USER);
   // NOT: await NavigationService.HandleBackButton("EditeUserPage");
   // NOT: await NavigationService.HandleBackButton(nameof(EditeUserPage));
   ```

4. **For UI back buttons, use the same handler**
   ```csharp
   private async void BackButton_Clicked(object sender, EventArgs e)
   {
       await NavigationService.HandleBackButton(NavigationService.ROUTE_MY_PAGE);
   }
   ```

### ❌ DON'T:

1. **Don't use Navigation.PopAsync()**
   ```csharp
   // ❌ WRONG
   protected override bool OnBackButtonPressed()
   {
       Navigation.PopAsync();
       return true;
   }
   ```

2. **Don't use Navigation.PushAsync()**
   ```csharp
   // ❌ WRONG
   private async void OnNavigate()
   {
       await Navigation.PushAsync(new SomePage());
   }
   ```

3. **Don't create custom back button logic**
   ```csharp
   // ❌ WRONG
   if (currentPage == "HomePage")
   {
       return false;
   }
   await Shell.Current.GoToAsync("..", animate: true);
   ```

4. **Don't forget to add the using statement**
   ```csharp
   // ❌ WRONG - Missing using
   namespace loukupm.View;

   // ✅ RIGHT
   namespace loukupm.View;
   using loukupm.Services;
   ```

---

## Adding a New Page

If you create a new page, follow these steps:

### Step 1: Add Route Constant
```csharp
// In NavigationService.cs
public const string ROUTE_MY_NEW_PAGE = "MyNewPage";
```

### Step 2: Register Route
```csharp
// In AppShell.cs - RegisterAllRoutes()
Routing.RegisterRoute(NavigationService.ROUTE_MY_NEW_PAGE, typeof(MyNewPage));
```

### Step 3: Add to AllValidRoutes
```csharp
// In NavigationService.cs
private static readonly HashSet<string> AllValidRoutes = new()
{
    // ... existing routes ...
    ROUTE_MY_NEW_PAGE,  // Add here
};
```

### Step 4: If it's a Profile Flow Page, add to ProfileFlowPages
```csharp
// In NavigationService.cs
private static readonly HashSet<string> ProfileFlowPages = new()
{
    // ... existing routes ...
    ROUTE_MY_NEW_PAGE,  // Add here if it's profile-related
};
```

### Step 5: Implement Back Button Handler
```csharp
protected override bool OnBackButtonPressed()
{
    MainThread.BeginInvokeOnMainThread(async () =>
    {
        await NavigationService.HandleBackButton(NavigationService.ROUTE_MY_NEW_PAGE);
    });
    return true;
}
```

---

## Debugging Navigation Issues

### Enable Console Logging

The NavigationService logs all back button decisions:
```
[Navigation] Back from HomePage — allowing OS exit
[Navigation] Back from TabBar page 'BookingPage' → //HomePage
[Navigation] Back from profile flow page 'EditeUserPage' → //ProfilePage
[Navigation] Back from subpage 'TerminbuchenPage' → pop one level (..)
```

### Check Current Route
```csharp
string currentRoute = NavigationService.GetCurrentRoute();
string currentPageName = NavigationService.GetCurrentPageName();
Console.WriteLine($"Current route: {currentRoute}");
Console.WriteLine($"Current page: {currentPageName}");
```

### Verify Page Type
```csharp
string page = NavigationService.GetCurrentPageName();
bool isTabBar = NavigationService.IsTabBarPage(page);
bool isProfileFlow = NavigationService.IsProfileFlowPage(page);
Console.WriteLine($"Is TabBar: {isTabBar}");
Console.WriteLine($"Is Profile Flow: {isProfileFlow}");
```

---

## Common Issues & Solutions

### Issue 1: Back button doesn't work from a subpage
**Cause:** Page doesn't have OnBackButtonPressed() implemented
**Solution:** Add the handler following Pattern 3 above

### Issue 2: Unexpected exit from profile flow page
**Cause:** Page is using Navigation.PopAsync() instead of centralized handler
**Solution:** Update to use NavigationService.HandleBackButton()

### Issue 3: Wrong route constant passed
**Cause:** Using string instead of constant, or wrong constant name
**Solution:** Always use NavigationService.ROUTE_* constants

### Issue 4: Back button seems slow/delayed
**Cause:** MainThread.BeginInvokeOnMainThread() causing UI thread scheduling
**Solution:** This is normal - short delay ensures thread safety. Not an issue.

---

## Architecture Benefits

✅ **Single Source of Truth:** All logic in one place
✅ **Consistent Behavior:** Same rules applied everywhere
✅ **Easy Maintenance:** Modify logic once, affects all pages
✅ **Easy Testing:** Test centralized handler, not individual pages
✅ **Debugging:** Centralized logging for all navigation decisions
✅ **Future-Proof:** Adding new navigation rules requires one change

---

## Summary

- **Tab Bar Pages** → Navigate to HomePage (or exit if HomePage)
- **Profile Flow Pages** → Always navigate to ProfilePage
- **Subpages** → Pop one level
- **Implementation** → Use `NavigationService.HandleBackButton(route)`
- **UI Buttons** → Use same handler as system back button
- **Thread Safety** → Use `MainThread.BeginInvokeOnMainThread()`

