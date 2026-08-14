# ���� ����� ����� ����� ���� ������ - Change Password Fix Guide

## ?? ������� ��������
**�����:** `UnprocessableEntity (422)`
- ��� ����� ���� �� �������� ������� �� ������ ������ �� ������� API

## ? ���� ������

### 1. **����� Endpoint**
```csharp
// ? ������
"https://lookupfriseur.com/api/auth/reset-password"

// ? ������
"https://lookupfriseur.com/api/auth/change-password"
```

### 2. **���� �������� �������**
```json
{
  "current_password": "CurrentPassword123!",
  "password": "NewPassword123!",
  "password_confirmation": "NewPassword123!"
}
```

### 3. **������ ������� ��������**

| ����� | ����� | ���� |
|------|------|-----|
| ���� ���� ��� ����� | 401 (Unauthorized) | ��� �����: "���� ������ ������� ��� �����" |
| ������ ��� ����� | 422 (Unprocessable Entity) | ������� ����� ����� ��������� �� API |
| ��� ��� | 400 (Bad Request) | ������ �� ���� �������� |
| ���� | 200 | ����� ������ ���� ����� ������ |

### 4. **������ �� �������� ��� �������**

```csharp
// ������ �� ���� ������ ���� ���� ������
if (password.Length < 8)
{
    await Toast.Make("���� ������ ������� ��� �� ���� 8 ���� ��� �����").Show();
    return;
}

// ������ �� ��� ����� ����� ������
if (password != confirmPassword)
{
    await Toast.Make("����� ������ ������� ��� �������").Show();
    return;
}

// ������ �� ������ ���� ������ ������� �� �������
if (password == CurrentPassword)
{
    await Toast.Make("���� ������ ������� ��� �� ���� ������ �� �������").Show();
    return;
}
```

### 5. **����� �������� (Logging)**

```csharp
Console.WriteLine("?? Sending password change request:");
Console.WriteLine($"   Current Password: ***");
Console.WriteLine($"   New Password: ***");
Console.WriteLine($"   Confirmation: ***");
Console.WriteLine($"   Password Length: {password.Length}");

Console.WriteLine($"?? JSON Payload: {json}");
Console.WriteLine($"?? Response Status: {response.StatusCode}");
Console.WriteLine($"?? Response Body: {responseBody}");
```

## ?? ������� ���������

### 1. **loukupm/ViewModel/AppViweModel.cs**
- ? ����� `ChangeUserPasswordAsync()` method
- ? ����� ������ ����� �������
- ? ����� ������� `Auth.ErrorResponse`
- ? ����� ������ �� ��������

### 2. **loukupm/View/EditePasswordPage.xaml**
- ? ��� ������ �� Bindings �������:
  - `CurrentPassword` ? ���� ������ �������
  - `Password` ? ���� ������ �������
  - `ConfirmPassword` ? ����� ������ �������
- ? ������� Localization (�����)

### 3. **loukupm/View/EditePasswordPage.xaml.cs**
- ? ��� ������ �� `AppViewModel` instance

## ?? ������� ���� ������

��� ���� APIs �����ɡ ���� ������ ��� ��:
- ? ���� 8 ���� ��� �����
- ? ����� ��� ���� �����
- ? ����� ��� ���� �����
- ? ����� ��� �����
- ? ����� ��� ���� ���� (Optional)

## ?? ����� ��������

### 1. **������ ����**
```
���� ������ �������: Password123!
���� ������ �������: NewPassword456!
����� ������ �������: NewPassword456!
?
�������: ? �� ����� ���� ������ �����
```

### 2. **������ ��� - ��� �����**
```
���� ������ �������: Password123!
���� ������ �������: NewPassword456!
����� ������ �������: Different789!
?
�������: ? ����� ������ ������� ��� �������
```

### 3. **������ ��� - ���� ���� ����� �����**
```
���� ������ �������: WrongPassword!
���� ������ �������: NewPassword456!
����� ������ �������: NewPassword456!
?
�������: ? ���� ������ ������� ��� ����� [401]
```

## ?? ������� ������

?? **������� ���� ����:**

1. **�� ���� ����� ������**
   - ��� ������ ���� `string` �����
   - ��� ����� ��� ������� ������

2. **������� HTTPS ���**
   - ������� ������ `https://` ����� �� `http://`

3. **����� �������� ������� �� �������**
   - ���� `***` ����� �� ����� ������ �������

4. **������ �� ��� ��� Token**
   - ��� ������ �� ���� Token ��� �������
   - ��� ����� Authorization Header

## ?? ����� �����

��� ������ �������:

1. **���� �� ��� API Endpoint**
   - �� ��� URL ���Ϳ
   - �� ��� Method ���� (POST)�

2. **���� �� ������� ���� ������**
   - �� ����� ��� 8 ���� ��� �����
   - �� ����� ��� ���� ������

3. **���� �� ��� Token**
   - �� Token ���Ϳ
   - �� Token ����� �������ɿ

4. **���� �� �������**
   - ���� ���� Console �������� �������
   - ���� �� ����� ����� �� API

## ? ������� ��������

- ? ����� ���� �������
- ? ����� ��� ����� ��������
- ? ������ ���� ����� ����� �������
- ? ����� �������� ��� ������
- ? ��� ������� (Localization)

---
**��� �����:** 2024
**������:** ? ��� ����
