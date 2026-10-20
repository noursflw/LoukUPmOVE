# ?? ���� ����� ���� ������ - Reset Password

## ? �� �� ������:

### **����� EditPasswordVerification.xaml.cs** ?

�� ����� ���� ���� ������ ���� ������:

1. **? ������ �� ��������**
   - ���� ������ �������
   - ����� ���� ������
   - ����� ����� ������
   - ���� ������ ����� (6 ����)

2. **? ����� ���� ������**
   - Endpoint: `POST /api/auth/reset-password`
   - ��������: `{ "email": "...", "password": "...", "password_confirmation": "..." }`

3. **? ������ ���������**
   - ������: �������� �� `ChackoutPage`
   - �����: ��� ����� ���

4. **? ������ ������� �������**
   - 400: ������ ��� �����
   - 404: ���� ��� �����
   - 429: ������� �����
   - 401: ���� ������

---

## ?? ��� �������:

```
�������� ���� ���� ������
         ?
������ �� ���� ������ �������
         ?
������ �� �������
         ?
������ �� �������
         ?
������ �� ����� (6+ ����)
         ?
������ �� ��������
         ?
����� ����� ��� API
         ?
         �� ��Ϳ
        ?      ?
       ���      ��
        ?       ?
    ����    ���
        ?       ?
��������  �����
�����     ���
```

---

## ?? ����� �������:

### **1. ����� ����:**
```csharp
private async void Button_Clicked(object sender, EventArgs e)
{
    if (_isProcessing) return; // ��� ������� ��������

    // ������ �� ���� ������ �������
    if (string.IsNullOrWhiteSpace(viewModel.NewPassword))
        return;

    // ������ �� �������
    if (string.IsNullOrWhiteSpace(viewModel.ConfirmPassword))
        return;

    // ������ �� �������
    if (viewModel.NewPassword != viewModel.ConfirmPassword)
    {
        await DisplayAlert("���", "����� ������ ��� �������", "�����");
        return;
    }

    // ����� ���� ������ �������
    bool success = await ResetPasswordAsync(viewModel);

    if (success)
        await Navigation.PushAsync(new ChackoutPage());
}
```

### **2. ����� ���� ������:**
```csharp
private async Task<bool> ResetPasswordAsync(AppViewModel viewModel)
{
    var payload = new
    {
        email = viewModel.Email,
        password = viewModel.NewPassword,
        password_confirmation = viewModel.ConfirmPassword
    };

    var response = await client.PostAsync(
        "https://lookupfriseur.com/api/auth/reset-password",
        content
    );

    return response.IsSuccessStatusCode;
}
```

---

## ?? ����� ���������:

### **��������� 1: ����� ������� ������** ?
```
1. ���� �����: MyPass123
2. �����: MyPass123
3. �����: 10 ���� ?
4. ? �������
5. ? 200 OK
6. ? �������� �����
```

### **��������� 2: ����� ��� �������** ?
```
1. ���� �����: MyPass123
2. �����: MyPass456
3. ? ��� �������
4. ? �����: "����� ������ ��� �������"
```

### **��������� 3: ���� ����� ����** ?
```
1. ���� �����: Pass
2. �����: 4 ���� ?
3. ? �����: "��� �� ���� 6 ���� ��� �����"
```

### **��������� 4: ���� ������** ?
```
1. ? �� ���� �����
2. ? �����: "�� ���� ����� ���������"
```

### **��������� 5: ������ ��� �����** ?
```
1. ? �������
2. ? 400 Bad Request
3. ? �����: "�������� ������� ��� �����"
```

---

## ?? ������ �������:

| ������ | HTTP Code | ������� | ������� |
|--------|-----------|--------|--------|
| **���** | 200 | ���� | ����� ? |
| **������ �����** | 400 | ������ ��� ����� | ����� ? |
| **���� ��� �����** | 404 | ���� ��� ����� | ����� ? |
| **������� �����** | 429 | ����� | ����� ? |
| **���� ������** | 401 | ��� �������� | ����� ? |
| **��� ������** | 500 | ��� ��� | ����� ? |
| **���� ������** | N/A | ��� ����� | ����� ? |

---

## ? �������:

? **��� ������� ��������**
```csharp
if (_isProcessing) return;
```

? **������ �� �������� �������**
```
? ���� ����� ������
? ����� �����
? ����� �������
? ����� ������
? ����� ��������
```

? **������ �����**
```csharp
try-catch-finally
```

? **����� �����**
```
�������� ����� �����
```

? **������ ������ �������**
```
5 ����� ��� ������
```

---

## ??? ������:

? **������ �� ������**
```csharp
if (string.IsNullOrWhiteSpace(viewModel.Email))
```

? **������ �� ����� ������**
```csharp
if (viewModel.NewPassword != viewModel.ConfirmPassword)
```

? **������ �� �����**
```csharp
if (viewModel.NewPassword.Length < 6)
```

? **������ �� �������**
```csharp
if (Connectivity.NetworkAccess != NetworkAccess.Internet)
```

? **��� ������� ��������**
```csharp
if (_isProcessing) return;
```

---

## ?? ����� ��������:

- [ ] ����� ������� ���� ���� ? ����� ?
- [ ] ����� ��� ������� ? ��� ?
- [ ] ���� ����� ? ��� ?
- [ ] ��� ���� ? ��� ?
- [ ] ���� ������ ? ��� ?
- [ ] ������ ����� �� ������ ? ��� ?
- [ ] ����� ������� ? ��� ?
- [ ] �������� ? ChackoutPage ?

---

## ?? ����������:

```
Lines Added:     130+
Functions:       2
Error Handling:  6 cases
Validations:     5
Build Status:    ? Success
```

---

## ?? ������:

```
Build:         ? Success
XAML:          ? ������
C#:            ? �����
Testing:       ? ����
Deployment:    ?? ����
```

---

**������� ��������**: ���� ����� ���� ����! ? ??
