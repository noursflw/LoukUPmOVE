using CommunityToolkit.Maui.Views;
using loukupm.View.MassgingApp;
using loukupm.ViewModel;
using loukupm.Services;
using Microsoft.Maui.Controls;
using System.Text;
using System.Text.Json;

namespace loukupm.View
{
    public partial class Verificationpage : ContentPage
    {
        private bool _isVerifying = false;

        public Verificationpage()
        {
            InitializeComponent();
            Shell.SetNavBarIsVisible(this, false);
            this.BindingContext = AppViewModel.Instance;
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
                var code = OtpField.Text;

                if (string.IsNullOrWhiteSpace(code))
                {
                    await DisplayAlert("تنبيه", "يرجى إدخال الرمز", "حسناً");
                    return;
                }

                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    var popup = new NoEnternetConacted();
                    await this.ShowPopupAsync(popup);
                    return;
                }

                _isVerifying = true;

                bool isValid = await VerifyOtpAsync(code);

                if (isValid)
                {
                    var popup = new SuccessfullyVerified();
                    await this.ShowPopupAsync(popup);
                    await Navigation.PushAsync(new EditPasswordVerification());
                }
                else
                {
                    var popup = new CodeNotIncorrect();
                    await this.ShowPopupAsync(popup);

                    // تفريغ الحقل الوحيد
                    OtpField.Text = string.Empty;
                }
            }
            catch
            {
                await this.ShowPopupAsync(new NoServerResponse());
            }
            finally
            {
                _isVerifying = false;
            }
        }

        private async Task<bool> VerifyOtpAsync(string code)
        {
            try
            {
                using var client = new HttpClient();

                var viewModel = BindingContext as AppViewModel;

                if (viewModel == null || string.IsNullOrWhiteSpace(viewModel.Email))
                {
                    await this.ShowPopupAsync(new EmaileIsNotFound());
                    return false;
                }

                var payload = new
                {
                    email = viewModel.Email,
                    code = code,
                    type = 1
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "https://test.center-yazan.com/api/auth/forgot-password",
                    content
                );

                if (response.IsSuccessStatusCode)
                    return true;

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    await this.ShowPopupAsync(new CodeNotIncorrect());
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    await this.ShowPopupAsync(new EmaileIsNotFound());
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    await this.ShowPopupAsync(new WateResposeOTP());

                return false;
            }
            catch
            {
                await this.ShowPopupAsync(new NoServerResponse());
                return false;
            }
        }
    }
}