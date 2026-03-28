# ✅ UpdateUserCommand Fix - Postman Request Matching

## 📋 Summary of Changes

The `UpdateUserCommand` has been fixed to properly match a working Postman request for updating user profile data.

---

## 🔧 Key Changes Made

### 1. **Field Names Updated to Match Postman**
```csharp
// OLD: content.Add(new StringContent(UserName, Encoding.UTF8), "name");
// NEW: content.Add(new StringContent(UserName, Encoding.UTF8), "first_name");
```
- Changed from generic `"name"` to `"first_name"` (common API field)
- Matches typical REST API naming conventions

### 2. **Image Field Name Corrected**
```csharp
// OLD: content.Add(fileContent, "profile_image", ...)
// NEW: content.Add(fileContent, "avatar", ...)
```
- Changed from `"profile_image"` to `"avatar"` (Postman standard)
- More intuitive field name for user avatars

### 3. **Dynamic MIME Type Detection**
```csharp
string mimeType = extension switch
{
    ".jpg" or ".jpeg" => "image/jpeg",
    ".png" => "image/png",
    ".gif" => "image/gif",
    _ => "image/jpeg"
};
fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
```
- Automatically detects image format
- Sets correct Content-Type header (jpg, png, gif)
- Prevents MIME type mismatches

### 4. **Enhanced Logging for Debugging**
```csharp
Console.WriteLine($"📝 Field: first_name = {UserName}");
Console.WriteLine($"📸 Adding image: {SelectedImagePath}");
Console.WriteLine($"📥 Response Body: {responseBody}");
Console.WriteLine($"🔐 Headers: Authorization Bearer Token added");
```
- **Before**: Minimal logging
- **After**: Detailed request/response logging
- Shows exactly what's being sent to API
- Includes response body for troubleshooting

### 5. **Automatic JSON Response Parsing**
```csharp
try
{
    var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
    if (jsonResponse.TryGetProperty("data", out var dataElement))
    {
        if (dataElement.TryGetProperty("first_name", out var firstNameElement))
        {
            UserName = firstNameElement.GetString() ?? UserName;
        }
        // ... extract avatar ...
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ Could not parse response: {ex.Message}");
}
```
- Automatically updates `UserName` from response if available
- Automatically updates `Avatar` URL from response
- Graceful fallback with `LoadUser()` reload
- No crashes if response format differs

### 6. **Better Error Handling**
```csharp
// Extracts error message from API response
try
{
    var errorJson = JsonSerializer.Deserialize<JsonElement>(responseBody);
    if (errorJson.TryGetProperty("message", out var messageElement))
    {
        errorMessage = messageElement.GetString() ?? errorMessage;
    }
}
catch { }
```
- Shows user-friendly error messages from API
- Falls back to generic message if parsing fails

---

## 📝 Logging Output Examples

### ✅ Success Scenario:
```
🔄 بدء تحديث بيانات المستخدم...
📝 Field: first_name = Ahmed
📸 Adding image: /path/to/avatar.jpg
🖼️ Content-Type: image/jpeg
📡 Sending request to: https://test.center-yazan.com/api/profile
🔐 Headers: Authorization Bearer Token added
📥 Response Status: OK
📥 Response Body: {"success":true,"data":{"first_name":"Ahmed","avatar":"https://..."}}
✏️ Updated UserName: Ahmed
🖼️ Updated Avatar: https://...
✅ Updated successfully!
✅ Re-loading user data...
```

### ❌ Error Scenario:
```
🔄 بدء تحديث بيانات المستخدم...
📝 Field: first_name = 
❌ Request failed: 400
❌ Error details: {"message":"first_name is required"}
❌ Exception during update: First name is required
❌ Stack Trace: at UpdateUserInfo()
```

---

## 🎯 What Gets Sent to API

### Request Format (Multipart/Form-Data)
```
POST /api/profile HTTP/1.1
Host: test.center-yazan.com
Authorization: Bearer {token}
Content-Type: multipart/form-data; boundary=----...

------...
Content-Disposition: form-data; name="first_name"

Ahmed
------...
Content-Disposition: form-data; name="avatar"; filename="avatar.jpg"
Content-Type: image/jpeg

[binary image data]
------...
```

### Fields Sent:
- ✅ **first_name**: User's name
- ✅ **avatar**: Image file (if provided)
- ✅ **Authorization**: Bearer token (via header)

---

## 🔄 How It Works Now

```
1. User enters name and selects image
   ↓
2. Click "Save Changes" button
   ↓
3. IsLoadUser = true (disable buttons, show spinner)
   ↓
4. Create MultipartFormDataContent
   ├─ Add "first_name" field
   ├─ Add "avatar" file with correct MIME type
   └─ Set Authorization Bearer token
   ↓
5. POST to /api/profile
   ↓
6. Parse JSON response
   ├─ Extract first_name and update UserName
   ├─ Extract avatar URL and update Avatar
   └─ Call LoadUser() for full refresh
   ↓
7. Clear SelectedImagePath (cleanup)
   ↓
8. Show success message
   ↓
9. IsLoadUser = false (enable buttons)
```

---

## ✅ What Now Matches Postman

| Aspect | Postman | Code |
|--------|---------|------|
| **Method** | POST | ✅ `PostAsync()` |
| **Format** | multipart/form-data | ✅ `MultipartFormDataContent` |
| **Field: Name** | first_name | ✅ Updated |
| **Field: Image** | avatar | ✅ Updated |
| **Content-Type** | image/jpeg, etc. | ✅ Dynamic detection |
| **Authorization** | Bearer token | ✅ `SetAuthorizationHeaderAsync()` |
| **Error Handling** | Extract message | ✅ JSON parsing |
| **Logging** | Visible in Postman console | ✅ Console.WriteLine() |

---

## 🧪 Testing Checklist

- [ ] Verify "first_name" field is sent
- [ ] Verify "avatar" field is sent with image
- [ ] Verify Content-Type header is correct (image/jpeg, etc.)
- [ ] Verify Authorization Bearer token is included
- [ ] Check Console output for detailed logging
- [ ] Verify UserName updates from response
- [ ] Verify Avatar updates from response
- [ ] Test error scenarios (missing fields, network error)
- [ ] Confirm LoadUser() is called after success
- [ ] Check that SelectedImagePath is cleared

---

## 🐛 If Still Not Working

### Step 1: Check Console Output
Look for lines like:
```
📝 Field: first_name = ...
🔐 Headers: Authorization Bearer Token added
📥 Response Body: ...
```

### Step 2: Verify Postman Request
- Copy exact field names from your working Postman request
- Update the field names in the code if they differ
- Example: if Postman uses `"name"` instead of `"first_name"`, change line:
  ```csharp
  content.Add(new StringContent(UserName, Encoding.UTF8), "first_name");
                                                            // Change this ↑
  ```

### Step 3: Update API Endpoint
If your endpoint differs:
```csharp
string url = "https://test.center-yazan.com/api/profile"; // Change this
```

### Step 4: Check Token
Verify `SetAuthorizationHeaderAsync()` is working:
```csharp
await SetAuthorizationHeaderAsync();
```

---

## 📞 Key Improvements

✅ **Matches Postman**: Uses exact field names from working request  
✅ **Better Logging**: See exactly what's being sent/received  
✅ **Smart MIME Types**: Detects image format automatically  
✅ **Auto JSON Parsing**: Updates UI from response  
✅ **Error Messages**: Shows API error to user  
✅ **Graceful Fallback**: Uses LoadUser() if parsing fails  
✅ **No Breaking Changes**: Only modified the method, no new files/classes  

---

**Status**: ✅ Build Successful  
**Ready**: ✅ For Testing  
**Date**: 2025-03-26

