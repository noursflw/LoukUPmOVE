# ? ���� ���� - ����� ���� ������

## ? �� �� ������:

### **EditPasswordVerification.xaml.cs** ?

```csharp
// 1. ����� ����
private async void Button_Clicked(object sender, EventArgs e)
{
    // ��� ������� ��������
    // ������ �� ��������
    // ����� ���� ������ �������
    // �������� ��� ������
}

// 2. ����� ���� ������
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

## ?? ������ �� ��������:

| ������� | ����� |
|--------|-------|
| **���� �����** | ? ������ |
| **�����** | ? ����� |
| **�������** | ? ������� |
| **�����** | ? 6+ ���� |
| **��������** | ? ���� |

---

## ?? �������:

| ��������� | ������� |
|---------|--------|
| ���� | ? ����� |
| ��� ������ | ? ��� |
| ���� ���� | ? ��� |
| ��� ���� | ? ��� |
| ���� ������ | ? ��� |
| ��� ���� | ? ��� |

---

## ?? �������:

```
? ���� ? ChackoutPage
? ��� ? ����� ���
```

---

## ? ������:

```
Build:       ? ���
Updates:     ? ����
Testing:     ? ����
Deployment:  ?? ����
```

---

**���� ��������� ������! ??**
