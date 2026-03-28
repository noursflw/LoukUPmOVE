# Copilot Instructions

## Project Guidelines
- User is working on a .NET MAUI 10 appointment booking application. They had a critical formatting error where DateTime format specifiers (HH:mm:ss) were being applied to TimeSpan objects, causing "Input string was not in a correct format" exceptions. The fix requires using hh\:mm\:ss format for TimeSpan objects instead. This is a common pitfall in .NET when handling time-related objects. The user appreciates detailed documentation and comprehensive solutions.

### Critical DateTime Comparison Bug Fix
- **Problem**: The system compared only TimeSpan values (time of day) without date context, causing midnight edge cases to fail. For example, a 23:30 reminder for a 00:15 appointment would be rejected because 23:30 > 00:15 numerically, ignoring that they are on different days.
- **Solution**: Changed from TimeSpan comparison to full DateTime object comparison with automatic date adjustment:
  1. Construct `reminderDateTime` using appointment DATE + selected TIME.
  2. If `reminderDateTime` >= `appointmentTime` on the same day, move the reminder to the previous day.
  3. Compare full DateTime objects (date + time).
- **Implementation**: Modified `EnableReminderTimerAsync()` in `lookupm\ViewModel\AppViewModel.cs`:
  - OLD: `if (reminderTime >= appointmentTime)` where both are TimeSpan.
  - NEW: Construct full DateTime objects, auto-adjust for previous day if needed, then compare.
- **Critical Test Cases**:
  1. Midnight edge (23:30 for 00:15 tomorrow) - NOW WORKS.
  2. Same-day (09:30 for 10:00 same day) - STILL WORKS.
  3. Early morning (01:30 for 02:00 same day) - STILL WORKS.
  4. Late evening (20:00 for 08:00 tomorrow) - NOW WORKS (auto-adjusts to previous day).
- **Build Status**: ✅ Successful, no errors.
- **Risk Level**: LOW - isolated logic change, no API modifications.
- **Deployment**: Ready for production.
- **Key Principle**: Never compare TimeSpan values when date context matters. Always use full DateTime objects for any time comparison across potential day boundaries.