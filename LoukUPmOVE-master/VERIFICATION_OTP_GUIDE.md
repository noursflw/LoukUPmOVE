# ?? ���� ������ �� OTP - Verification Page

## ? �� �� ������:

### **����� Verificationpage.xaml.cs** ?

�� ����� ���� ���� ������ �� ��� OTP:

1. **? ����� ����� ������**
   - Endpoint: `POST /api/auth/verify-otp`
   - ��������: `{ "email": "...", "code": "1234" }`

2. **? ������ ���������**
   - ������: �������� �� `EditPasswordVerification`
   - �����: ��� ����� ���

3. **? ������ �������**
   - 400: ����� ��� ����
   - 404: ������ ��� �����
   - 429: ������� �����
   - ���� ������

---

## ?? ��� �������:

```
�������� ���� ����� �������
         ?
������ �� ��� ���� �������
         ?
������ �� ������� ���������
         ?
����� ����� + ������ ��� API
         ?
         �� ���Ϳ
        ?      ?
       ���      ��
        ?       ?
    ����    ���
        ?       ?
��������  ��� �������
�������   ������ �����
```

---

## ?? ����� �������:

### **1. ����� ����:**
```csharp
private async void ConfirmCode_Clicked(object sender, EventArgs e)
{
    if (_isVerifying) return; // ��� ������� ��������

    // ������ �� ��� ���� �������
    if (fields.Any(f => string.IsNullOrWhiteSpace(f.Text)))
    {
        HighlightEmptyFields();
        return;
    }

    // ���� �����
    string code = string.Concat(fields.Select(f => f.Text));

    // ����� ������
    bool isValid = await VerifyOtpAsync(code);

    if (isValid)
    {
        await Navigation.PushAsync(new EditPasswordVerification());
    }
    else
    {
        // ��� ������ �����
        foreach (var field in fields)
            field.Text = string.Empty;
    }
}
```

### **2. ����� �����:**
```csharp
private async Task<bool> VerifyOtpAsync(string code)
{
    var payload = new
    {
        email = viewModel.Email,  // �� ViewModel
        code = code                 // ����� ������
    };

    var response = await client.PostAsync(
        "https://lookupfriseur.com/api/auth/verify-otp",
        content
    );

    if (response.IsSuccessStatusCode)
        return true;

    return false;
}
```

---

## ?? ����� ���������:

### **��������� 1: ��� ����** ?
```
1. �������� ����: 1234
2. ? ����� ����
3. ? �������� ����
4. ? ����� ������
5. ? 200 OK
6. ? �������� �������
```

### **��������� 2: ����� �����** ?
```
1. �������� ����: 12__
2. ? ������� ������� ����� ������
3. ? �����: "���� ����� ������"
```

### **��������� 3: ��� ��� ����** ?
```
1. �������� ����: 5678
2. ? ������� �����
3. ? �������� ����
4. ? ����� ������
5. ? 400 Bad Request
6. ? ������� ���� ������ ������
7. ? �����: "����� ��� ����"
```

### **��������� 4: ���� ������** ?
```
1. ? �� ���� �����
2. ? �����: "�� ���� �����"
```

### **��������� 5: ������� �����** ?
```
1. �������� ����� ���� �����
2. ? 429 Too Many Requests
3. ? �����: "����� ���� ����ɡ �����"
```

---

## ?? ������ �������:

| ������ | HTTP Code | ������� | ������� |
|--------|-----------|--------|--------|
| **��� ����** | 200 | ���� | ����� |
| **��� ����** | 400 | ��� ���� | ���� |
| **���� ��� �����** | 404 | ��� ����� | ����� |
| **������� �����** | 429 | ����� | ����� |
| **��� ������** | 500 | ��� ��� | ����� |
| **���� ������** | N/A | ��� ����� | ����� |

---

## ? �������:

? **��� ������� ��������**
```csharp
if (_isVerifying) return;
```

? **����� ������� �������**
```csharp
f.BackgroundColor = Colors.Red;
```

? **��� ������� ��� �����**
```csharp
field.Text = string.Empty;
```

? **������� ��������**
```csharp
Digit1.Focus();
```

? **������ �����**
```csharp
try-catch-finally
```

---

## ??? ������:

? **������ �� ������**
```csharp
if (string.IsNullOrWhiteSpace(viewModel.Email))
```

? **������ �� �������**
```csharp
if (Connectivity.NetworkAccess != NetworkAccess.Internet)
```

? **��� ������� ��������**
```csharp
if (_isVerifying) return;
```

? **������ �����������**
```csharp
catch (Exception ex)
```

---

## ?? ����� ��������:

- [ ] ��� ���� ���� ? ����� ?
- [ ] ����� ����� ? ����� ?
- [ ] ��� ���� ? ��� ���� ?
- [ ] ���� ������ ? ��� ?
- [ ] ����� ������� ? ��� ?
- [ ] �������� ? EditPasswordVerification ?

---

## ?? ����������:

```
Lines Added:     100+
Functions:       2
Error Handling:  5 cases
Safety Checks:   4
Build Status:    ? Success
```

---

## ?? ������:

```
Build:         ? Success
XAML:          ? �����
C#:            ? �����
Testing:       ? ����
Deployment:    ?? ����
```

---

**������� ��������**: ���� ���� ��� �����! ? ??
