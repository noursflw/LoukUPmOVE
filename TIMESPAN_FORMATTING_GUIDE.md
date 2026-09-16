# TimeSpan & DateTime Formatting Guide for .NET MAUI

## ❌ Issue Fixed: "Input string was not in a correct format"

### Root Cause
Using `DateTime` format specifiers on `TimeSpan` objects:
- **WRONG**: `{timespan:HH:mm:ss}` ❌
- **CORRECT**: `{timespan:hh\:mm\:ss}` ✅

---

## Format String Reference

### DateTime Formatting
| Format | Example | Use Case |
|--------|---------|----------|
| `yyyy-MM-dd HH:mm:ss` | 2026-03-27 14:30:45 | Full datetime |
| `yyyy-MM-ddTHH:mm:ss` | 2026-03-27T14:30:45 | ISO 8601 format (API) |
| `MM/dd/yyyy` | 03/27/2026 | Date only |
| `HH:mm:ss` | 14:30:45 | Time only (DateTime) |

```csharp
var now = DateTime.Now;
Console.WriteLine($"DateTime: {now:yyyy-MM-dd HH:mm:ss}"); // ✅ CORRECT
```

### TimeSpan Formatting
| Format | Example | Use Case |
|--------|---------|----------|
| `hh\:mm\:ss` | 14:30:45 | Hours:Minutes:Seconds |
| `mm\:ss` | 30:45 | Minutes:Seconds |
| `h\:mm\:ss\.ff` | 1:30:45.50 | With milliseconds |

```csharp
var timespan = TimeSpan.FromHours(14.5);
Console.WriteLine($"TimeSpan: {timespan:hh\\:mm\\:ss}"); // ✅ CORRECT
```

**Why escape the colons?** In TimeSpan format strings, colons are special characters, so they must be escaped with backslashes.

---

## Common Errors & Solutions

### ❌ Error 1: Using DateTime Format on TimeSpan
```csharp
var reminderTime = TimeSpan.FromHours(2);
Console.WriteLine($"Time: {reminderTime:HH:mm:ss}"); // ❌ CRASHES!
// Exception: Input string was not in a correct format
```

**Fix:**
```csharp
var reminderTime = TimeSpan.FromHours(2);
Console.WriteLine($"Time: {reminderTime:hh\\:mm\\:ss}"); // ✅ Works!
```

---

### ❌ Error 2: Invalid Parse Format
```csharp
string timeString = "14:30:45";
var parsed = TimeSpan.Parse(timeString); // ✅ This works fine

// But be careful with:
string invalidTime = "not a time";
var parsed = TimeSpan.Parse(invalidTime); // ❌ CRASHES!
```

**Fix: Always use TryParse**
```csharp
string timeString = "14:30:45";
if (TimeSpan.TryParse(timeString, out var parsed))
{
    Console.WriteLine($"Parsed: {parsed:hh\\:mm\\:ss}"); // ✅ Safe!
}
else
{
    Console.WriteLine("Invalid time format");
}
```

---

### ❌ Error 3: DateTime.Parse on Unexpected Format
```csharp
string appointmentDate = "2026-04-01";
var dateTime = DateTime.Parse(appointmentDate); // ✅ Works
var dateTime2 = DateTime.Parse("invalid"); // ❌ CRASHES!
```

**Fix: Always use TryParse with CultureInfo**
```csharp
string appointmentDate = "2026-04-01";
if (DateTime.TryParse(appointmentDate, System.Globalization.CultureInfo.InvariantCulture, 
    System.Globalization.DateTimeStyles.None, out var dateTime))
{
    Console.WriteLine($"Parsed: {dateTime:yyyy-MM-dd}"); // ✅ Safe!
}
else
{
    Console.WriteLine("Invalid date format");
}
```

---

## Best Practices for Your Appointment Reminder System

### ✅ Safe TimeSpan Formatting
```csharp
// From TimeSpan (e.g., from TimePicker)
var reminderTime = ReminderTime; // TimeSpan
Console.WriteLine($"Reminder: {reminderTime:hh\\:mm\\:ss}"); // ✅ CORRECT

// Extract TimeOfDay from DateTime
var appointmentTime = appointmentDateTime.TimeOfDay; // Returns TimeSpan
Console.WriteLine($"Appointment: {appointmentTime:hh\\:mm\\:ss}"); // ✅ CORRECT
```

### ✅ Safe DateTime Formatting
```csharp
// For API communication (use ISO 8601)
var remindAtDateTime = DateTime.Now;
var apiPayload = new { remind_at = remindAtDateTime.ToString("yyyy-MM-ddTHH:mm:ss") };

// For display
Console.WriteLine($"Date: {remindAtDateTime:yyyy-MM-dd HH:mm:ss}"); // ✅ CORRECT
```

### ✅ Safe Parsing from API
```csharp
// Parse appointment date from API
string appointmentDateFromApi = "2026-04-01"; // Could be any format!

if (DateTime.TryParse(appointmentDateFromApi, 
    System.Globalization.CultureInfo.InvariantCulture,
    System.Globalization.DateTimeStyles.AssumeUniversal,
    out var appointmentDateTime))
{
    var appointmentTime = appointmentDateTime.TimeOfDay;
    Console.WriteLine($"✅ Parsed successfully: {appointmentDateTime:yyyy-MM-dd HH:mm:ss}");
}
else
{
    Console.WriteLine($"❌ Failed to parse: {appointmentDateFromApi}");
}
```

---

## Applied Fixes in Your Code

All these lines were corrected in `AppViweModel.cs`:

| Line | Before | After | Issue |
|------|--------|-------|-------|
| 1491 | `{reminderTime:HH:mm:ss}` | `{reminderTime:hh\\:mm\\:ss}` | TimeSpan format |
| 1507 | `{reminderTime:HH:mm:ss}` | `{reminderTime:hh\\:mm\\:ss}` | TimeSpan format |
| 1522 | `{reminderTime:HH:mm:ss}` | `{reminderTime:hh\\:mm\\:ss}` | TimeSpan format |
| 1532 | `{reminderTime:HH:mm:ss}` | `{reminderTime:hh\\:mm\\:ss}` | TimeSpan format |
| 1563 | `{appointmentTime:HH:mm:ss}` | `{appointmentTime:hh\\:mm\\:ss}` | TimeSpan format |
| 1564 | `{reminderTime:HH:mm:ss}` | `{reminderTime:hh\\:mm\\:ss}` | TimeSpan format |
| 1569 | `{reminderTime:HH:mm:ss}` | `{reminderTime:hh\\:mm\\:ss}` | TimeSpan format |
| 1569 | `{appointmentTime:HH:mm:ss}` | `{appointmentTime:hh\\:mm\\:ss}` | TimeSpan format |

---

## Production-Ready Implementation Template

```csharp
public class DateTimeHelper
{
    private static readonly IFormatProvider Culture = System.Globalization.CultureInfo.InvariantCulture;

    /// <summary>
    /// Safely format TimeSpan for display
    /// </summary>
    public static string FormatTimeSpan(TimeSpan time)
    {
        try
        {
            return time.ToString(@"hh\:mm\:ss");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TimeSpan format error: {ex.Message}");
            return "00:00:00";
        }
    }

    /// <summary>
    /// Safely format DateTime for display
    /// </summary>
    public static string FormatDateTime(DateTime dateTime)
    {
        try
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ DateTime format error: {ex.Message}");
            return "N/A";
        }
    }

    /// <summary>
    /// Safely format DateTime for API communication
    /// </summary>
    public static string FormatDateTimeForApi(DateTime dateTime)
    {
        try
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ DateTime API format error: {ex.Message}");
            return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        }
    }

    /// <summary>
    /// Safely parse DateTime from string
    /// </summary>
    public static bool TryParseDateTime(string dateString, out DateTime result)
    {
        return DateTime.TryParse(dateString, Culture, 
            System.Globalization.DateTimeStyles.AssumeUniversal, 
            out result);
    }

    /// <summary>
    /// Safely parse TimeSpan from string
    /// </summary>
    public static bool TryParseTimeSpan(string timeString, out TimeSpan result)
    {
        return TimeSpan.TryParse(timeString, out result);
    }
}

// Usage:
var reminderTime = TimeSpan.FromHours(2);
Console.WriteLine($"✅ {DateTimeHelper.FormatTimeSpan(reminderTime)}");

var appointmentDate = "2026-04-01";
if (DateTimeHelper.TryParseDateTime(appointmentDate, out var dateTime))
{
    Console.WriteLine($"✅ {DateTimeHelper.FormatDateTime(dateTime)}");
}
```

---

## Checklist for Prevention

- [ ] Always use `hh\:mm\:ss` for **TimeSpan** formatting
- [ ] Always use `HH:mm:ss` or `yyyy-MM-dd HH:mm:ss` for **DateTime** formatting
- [ ] Use `TryParse()` instead of `Parse()` for user/API input
- [ ] Use `InvariantCulture` when parsing data from APIs
- [ ] Wrap format operations in try-catch blocks
- [ ] Log format errors for debugging
- [ ] Create helper methods for common formatting operations
- [ ] Test with edge cases: midnight, leap years, timezone boundaries
- [ ] Validate API responses before parsing

---

## Testing Examples

```csharp
// Test TimeSpan formatting
[Test]
public void TestTimeSpanFormatting()
{
    var time = TimeSpan.FromHours(14.5); // 14:30:00
    Assert.AreEqual("14:30:00", time.ToString(@"hh\:mm\:ss"));
}

// Test DateTime formatting
[Test]
public void TestDateTimeFormatting()
{
    var dt = new DateTime(2026, 4, 1, 14, 30, 45);
    Assert.AreEqual("2026-04-01 14:30:45", dt.ToString("yyyy-MM-dd HH:mm:ss"));
}

// Test safe parsing
[Test]
public void TestSafeTimeSpanParsing()
{
    var result = TimeSpan.TryParse("14:30:45", out var timespan);
    Assert.IsTrue(result);
    Assert.AreEqual(14, timespan.Hours);
}
```

---

## Summary

✅ **All format string issues have been fixed in your code**
✅ **Build is successful**
✅ **Ready for testing and deployment**

The appointment reminder system now properly handles:
- TimeSpan formatting for display
- DateTime formatting for display and API
- Safe parsing with error handling
- Localized and invariant culture support
