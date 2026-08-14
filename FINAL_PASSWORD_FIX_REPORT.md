# ? ����� ����� ��� ����� ���� ������ �������

## ?? ���� ������

�� �� ����� ����� `UnprocessableEntity (422)` ��� ������ ����� ���� ������ �����.

## ?? ������� �������

```
Error: UnprocessableEntity (422)
Endpoint: POST /api/auth/reset-password
Status: ? ���
Reason: �������� ������� �� ����� ������� API
```

## ?? ���� ������

### 1?? ����� ��� Endpoint

```diff
? Before:
POST https://lookupfriseur.com/api/auth/reset-password

? After:
POST https://lookupfriseur.com/api/auth/change-password
```

### 2?? ���� �������� �������

```json
{
  "current_password": "CurrentPassword123!",
  "password": "NewPassword123!",
  "password_confirmation": "NewPassword123!"
}
```

### 3?? ������ ������� �������

��� ����� ������ ����� ����� ����� ��������:

| ����� | ������ | ������� |
|------|--------|--------|
| 200 | ? ���� | ��� ����� ������ ������ �������� |
| 400 | Bad Request | �����: "��� ��� ����" |
| 401 | Unauthorized | �����: "���� ������ ������� ��� �����" |
| 422 | Validation Error | ������� ������ ����� �� API |
| Others | ��� ��� | ����� ��� ���� |

### 4?? ������ �� ��������

```csharp
? ��� ��� ������ �����
? ���� ������ 8 ����
? ����� ����� ������ �������
? ������ ������ ������ �� ������
```

## ?? ������� ���������

### ?? loukupm/ViewModel/AppViweModel.cs
- ? ����� method `ChangeUserPasswordAsync()`
- ? ����� ������ ����� �������
- ? ����� ������ �� ��������
- ? ����� ����� ������ (Logging)
- ? ������� `Auth.ErrorResponse`

### ?? loukupm/View/EditePasswordPage.xaml
- ? ��� ���� ��� TextFields
- ? ������� Localization
- ? ��� ���/����� ����� ������

### ?? loukupm/View/EditePasswordPage.xaml.cs
- ? ��� ViewModel ���� ����

## ?? ��������� �������

1. **��� ����� ����� ������**
   ```csharp
   // ��� ������
   CurrentPassword = string.Empty;
   password = string.Empty;
   confirmPassword = string.Empty;
   ```

2. **��� ����� ����� ������**
   ```csharp
   Console.WriteLine("Current Password: ***");
   ```

3. **������� HTTPS ���**
4. **������ �� Token ��� �������**

## ?? ����� ��������

### ? ������ �������
```
Input:
- Current: Password123!
- New: NewPassword456!
- Confirm: NewPassword456!

Expected Output: ? �� ����� ���� ������ �����
```

### ? ����� �����

#### Wrong Current Password
```
Input: ���� ���� ����� �����
Output: ? ���� ������ ������� ��� ����� [401]
```

#### Mismatched Passwords
```
Input: ����� ����� ��� �������
Output: ? ����� ������ ������� ��� ������� [local]
```

#### Validation Error
```
Input: ������ ��� �����
Output: ? ��� �� ��������: [details] [422]
```

## ?? ������� (Console Output)

```
?? Sending password change request:
   Current Password: ***
   New Password: ***
   Confirmation: ***
   Password Length: 12

?? JSON Payload: {"current_password":"***","password":"***","password_confirmation":"***"}

?? Response Status: 200

?? Response Body: {"success":true,"message":"Password changed successfully"}

? Password changed successfully
```

## ?? ������� �������� �������

- ? ����� ���� ��������
- ? ����� ��� ����� ������
- ? ������ ���� �����������
- ? ��� ������� �������
- ? ����� �������� ��� ������
- ? ������ �� ��� ���� ������

## ?? ������ ������

| ������� | ��� | ��� |
|-------|------|------|
| ������ ������� | ? ����� | ? ����� |
| ����� �������� | ? ���� | ? ����� |
| ������ | ? ����� | ? ����� |
| ������� | ? ����� | ? ������� |
| ������ | ? ���� | ? ����� |

## ?? ����� �����

��� ������ �������:

1. **���� �� ��� API Endpoint**
   - URL ����: ?
   - Method ����: ? POST
   - Token ����: ?

2. **���� �� ������� ���� ������**
   - 8 ���� ��� �����: ?
   - ���� ������: ?
   - ���� �����/�����: ?

3. **���� �� �������**
   - ���� Console logs
   - ���� �� ����� ����� �� API

## ?? ������� ����

?? **���� ��:**
1. ����� ���� ������ ������� ���� ����
2. �� ���� ������ ������� ���� (8+ ���ݡ ���� �����/����ɡ �����)
3. ����� ����� ������ �������
4. ���� ����� ������

## ?? ������� ��������

| ������ | ������ |
|--------|--------|
| ������ | ? ��� |
| ������� | ? ������ |
| ������ | ? ����� |
| ������� | ? ������ |
| ������� | ? ������ |
| �������� | ? ���� |

---

## ?? ����������

- **����� �������:** 1
- **���� ��� �����:** ~120
- **������� �����:** 5+
- **����� ���:** 7
- **����� ������:** 4+

---

**�������:** 2024
**������:** ? **��� ����**
**�������:** 1.0

