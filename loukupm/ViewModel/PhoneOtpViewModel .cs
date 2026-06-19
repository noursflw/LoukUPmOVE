using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loukupm.Services;
using static loukupm.Model.Auth;

namespace loukupm.ViewModel
{
    public partial class PhoneOtpViewModel : ObservableObject
    {
        private readonly ApiServices _api;

        public PhoneOtpViewModel(ApiServices api)
        {
            _api = api;
            System.Diagnostics.Debug.WriteLine("OTP VM CREATED");
        }

        // ========================
        // 📦 Properties
        // ========================

        [ObservableProperty]
        private string phone;

        [ObservableProperty]
        private string otp;

        [ObservableProperty]
        private bool otpSent;

        [ObservableProperty]
        private bool isVerified;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string message;

        // ========================
        // 🚀 Commands
        // ========================

        [RelayCommand]
        private async Task SendOtp()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Message = "";

                if (string.IsNullOrWhiteSpace(Phone))
                {
                    Message = "الرجاء إدخال رقم الهاتف";
                    return;
                }

                var result = await _api.SendPhoneOtpAsync(Phone);

                if (result)
                {
                    OtpSent = true;
                    Message = "تم إرسال رمز التحقق";
                    await NavigationService.NavigateToPage(
               NavigationService.ROUTE_OTP_PHONE_NUMBER);

                }
                else
                {
                    Message = "فشل إرسال الرمز";
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ========================
        // 📥 Verify OTP
        // ========================

        [RelayCommand]
        private async Task VerifyOtp()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Message = "";

                if (string.IsNullOrWhiteSpace(Otp))
                {
                    Message = "أدخل رمز التحقق";
                    return;
                }

                var result = await _api.VerifyPhoneOtpAsync(Phone, Otp);

                if (result)
                {
                    IsVerified = true;
                    Message = "تم التحقق بنجاح 🎉";
                }
                else
                {
                    Message = "رمز غير صحيح";
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ========================
        // 🔁 Resend OTP
        // ========================

        [RelayCommand]
        private async Task ResendOtp()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Message = "";

                var result = await _api.SendPhoneOtpAsync(Phone);

                if (result)
                    Message = "تم إعادة إرسال الرمز";
                else
                    Message = "فشل إعادة الإرسال";
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}