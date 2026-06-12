using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using loukupm.Services;
using loukupm.Services;

namespace loukupm.ViewModel
{
    public class PaymentViewModel : INotifyPropertyChanged
    {
        private readonly ApiServices _apiServices;
        private bool _isBusy;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;
                _isBusy = value;
                OnPropertyChanged();
                (PayCommand as Command)?.ChangeCanExecute();
            }
        }

        public ICommand PayCommand { get; }

        public PaymentViewModel(ApiServices apiServices)
        {
            _apiServices = apiServices;
            PayCommand = new Command(async () => await ExecutePayAsync(), () => !IsBusy);
        }

        private async Task ExecutePayAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var session = await _apiServices.CreateCheckoutSessionAsync(
                    amount: 5000,
                    currency: "usd",
                    email: "email@example.com"
                );

                if (session == null || string.IsNullOrEmpty(session.Url))
                {
                    await CommunityToolkit.Maui.Alerts.Toast.Make("Fehler beim Erstellen der Zahlung.", CommunityToolkit.Maui.Core.ToastDuration.Short, 14).Show();
                    return;
                }

                await Browser.OpenAsync(session.Url, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await CommunityToolkit.Maui.Alerts.Toast.Make($"Fehler: {ex.Message}", CommunityToolkit.Maui.Core.ToastDuration.Short, 14).Show();
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


