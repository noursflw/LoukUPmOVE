# Profile Update - Quick Testing Guide

## 🎯 Step-by-Step Testing

### Step 1: Test in Postman (Verify Backend Works)

#### Scenario A: Update First Name Only
```
1. Open Postman
2. Create new POST request
3. URL: https://test.center-yazan.com/api/profile
4. Headers tab:
   - Authorization: Bearer [PASTE_YOUR_TOKEN]
5. Body tab:
   - Select "form-data"
   - KEY: first_name | VALUE: "TestName" | TYPE: Text
6. Click Send
```

**Expected**: HTTP 200 + `profile_image_url` in response

#### Scenario B: Update Image Only
```
1. Open Postman
2. Create new POST request
3. URL: https://test.center-yazan.com/api/profile
4. Headers tab:
   - Authorization: Bearer [PASTE_YOUR_TOKEN]
5. Body tab:
   - Select "form-data"
   - KEY: image | VALUE: [SELECT_FILE] | TYPE: File
6. Click Send
```

**Expected**: HTTP 200 + new `profile_image_url` in response

---

### Step 2: Test in App (Check Debug Output)

#### Before Testing
1. **Build and run the app**
2. **Make sure you're logged in**
3. **Open Debug Console** (View → Output or Debug → Debug Output window)

#### Test Scenario A: Update Name
1. Go to Edit Profile page
2. Clear the name field
3. Type a new name: "TestName"
4. Don't select an image
5. Click Update button
6. **Check console output** ⬇️

```
Expected Console Output:
============================================================
🔄 UPDATE USER INFO STARTED
============================================================
📋 UserFirstName: 'TestName' (IsEmpty: False)
📋 SelectedImagePath: '' (IsEmpty: True)
============================================================

📤 Calling UpdateUserProfileAsync...

✅ Authorization: Bearer token present (length: 123)
📋 Field added: first_name = 'TestName'
📤 Sending update request to API:
   URL: https://test.center-yazan.com/api/profile
   Method: POST
   Content-Type: multipart/form-data
   Authorization: Bearer [token]
   Fields being sent: first_name 
📊 Response Status: 200
📄 Response Body: {"success":true,"message":"Profile updated successfully",...}
✅ Response parsed successfully

============================================================
📥 RESPONSE RECEIVED FROM API
============================================================
📊 Success: True
📊 Message: Profile updated successfully
📊 Data: Present
   - Id: 11
   - FirstName: TestName
   - ProfileImageUrl: https://test.center-yazan.com/storage/...
============================================================

✅ Profile image updated from API: https://test.center-yazan.com/storage/...
```

#### Test Scenario B: Update Image
1. Go to Edit Profile page
2. Clear the name field (or leave empty)
3. Select an image file
4. Click Update button
5. **Check console output** ⬇️

```
Expected Console Output:
============================================================
🔄 UPDATE USER INFO STARTED
============================================================
📋 UserFirstName: '' (IsEmpty: True)
📋 SelectedImagePath: '/path/to/image.jpg' (IsEmpty: False)
📋 Image File Exists: True
============================================================

📤 Calling UpdateUserProfileAsync...

✅ Authorization: Bearer token present (length: 123)
📋 Field added: image = file 'image.jpg' (size: 45332 bytes, content-type: image/jpeg)
📤 Sending update request to API:
   URL: https://test.center-yazan.com/api/profile
   Method: POST
   Content-Type: multipart/form-data
   Authorization: Bearer [token]
   Fields being sent: image
📊 Response Status: 200
📄 Response Body: {"success":true,"message":"Profile updated successfully",...}
✅ Response parsed successfully

============================================================
📥 RESPONSE RECEIVED FROM API
============================================================
📊 Success: True
📊 Message: Profile updated successfully
📊 Data: Present
   - Id: 11
   - FirstName: Nour
   - ProfileImageUrl: https://test.center-yazan.com/storage/new_image.jpg
============================================================

✅ Profile image updated from API: https://test.center-yazan.com/storage/new_image.jpg
```

---

## ❌ Common Errors & Solutions

### Error 1: No Token
```
❌ Authorization: No token found
```
**Solution**: Make sure you're logged in first!

### Error 2: Token Expired
```
📊 Response Status: 401
❌ Unauthorized (401) - Token invalid or expired
```
**Solution**: Logout and login again to refresh token

### Error 3: File Not Found
```
📋 Image File Exists: False
❌ File not found at path: /path/to/image.jpg
```
**Solution**: Select a valid image file that exists

### Error 4: Both Fields Empty
```
📋 UserFirstName: '' (IsEmpty: True)
📋 SelectedImagePath: '' (IsEmpty: True)
```
**Solution**: Enter a name OR select an image (at least one!)

### Error 5: Network Error
```
❌ Exception: The request timed out
```
**Solution**: Check your internet connection

---

## ✅ Success Criteria

After clicking Update, you should see:

- ✅ "Success: True" in console
- ✅ "HTTP 200" response status
- ✅ Confirmation popup appears
- ✅ New profile_image_url in response
- ✅ Image displays the server URL (not local path)

---

## 🔍 If Something's Wrong

Copy-paste the **ENTIRE console output** and provide:
1. What you tried to update (name/image/both)
2. The exact error message
3. The complete console output
4. Whether Postman test works

Then we can identify the exact issue!

