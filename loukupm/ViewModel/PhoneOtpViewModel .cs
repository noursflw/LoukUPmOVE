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


        // ========================
        // 📥 Verify OTP
        // ========================

       
    }
}