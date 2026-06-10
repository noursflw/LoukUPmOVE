namespace loukupm.Services;

/// <summary>
/// Tracks consecutive back button presses for terminal pages (like ROUTE_LOGIN and ROUTE_SPLASH).
/// Enables "double-tap to exit" functionality: first press is handled (returns true),
/// second press within the timeout window exits the app (returns false).
/// </summary>
public static class BackButtonTracker
{
    private static readonly Dictionary<string, BackPressRecord> BackPressRecords = new();
    private const int TimeoutMilliseconds = 2000; // 2-second window for double-tap

    private class BackPressRecord
    {
        public DateTime LastPressTime { get; set; }
        public int PressCount { get; set; }
    }

    /// <summary>
    /// Registers a back button press for the specified page.
    /// Returns true if this is the first press (should handle it).
    /// Returns false if this is the second press (should let the OS exit).
    /// </summary>
    /// <param name="pageName">The name of the page (e.g., "LoginPage").</param>
    /// <returns>true if first press, false if second press within timeout window.</returns>
    public static bool RegisterBackPress(string pageName)
    {
        if (string.IsNullOrWhiteSpace(pageName))
            return true;

        var now = DateTime.UtcNow;

        if (!BackPressRecords.ContainsKey(pageName))
        {
            // First press ever on this page
            BackPressRecords[pageName] = new BackPressRecord
            {
                LastPressTime = now,
                PressCount = 1
            };

            Console.WriteLine($"[BackButtonTracker] First back press on {pageName} - preventing exit");
            return true; // Handle first press
        }

        var record = BackPressRecords[pageName];
        var timeSinceLastPress = (now - record.LastPressTime).TotalMilliseconds;

        if (timeSinceLastPress < TimeoutMilliseconds)
        {
            // Second press within timeout window - allow exit
            record.PressCount++;
            Console.WriteLine($"[BackButtonTracker] Double-tap detected on {pageName} (presses: {record.PressCount}) - allowing exit");
            return false; // Allow exit
        }

        // Timeout expired - reset and treat as first press again
        record.LastPressTime = now;
        record.PressCount = 1;
        Console.WriteLine($"[BackButtonTracker] Timeout expired on {pageName} - reset counter");
        return true; // Handle as new first press
    }

    /// <summary>
    /// Resets the back press count for a specific page.
    /// Call this when user navigates away from the page.
    /// </summary>
    public static void ResetPage(string pageName)
    {
        if (BackPressRecords.ContainsKey(pageName))
        {
            BackPressRecords.Remove(pageName);
            Console.WriteLine($"[BackButtonTracker] Reset counter for {pageName}");
        }
    }

    /// <summary>
    /// Clears all back press records.
    /// </summary>
    public static void ResetAll()
    {
        BackPressRecords.Clear();
        Console.WriteLine("[BackButtonTracker] Cleared all back press records");
    }
}
