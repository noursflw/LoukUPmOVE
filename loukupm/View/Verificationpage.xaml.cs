using CommunityToolkit.Maui.Views;
using loukupm.View.MassgingApp;
using loukupm.ViewModel;
using loukupm.Services;
using System.Linq;

namespace loukupm.View
{
    /// <summary>
    /// Optional OTP-only step (legacy). Main flow now goes RestPassword → EditPasswordVerification.
    /// </summary>
    public partial class Verificationpage : ContentPage
    {
        private bool _isVerifying;

        public Verificationpage()
        {
            InitializeComponent();
            Shell.SetNavBarIsVisible(this, false);
            BindingContext = AppViewModel.Instance;
        }

        protected override bool OnBackButtonPressed()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await NavigationService.HandleBackButton("Verificationpage");
            });
            return true;
        }

        private async void ConfirmCode_Clicked(object sender, EventArgs e)
        {
            if (_isVerifying) return;

            try
            {
                _isVerifying = true;
                if (VerifyButton != null)
                    VerifyButton.IsEnabled = false;

                var code = OtpField?.Text?.Trim();
                if (string.IsNullOrWhiteSpace(code) && BindingContext is AppViewModel boundVm)
                    code = boundVm.OtpCode?.Trim();

                code = new string((code ?? string.Empty).Where(char.IsDigit).ToArray());

                if (string.IsNullOrWhiteSpace(code))
                {
                    await DisplayAlert("تنبيه", "يرجى إدخال الرمز", "حسناً");
                    return;
                }

                if (code.Length < 4)
                {
                    await this.ShowPopupAsync(new CodeNotIncorrect());
                    return;
                }

                var viewModel = BindingContext as AppViewModel ?? AppViewModel.Instance;
                var email = PasswordResetSession.HasEmail
                    ? PasswordResetSession.Email
                    : viewModel?.Email?.Trim();

                if (string.IsNullOrWhiteSpace(email))
                {
                    await this.ShowPopupAsync(new EmaileIsNotFound());
                    return;
                }

                PasswordResetSession.SetEmail(email);
                PasswordResetSession.SetOtp(code);
                if (viewModel != null)
                {
                    viewModel.Email = email;
                    viewModel.OtpCode = code;
                }

                await Navigation.PushAsync(new EditPasswordVerification(email));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Verificationpage] ConfirmCode error: {ex}");
                try
                {
                    await Navigation.PushAsync(new EditPasswordVerification(PasswordResetSession.Email));
                }
                catch
                {
                    await this.ShowPopupAsync(new NoServerResponse());
                }
            }
            finally
            {
                _isVerifying = false;
                if (VerifyButton != null)
                    VerifyButton.IsEnabled = true;
            }
        }
    }
}
