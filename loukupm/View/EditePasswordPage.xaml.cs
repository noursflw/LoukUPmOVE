using loukupm.ViewModel;
using loukupm.Services;
using System.Text.RegularExpressions;

namespace loukupm.View;

public partial class EditePasswordPage : ContentPage
{
    public EditePasswordPage()
    {
        InitializeComponent();
        // Must use the singleton — a new VM does not share profile/session state
        // and logout/re-auth after password change would not refresh the live UI.
        BindingContext = AppViewModel.Instance;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is AppViewModel vm)
        {
            vm.CurrentPassword = string.Empty;
            vm.Password = string.Empty;
            vm.ConfirmPassword = string.Empty;
        }
    }

    /// <summary>
    /// معالج زر العودة - يستخدم نظام الملاحة المركزي
    /// يتبع القاعدة: صفحات تدفق الملف الشخصي → //ProfilePage مباشرة
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigationService.HandleBackButton(NavigationService.ROUTE_EDIT_PASSWORD);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EditePasswordPage] Back button error: {ex.Message}");
                    Console.WriteLine($"[EditePasswordPage] Exception: {ex.GetType().Name}");
                }
            });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EditePasswordPage] OnBackButtonPressed crash: {ex.Message}");
            return true; // Prevent crash
        }
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        try
        {
            await NavigationService.HandleBackButton(NavigationService.ROUTE_EDIT_PASSWORD);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EditePasswordPage] Button click navigation error: {ex.Message}");
        }
    }

    private bool _hasMinLength;
    private bool _hasUppercase;
    private bool _hasNumber;
    private bool _isPasswordValid;
    private void RegisterPasswordEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        var password = e.NewTextValue ?? "";

        _hasMinLength = password.Length >= 8;
        _hasUppercase = Regex.IsMatch(password, "[A-Z]");
        _hasNumber = Regex.IsMatch(password, "[0-9]");

        UpdatePasswordUI();
        ValidateForm();
    }
    private void UpdatePasswordUI()
    {
        SetRuleUI(LengthFrame, LengthIcon, _hasMinLength);
        SetRuleUI(UpperFrame, UpperIcon, _hasUppercase);
        SetRuleUI(NumberFrame, NumberIcon, _hasNumber);
    }

    private void SetRuleUI(Frame frame, Label icon, bool isValid)
    {
        frame.BackgroundColor = isValid ? Colors.Green : Colors.Red;
        icon.Text = isValid ? "✔" : "○";
    }
    private void ValidateForm()
    {


        var isPasswordValid = _hasMinLength && _hasUppercase && _hasNumber;

        RegisterButton.IsEnabled =isPasswordValid;
    }

}