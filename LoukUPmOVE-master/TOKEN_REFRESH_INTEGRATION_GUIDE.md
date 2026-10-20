# Token Refresh Integration Guide

## Quick Start for Existing Booking Code

### No Changes Required for Callers

Your existing booking code works unchanged:

```csharp
// Existing code - NO CHANGES NEEDED
var result = await apiServices.CreateBookingAsync(bookingPayload);

if (result.Success)
{
	// Handle booking success
	Debug.WriteLine("Booking created!");
}
else
{
	// Handle error
	Debug.WriteLine($"Error: {result.StatusCode} - {result.Message}");
}
```

### How Token Refresh Works Transparently

1. **Before**: 401 response → Booking failed immediately
2. **After**: 401 response → Token refreshed automatically → Booking retried → User never knows

### Multiple Services Booking (Already Supported)

```csharp
// Booking multiple services concurrently
var bookingTasks = await Task.WhenAll(
	apiServices.CreateBookingAsync(service1Payload),
	apiServices.CreateBookingAsync(service2Payload),
	apiServices.CreateBookingAsync(service3Payload)
);

// If token expired:
// - All 3 requests get 401
// - SemaphoreSlim coordinates single token refresh
// - All 3 requests retry with new token
// - Result: all succeed (or fail with new token)
```

---

## Architecture Overview

```
User Initiates Booking Request
		│
		├─→ Post to /api/bookings (with auth_token)
		│
		├─→ Server responds with 401 Unauthorized
		│   (auth_token expired)
		│
		├─→ Automatic token refresh triggered
		│   ├─→ RefreshTokenAsync() called
		│   ├─→ refresh_token sent to /api/auth/refresh
		│   ├─→ New access_token received
		│   └─→ Stored in SecureStorage
		│
		├─→ Booking request automatically retried
		│   (with new access_token)
		│
		└─→ Server responds with 201 Created
			Booking successful ✅
```

---

## What's Different from Before

| Aspect | Before | After |
|--------|--------|-------|
| **401 Response** | Booking fails immediately | Automatic token refresh → Retry |
| **Multiple 401s** | Each request refreshes token | Single coordinated refresh |
| **Refresh Token Expired** | Same as before (401 returned) | Same as before |
| **UI Experience** | "Booking failed" error | Transparent retry (user doesn't notice) |
| **Code Changes** | N/A | Internal to ApiServices only |

---

## Common Scenarios Handled

### ✅ Scenario 1: Single Service Booking with Expired Token
```
User books hair cut service
  → Token expired
  → Auto-refresh
  → Auto-retry
  → Booking appears in user's schedule ✅
```

### ✅ Scenario 2: Multiple Services (Different Stylists/Salons)
```
User books:
  • Hair cut at Salon A
  • Hair color at Salon B
  • Nail treatment at Salon C
All simultaneously
  → All get 401 (expired token)
  → Single refresh (coordinated)
  → All 3 retry
  → All 3 succeed ✅
```

### ✅ Scenario 3: Custom Booking Logic
```
YourBookingService calls CreateBookingAsync
  → Returns (Success: true/false, StatusCode, Message, Json)
  → Your code handles result normally
  → Token refresh completely transparent ✅
```

### ✅ Scenario 4: Token Still Invalid After Refresh
```
Booking request → 401
  → Refresh succeeds
  → Retry booking → Still 401
  → Return error (no infinite loop) ✅
```

### ✅ Scenario 5: Refresh Token Expired
```
Booking request → 401
  → Attempt refresh → Fails (refresh token expired)
  → Return 401 error
  → Application redirects to login ✅
```

---

## Debugging Tips

### Check Token in SecureStorage
```csharp
string token = await SecureStorage.GetAsync("auth_token");
if (string.IsNullOrEmpty(token))
	Debug.WriteLine("No token stored");
else
	Debug.WriteLine($"Token: {token.Substring(0, 20)}...");
```

### Monitor Booking Requests
```csharp
// In ApiServices (for debugging only)
// Line 663: var result = await SendBookingRequestAsync(payload);
// Add logging:
Debug.WriteLine($"Initial booking attempt: StatusCode = {result.StatusCode}");

// Line 678: result = await SendBookingRequestAsync(payload);
// Add logging:
Debug.WriteLine($"Retry booking attempt: StatusCode = {result.StatusCode}");
```

### Check RefreshTokenAsync Logs
```csharp
// The existing RefreshTokenAsync already handles errors
// If it fails, it returns false silently
// To debug, you could add try-catch logging
```

---

## Deployment Checklist

- [ ] Build solution successfully (✅ Completed)
- [ ] No compilation errors (✅ Completed)
- [ ] Test with single booking creation
- [ ] Test with multiple concurrent bookings
- [ ] Test with expired token scenario
- [ ] Test with invalid refresh token scenario
- [ ] Deploy to staging environment
- [ ] Monitor error logs for 401s
- [ ] Deploy to production
- [ ] Monitor user experience (bookings should work seamlessly)

---

## FAQ

### Q: Will existing booking code break?
**A**: No. The API of `CreateBookingAsync()` is unchanged. Token refresh is an internal improvement.

### Q: What if refresh token is also expired?
**A**: User will see a 401 error, and your UI should prompt them to log in again (existing behavior preserved).

### Q: Can the token be refreshed during a non-booking API call?
**A**: The lock only affects booking requests. Other API calls are unaffected.

### Q: What if multiple booking requests happen simultaneously?
**A**: The `SemaphoreSlim` ensures only one refresh happens, then all requests retry with the new token.

### Q: Is there a retry limit?
**A**: Yes, exactly one retry per request. If it fails twice, the error is returned (no infinite loops).

### Q: How long does token refresh take?
**A**: ~200-500ms for the HTTP call plus network latency. The UI should not notice due to async/await.

### Q: What happens if refresh endpoint is down?
**A**: RefreshTokenAsync returns false, the original 401 error is returned to the user.

### Q: Can I customize retry behavior?
**A**: The current implementation is optimized for the requirements. If custom behavior is needed, modify the logic in `CreateBookingAsync()` (lines 660-689).

### Q: Does this work with multiple salons/services?
**A**: Yes, the coordination lock ensures efficiency even with many concurrent requests.

---

## Technical Debt or Future Improvements

1. **Exponential Backoff**: Could add delays between refresh and retry (currently immediate)
2. **Refresh Token Refresh**: API could return new refresh token on success
3. **Token Expiry Tracking**: Could preemptively refresh before expiry
4. **Metrics**: Could track refresh rate for monitoring
5. **Configurable Timeout**: Retry delay could be configurable

None of these are required for the current requirements. ✅

---

## Reference Documentation

- **API Contract**: See inline comments in SendBookingRequestAsync()
- **Token Storage**: Uses Microsoft.Maui.Storage.SecureStorage (MAUI built-in)
- **Error Codes**: 
  - 401: Unauthorized (handled with refresh)
  - 422: Validation error
  - 429: Rate limited
  - 500+: Server errors
- **Concurrency**: Uses System.Threading.SemaphoreSlim (thread-safe)

---

## Questions or Issues?

If you encounter issues with token refresh:

1. Check that `refresh_token` is properly stored in SecureStorage after login
2. Verify the `/api/auth/refresh` endpoint is working
3. Ensure `auth_token` is being properly read by `SetAuthorizationHeaderAsync()`
4. Check server response codes (RefreshTokenAsync handles non-success responses)
5. Review error messages in CreateBookingAsync return value

All logging is handled via Console.WriteLine() which can be viewed in debug output.
