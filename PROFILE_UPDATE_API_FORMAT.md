# Profile Update - API Request/Response Format

## 📨 Exact API Request Format

### Request 1: Update First Name Only

```http
POST /api/profile HTTP/1.1
Host: test.center-yazan.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="first_name"

Nour
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

**What gets sent from C# code:**
```csharp
form.Add(new StringContent("Nour"), "first_name");
// Result: Sends "Nour" with field name "first_name" ✓
```

---

### Request 2: Update Image Only

```http
POST /api/profile HTTP/1.1
Host: test.center-yazan.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="image"; filename="photo.jpg"
Content-Type: image/jpeg

[BINARY IMAGE DATA - 45,332 bytes]
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

**What gets sent from C# code:**
```csharp
var fileContent = new ByteArrayContent(fileBytes);
fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
form.Add(fileContent, "image", "photo.jpg");
// Result: Sends image file with field name "image" ✓
```

---

### Request 3: Update Both

```http
POST /api/profile HTTP/1.1
Host: test.center-yazan.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="first_name"

Nour
------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="image"; filename="photo.jpg"
Content-Type: image/jpeg

[BINARY IMAGE DATA - 45,332 bytes]
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

**What gets sent from C# code:**
```csharp
form.Add(new StringContent("Nour"), "first_name");
form.Add(fileContent, "image", "photo.jpg");
// Result: Sends both fields ✓
```

---

## 📥 API Response Format

### Success Response (HTTP 200)

```json
{
  "success": true,
  "message": "Profile updated successfully",
  "data": {
    "id": 11,
    "first_name": "Nour",
    "last_name": "Al Hashimi",
    "full_name": "Nour Al Hashimi",
    "email": "hala.alhashimi@gmail.com",
    "phone": "+971-50-111-1111",
    "avatar_url": null,
    "profile_image_url": "https://test.center-yazan.com/storage/users/profile_images/11/Nour_11.jpg",
    "is_active": true,
    "created_at": "2026-03-24T11:04:59.000000Z",
    "updated_at": "2026-03-28T07:34:15.000000Z"
  }
}
```

**What C# code does with this:**
```csharp
// Deserializes to ProfileUpdateApiResponse
var response = JsonSerializer.Deserialize<ProfileUpdateApiResponse>(json);

// Access response data
response.Success         // true ✓
response.Message         // "Profile updated successfully" ✓
response.Data.FirstName  // "Nour" ✓
response.Data.ProfileImageUrl // "https://test.center-yazan.com/storage/..." ✓

// Update ViewModel
UserFirstName = response.Data.FirstName;
Avatar = response.Data.ProfileImageUrl;  // Use server URL, not local path! ✓
```

---

### Error Response (HTTP 401)

```json
{
  "success": false,
  "message": "Unauthenticated"
}
```

**When you get this:**
- Token is missing or expired
- Need to login again
- Call `RefreshTokenAsync()` or logout/login

---

### Error Response (HTTP 422)

```json
{
  "success": false,
  "message": "The given data was invalid",
  "errors": {
    "first_name": ["The first name must be a string"],
    "image": ["The image must be a file of type: jpg, jpeg, png, gif, bmp, webp"]
  }
}
```

**When you get this:**
- Check field values are valid
- Image must be: jpg, jpeg, png, gif, bmp, or webp
- Image file size might be too large

---

## 🔄 C# Code Flow

### Sending Request

```csharp
public async Task<ProfileUpdateApiResponse> UpdateUserProfileAsync(string firstName, string avatarImagePath)
{
    // 1. Build form with data
    using (var form = new MultipartFormDataContent())
    {
        // Add first_name if provided
        if (!string.IsNullOrWhiteSpace(firstName))
            form.Add(new StringContent(firstName.Trim()), "first_name");
        
        // Add image if file exists
        if (!string.IsNullOrWhiteSpace(avatarImagePath) && File.Exists(avatarImagePath))
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(avatarImagePath);
            var fileContent = new ByteArrayContent(fileBytes);
            string mimeType = GetMimeType(avatarImagePath);  // Detects jpg, png, etc.
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            form.Add(fileContent, "image", Path.GetFileName(avatarImagePath));
        }
        
        // 2. Send request
        var httpResponse = await _httpClient.PostAsync(
            "https://test.center-yazan.com/api/profile",
            form
        );
        
        // 3. Read response
        string responseBody = await httpResponse.Content.ReadAsStringAsync();
        
        // 4. Deserialize
        if (httpResponse.IsSuccessStatusCode)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<ProfileUpdateApiResponse>(responseBody, options);
        }
    }
}
```

---

## ✅ Verification Checklist

### Request Side
- [ ] Field names are: `first_name` and `image` ✓
- [ ] At least ONE field has data ✓
- [ ] Authorization header has valid token ✓
- [ ] Image file exists (if uploading) ✓
- [ ] Image MIME type detected correctly ✓

### Response Side
- [ ] HTTP 200 or 201 received ✓
- [ ] Response has `success: true` ✓
- [ ] Response has `data` object ✓
- [ ] `data.ProfileImageUrl` contains server URL ✓
- [ ] App uses server URL (not local path) ✓

---

## 🧪 Postman Configuration

### Headers
```
Authorization: Bearer [PASTE_YOUR_TOKEN_HERE]
```
(Content-Type is auto-set by Postman for form-data)

### Body → form-data
```
KEY             | VALUE           | TYPE
─────────────────────────────────────────
first_name      | Nour            | Text
image           | [SELECT_FILE]   | File
```

### Test It
1. Click "Send"
2. Should get HTTP 200
3. Response body shows new profile_image_url
4. That URL is what should display in app

---

## 🎯 Key Points

1. **Field names matter**: Must be `first_name` and `image` (not Avatar)
2. **At least one required**: Can't send both empty
3. **Use server URL**: Don't store local file path
4. **Token required**: Must be logged in
5. **File path must exist**: If uploading image
6. **MIME type auto-detected**: jpg, png, gif, bmp, webp supported

---

## 📞 If It's Still Not Working

1. **Test in Postman first** (instructions above)
2. **Check Debug Console** (look for error messages)
3. **Verify token is valid** (not expired)
4. **Confirm at least one field has data**
5. **Share console output** if error persists

Then we can pinpoint the exact issue!

