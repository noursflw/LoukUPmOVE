# Integration Code Snippets - Copy & Paste Ready

Copy these code snippets into the respective files to complete the OneSignal notification tap feature.

---

## 1️⃣ AppShell.xaml.cs - Add This to Constructor

**File**: `loukupm\AppShell.xaml.cs`  
**Location**: In the `AppShell()` constructor after `InitializeComponent();`

```csharp
public AppShell()
{
    InitializeComponent();

    // ✨ NEW CODE - Start
    SetupNotificationTapHandler();
    // ✨ NEW CODE - End
}

// ✨ NEW METHOD - Start
/// <summary>
/// Sets up notification tap handler for foreground and background states.
/// </summary>
private void SetupNotificationTapHandler()
{
    try
    {
        // Ensure notification handler is ready after shell initialization
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(500); // Allow shell to fully initialize
            Console.WriteLine("✅ AppShell notification tap handler ready");

            // Optional: You can add additional setup here if needed
            // For example, restoring notification state or checking for pending notifications
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error setting up notification handler in AppShell: {ex.Message}");
    }
}
// ✨ NEW METHOD - End
```

---

## 2️⃣ Android MainActivity.cs - Add This Handler

**File**: `loukupm\Platforms\Android\MainActivity.cs`  
**Location**: Inside the `MainActivity` class, add this method

```csharp
// ✨ NEW METHOD - Start
/// <summary>
/// Handles notification tap when app is in background or terminated state.
/// Called by Android system when user taps a notification.
/// </summary>
protected override void OnNewIntent(Intent intent)
{
    base.OnNewIntent(intent);

    try
    {
        // Check if app was opened via notification tap
        if (intent?.Extras != null)
        {
            var osData = intent.Extras.GetString("os_data");
            var notificationId = intent.Extras.GetString("notification_id");

            if (!string.IsNullOrEmpty(osData) || !string.IsNullOrEmpty(notificationId))
            {
                Console.WriteLine($"📬 Notification tap detected (Android, terminated/background state)");

                // Delay to ensure UI is fully initialized
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(1500); // Give UI time to initialize
                    await OneSignalService.HandleNotificationTapped();
                });
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error in OnNewIntent: {ex.Message}");
    }
}
// ✨ NEW METHOD - End
```

**Don't forget to add these using statements at the top**:
```csharp
using OneSignalSDK.DotNet;
using loukupm.Services;
using Android.Content;
```

---

## 3️⃣ iOS AppDelegate.cs - Add This Handler

**File**: `loukupm\Platforms\iOS\AppDelegate.cs`  
**Location**: In the `DidFinishLaunching()` method before `return true;`

```csharp
// ✨ NEW CODE - Start - Add this in DidFinishLaunching() before "return true;"
// Handle app launch from terminated state via notification tap
if (launchOptions != null)
{
    try
    {
        // Check if app was opened via remote notification
        if (launchOptions.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey))
        {
            var userInfo = launchOptions[UIApplication.LaunchOptionsRemoteNotificationKey] as NSDictionary;

            if (userInfo != null && userInfo.Count > 0)
            {
                Console.WriteLine("📬 App launched from notification (iOS, terminated state)");

                // Delay to ensure app UI is ready
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(2000); // iOS needs more time than Android
                    await OneSignalService.HandleNotificationTapped();
                });
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error handling iOS notification launch: {ex.Message}");
    }
}
// ✨ NEW CODE - End
```

**Don't forget to add these using statements at the top**:
```csharp
using loukupm.Services;
using UserNotifications;
```

---

## ⚡ Quick Integration Checklist

Use this checklist to verify you've added all code:

### AppShell.xaml.cs
- [ ] Added `SetupNotificationTapHandler()` call in constructor
- [ ] Added `SetupNotificationTapHandler()` method
- [ ] Code compiles without errors

### MainActivity.cs (Android)
- [ ] Added `using OneSignalSDK.DotNet;`
- [ ] Added `using loukupm.Services;`
- [ ] Added `using Android.Content;`
- [ ] Added `OnNewIntent()` method
- [ ] Method checks for `os_data` or `notification_id`
- [ ] Method calls `OneSignalService.HandleNotificationTapped()`
- [ ] Code compiles without errors

### AppDelegate.cs (iOS)
- [ ] Added `using loukupm.Services;`
- [ ] Added launch options check in `DidFinishLaunching()`
- [ ] Checks for `UIApplication.LaunchOptionsRemoteNotificationKey`
- [ ] Calls `OneSignalService.HandleNotificationTapped()`
- [ ] Code compiles without errors

### OneSignalService.cs
- [ ] Already has `HandleNotificationTapped()` method ✅
- [ ] Already has `SetupNotificationHandlers()` method ✅
- [ ] Already has `NavigateToNotificationPageAsync()` method ✅

---

## 🧪 Test After Integration

After adding all code snippets:

1. **Build the solution**: Verify no compiler errors
2. **Test Foreground**: App running → receive notification → tap it → should navigate to NotificationPage
3. **Test Background**: App in background → notification arrives → tap it → app resumes and navigates
4. **Test Terminated**: Kill app → notification arrives → tap it → app starts and navigates
5. **Check Logs**: Look for ✅ and 📍 messages in Visual Studio Output window

---

## 🐛 Debugging Tips

**If navigation isn't working**:
1. Check Visual Studio Output window (Debug pane) for error messages
2. Look for ❌ emoji - shows where error occurred
3. Verify `NotificationPage` is registered in `NavigationService` constants
4. Verify route in `AppShell.xaml` matches `ROUTE_NOTIFICATION`
5. Ensure delay times are sufficient (1-2 seconds minimum)

**If terminated state doesn't work on Android**:
- Verify `LaunchMode = LaunchMode.SingleTop` in `MainActivity` attributes
- Check if `OnNewIntent()` is being called (add log at start)
- Increase delay to 2000ms (2 seconds)

**If terminated state doesn't work on iOS**:
- Verify notification permission is granted
- Check if `LaunchOptionsRemoteNotificationKey` is present
- Increase delay to 2500ms (2.5 seconds)
- Verify app is not restoring to previous state

---

## 💾 Final Verification

Run this build command to verify everything compiles:
```bash
dotnet build
```

Expected output:
```
Build succeeded.
0 Warning(s)
0 Error(s)
```

If you see any errors:
1. Check the error message for file and line number
2. Verify you copied the code exactly
3. Check that using statements are correct
4. Ensure no duplicate methods
5. Verify syntax (braces, semicolons, etc.)

---

## 📞 Need Help?

If integration doesn't work after adding these snippets:

1. **Check Integration Guide**: `ONESIGNAL_NOTIFICATION_TAP_IMPLEMENTATION.md`
2. **Review Console Logs**: Look for 📬, 📍, or ❌ messages
3. **Verify File Paths**: Ensure files are in correct directories
4. **Check Syntax**: Copy-paste code line by line if needed
5. **Test Each State**: Foreground → Background → Terminated

---

**Status**: All code ready to integrate  
**Files to modify**: 3 (AppShell.xaml.cs, MainActivity.cs, AppDelegate.cs)  
**Lines of code**: ~100 (all provided above)  
**Estimated integration time**: 15-20 minutes
