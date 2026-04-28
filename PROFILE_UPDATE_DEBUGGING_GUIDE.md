# Profile Update Debugging & Troubleshooting Guide

## 🔴 Reported Issue

**"It's not updating"** - Profile update API call is not working

### Expected Behavior
```
User enters first_name OR selects image
↓
Click "Update"
↓
API receives multipart request
↓
Server processes request
↓
Returns updated profile with profile_image_url
↓
App displays confirmation
```

### Actual Behavior
```
Update button clicked
↓
❌ Update fails silently OR shows error
↓
Profile not updated
```

---

## 🔧 Step 1: Verify API Endpoint Works (Postman Test)

### Test 1A: Update First Name Only
```
POST: https://test.center-yazan.com/api/profile
Headers:
  Authorization: Bearer [YOUR_TOKEN]
  Content-Type: multipart/form-data

Body (form-data):
  first_name: "Nour" (text)

Expected Response: ✅
{
  "success": true,
  "message": "Profile updated successfully",
  "data": {
    "id": 11,
    "first_name": "Nour",
    "profile_image_url": "https://test.center-yazan.com/storage/..."
  }
}
```

### Test 1B: Update Image Only
```
POST: https://test.center-yazan.com/api/profile
Headers:
  Authorization: Bearer [YOUR_TOKEN]
  Content-Type: multipart/form-data

Body (form-data):
  image: [SELECT_FILE] (file upload)

Expected Response: ✅
{
  "success": true,
  "message": "Profile updated successfully",
  "data": {
    "id": 11,
    "first_name": "Nour",
    "profile_image_url": "https://test.center-yazan.com/storage/new_image.jpg"
  }
}
```

### Test 1C: Update Both
```
POST: https://test.center-yazan.com/api/profile
Headers:
  Authorization: Bearer [YOUR_TOKEN]
  Content-Type: multipart/form-data

Body (form-data):
  first_name: "Nour" (text)
  image: [SELECT_FILE] (file upload)

Expected Response: ✅
{
  "success": true,
  "message": "Profile updated successfully",
  "data": {
    "id": 11,
    "first_name": "Nour",
    "profile_image_url": "https://test.center-yazan.com/storage/new_image.jpg"
  }
}
```

---

## 🔍 Step 2: Check Console Logs

The `UpdateUserProfileAsync` method has comprehensive logging. Look for:

### Expected Console Output (Success)
```
✅ Authorization: Bearer token present (length: 123)
📋 Field added: first_name = 'Nour'
📋 Field added: image = file 'photo.jpg' (size: 45332 bytes, content-type: image/jpeg)
📤 Sending update request to API:
   URL: https://test.center-yazan.com/api/profile
   Method: POST
   Content-Type: multipart/form-data
   Authorization: Bearer [token]
   Fields being sent: first_name image
📊 Response Status: 200
📄 Response Body: {"success":true,"message":"Profile updated successfully",...}
✅ Response parsed successfully
```

### Error: No Token
```
❌ Authorization: No token found
```
**Fix**: User not logged in

### Error: Token Expired  
```
📊 Response Status: 401
❌ Unauthorized (401) - Token invalid or expired
```
**Fix**: Refresh token first

### Error: File Not Found
```
⚠️ Avatar file not found at path: /path/to/file.jpg
```
**Fix**: Verify file exists

---

## 🎯 Most Likely Cause

**Both fields are empty when sending!**

Check your ViewModel code - are you clearing the fields before calling update?

```csharp
// ❌ WRONG - both empty
UserFirstName = "";
SelectedImagePath = "";
await UpdateUserInfo();  // Sends nothing!

// ✅ RIGHT - at least one has value
UserFirstName = "NewName";
SelectedImagePath = "";
await UpdateUserInfo();  // Sends first_name

// ✅ RIGHT - image selected
UserFirstName = "";
SelectedImagePath = "/path/to/image.jpg";
await UpdateUserInfo();  // Sends image
```

---

## 🔧 Recommended Fix: Add Validation

Add this to your `UpdateUserInfo()` method before calling the API:

```csharp
// Validate at least one field has data
if (string.IsNullOrWhiteSpace(UserFirstName) && string.IsNullOrWhiteSpace(SelectedImagePath))
{
    await Toast.Make("Enter a name or select an image", ToastDuration.Short).Show();
    return;
}

// Verify file exists if image path provided
if (!string.IsNullOrWhiteSpace(SelectedImagePath) && !File.Exists(SelectedImagePath))
{
    await Toast.Make("Image file not found", ToastDuration.Short).Show();
    return;
}

Console.WriteLine($"📋 Updating profile:");
Console.WriteLine($"   Name: {UserFirstName ?? "[not changing]"}");
Console.WriteLine($"   Image: {SelectedImagePath ?? "[not changing]"}");

var apiResponse = await _apiServices.UpdateUserProfileAsync(UserFirstName, SelectedImagePath);

Console.WriteLine($"📊 Response received:");
Console.WriteLine($"   Success: {apiResponse?.Success}");
Console.WriteLine($"   Message: {apiResponse?.Message}");
```

---

## ✅ Checklist

- [ ] Test in Postman with valid token
- [ ] Check console output for errors
- [ ] Verify token is not expired
- [ ] Ensure file exists if uploading
- [ ] Verify at least ONE field has data
- [ ] Check network response in DevTools
- [ ] Confirm field names: `first_name` and `image`

---

## 📞 Next Steps

**Option 1: Test in Postman**
- Copy your auth token
- Try the exact Postman tests above
- Report back if it works/fails

**Option 2: Enable Debug Mode**
- Check Android Logcat or iOS Console
- Look for the console logs above
- Share the exact error message

**Option 3: Share Details**
- What error do you see?
- What values are you trying to update?
- Is Postman test working?

