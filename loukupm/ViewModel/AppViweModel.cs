using CommunityToolkit.Mvvm.ComponentModel;
using loukupm.Model;
using loukupm.services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;


namespace loukupm.ViewModel
{
    public partial class AppViewModel : ObservableObject
    {
        // Flags (Skeleton Loading)
        [ObservableProperty] private bool isCouselLoad;
        [ObservableProperty] private bool isWorkTeamLoad;
        [ObservableProperty] private bool isServicesLoad;
        [ObservableProperty] private bool isCatogory;
        [ObservableProperty] private bool isloadday;
        [ObservableProperty] private bool isTimework;
        [ObservableProperty] private bool invoiceLoad;
        [ObservableProperty] private bool isloadBooking;
        [ObservableProperty] private bool isLoadNotifiction;
        /// <summary>
        /// /loclazion
        /// </summary>

        // Collections

        [ObservableProperty] private ObservableCollection<Servies> services;
        [ObservableProperty] private ObservableCollection<Servies> filteredServices;
        [ObservableProperty] private ObservableCollection<Booking> bookings;
        [ObservableProperty] private ObservableCollection<WorkTeam> workTeams;
        [ObservableProperty] private ObservableCollection<Notifiction> notifications;

        private readonly ApiServices _apiServices = new ApiServices();
        private static AppViewModel _instance;     // يخزن النسخة الوحيدة
        public static AppViewModel Instance        // واجهة الوصول العامة
            => _instance ??= new AppViewModel();

        public ICommand SelectServiceButtonCommand { get; }
        public AppViewModel()
        {
            LoadData();

            // Fire and forget
            _ = LoadBookingsAsync();
            _ = LoadNotificationsAsync();
            _ = LoadWorkTeamsAsync();
            _ = LoadServicesAsync();
            ConfirmCommand = new Command(async () => await SendCodeAsync());
            PostEmailCommand = new Command<string>(async (email) => await PostEmailAsync(email));
            ChangePasswordCommand = new Command(async () => await ChangePasswordAsync());
            PostBookingCommand = new Command(async () => await PostBookingAsync());
            SelectServiceButtonCommand = new Command<Servies>(service =>
            {
                if (service == null) return;

                SelectedService = service;
                CurrentBooking.ServiceName = service.NameServies;
            });


        }


        ///Section Load Data    
        private void LoadData()
        {
            IsCouselLoad = true;
            IsWorkTeamLoad = true;
            IsServicesLoad = true;
            IsCatogory = true;
            Isloadday = true;
            IsTimework = true;
            InvoiceLoad = true;
            IsloadBooking = true;
            IsLoadNotifiction = true;
        }

        private async Task LoadBookingsAsync()
        {
            try
            {
                var data = await _apiServices.GetBookingsAsync();
                Bookings = new ObservableCollection<Booking>(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading bookings: {ex.Message}");
            }
            finally
            {
                IsloadBooking = false;
            }
        }

        private async Task LoadNotificationsAsync()
        {
            try
            {
                var data = await _apiServices.GetNotifictionsAsync();
                Notifications = new ObservableCollection<Notifiction>(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading notifications: {ex.Message}");
            }
            finally
            {
                IsLoadNotifiction = false;
            }
        }

        private async Task LoadWorkTeamsAsync()
        {
            try
            {
                var data = await _apiServices.GetWorkTeamsAsync();
                WorkTeams = new ObservableCollection<WorkTeam>(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading work teams: {ex.Message}");
            }
            finally
            {
                IsWorkTeamLoad = false;
            }
        }

        private async Task LoadServicesAsync()
        {
            try
            {
                var data = await _apiServices.GetServiesasync(); // ✅ مو WorkTeams
                Services = new ObservableCollection<Servies>(data);

                // نسخة للفلترة
                FilteredServices = new ObservableCollection<Servies>(Services);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading services: {ex.Message}");
            }
            finally
            {
                IsServicesLoad = false;
            }
        }

        // فلترة الخدمات حسب النوع
        public void FilterServices(string type)
        {
            if (string.IsNullOrWhiteSpace(type) || type == "الكل")
            {
                FilteredServices = new ObservableCollection<Servies>(Services);
            }
            else
            {
                FilteredServices = new ObservableCollection<Servies>(
                    Services?.Where(s => s.Catgery == type) ?? Enumerable.Empty<Servies>()
                );
            }
        }


        ///post For send Email use on forget password 
        public ICommand PostEmailCommand { get; set; }
        public async Task PostEmailAsync(string email)
        {
            using var client = new HttpClient();
            var json = JsonSerializer.Serialize(new { email });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                // ضع عنوان الـ API الصحيح هنا
                var response = await client.PostAsync("https://example.com/api/send-email", content);
                if (response.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("تم", "تم إرسال البريد الإلكتروني بنجاح", "موافق");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("فشل", "فشل في إرسال البريد الإلكتروني", "موافق");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("خطأ", ex.Message, "موافق");
            }
        }

        //section OTP FORGET PASSWORD
        public string Digit1 { get; set; }
        public string Digit2 { get; set; }
        public string Digit3 { get; set; }
        public string Digit4 { get; set; }

        public ICommand ConfirmCommand { get; }
        private async Task SendCodeAsync()
        {
            string code = $"{Digit1}{Digit2}{Digit3}{Digit4}";
            if (code.Length < 4)
            {
                await App.Current.MainPage.DisplayAlert("خطأ", "الرجاء إدخال الكود الكامل", "موافق");
                return;
            }

            using var client = new HttpClient();
            var json = JsonSerializer.Serialize(new { code });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // ضع عنوان الـ API الصحيح هنا
                var response = await client.PostAsync("https://example.com/api/verify", content);
                if (response.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("تم", "تم التحقق بنجاح", "موافق");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("فشل", "كود غير صالح", "موافق");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("خطأ", ex.Message, "موافق");
            }
        }
        [ObservableProperty]
        private Servies selectedService;
       




        //Post for new password
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        public ICommand ChangePasswordCommand { get; }

        private async Task ChangePasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                await App.Current.MainPage.DisplayAlert("خطأ", "يرجى إدخال كلمة المرور في كلا الحقلين", "موافق");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                await App.Current.MainPage.DisplayAlert("خطأ", "كلمة المرور غير متطابقة", "موافق");
                return;
            }

            using var client = new HttpClient();
            var json = JsonSerializer.Serialize(new { newPassword = NewPassword });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://example.com/api/reset-password", content);
                if (response.IsSuccessStatusCode)
                {
                    await App.Current.MainPage.DisplayAlert("تم", "تم تحديث كلمة المرور بنجاح", "موافق");
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("فشل", "فشل في تحديث كلمة المرور", "موافق");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("خطأ", ex.Message, "موافق");
            }
        }

        ////End Section OTP FORGET PASSWORD  

        ///Section Booking 
        ///Sectipn Post Booking
        [ObservableProperty] private WorkTeam selectedProvider;
        [ObservableProperty] private string selectedServiceName;
        [ObservableProperty] private DateTime selectedDate;
        [ObservableProperty] private TimeSpan selectedTime;

        public Booking CurrentBooking { get; set; } = new();

        public ICommand PostBookingCommand { get; }

        public async Task PostBookingAsync()
        {
            var booking = CurrentBooking;
            if (booking == null) return;

            using var client = new HttpClient();
            var json = JsonSerializer.Serialize(booking);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://example.com/api/bookings", content);
                response.EnsureSuccessStatusCode();
                await App.Current.MainPage.DisplayAlert("تم", "تم إرسال الحجز بنجاح", "موافق");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("خطأ", ex.Message, "موافق");
            }

        }
        ///Section Remove Booking



        /// End Section Booking



        ///Section Invoice
        ///

        /// End Section Invoice
        /// 

        ///Section Post Payment Stripe



        /// End Section Post Payment Stripe
        /// 


        ///Section Post User
        ///


        /// End Section Post User

    }







}


