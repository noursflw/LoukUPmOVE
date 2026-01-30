using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loukupm.Langue;
using loukupm.Model;
using loukupm.services;
using loukupm.View;
using loukupm.View.MassgingApp;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using loukupm.Langue;


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
        [ObservableProperty] private bool isLoadUser;

        /// <summary>
        /// /loclazion
        /// </summary>

        // Collections

        [ObservableProperty] private ObservableCollection<Servies> services = new();
        [ObservableProperty] private ObservableCollection<Servies> filteredServices = new();

        [ObservableProperty] private ObservableCollection<Booking> bookings = new();
        [ObservableProperty] private ObservableCollection<Appointment> appointments = new();

        [ObservableProperty] private ObservableCollection<WorkTeam> filteredWorkTeams = new();
        [ObservableProperty] private ObservableCollection<WorkTeam> workTeams = new();
        [ObservableProperty] private ObservableCollection<Notifiction> notifications;
        [ObservableProperty] private ObservableCollection<Booking> viewSrves;
        [ObservableProperty] private ObservableCollection<PolicyandPrivacyS> listpolicyandPrivacy = new();

        [ObservableProperty]
        private ObservableCollection<Servies> selectedServices = new();

        private static readonly Lazy<AppViewModel> _instance = new(() => new AppViewModel());
        public static AppViewModel Instance => _instance.Value;

        private readonly ApiServices _apiServices = new ApiServices();

        private readonly string _token;
        public ICommand SelectServiceButtonCommand { get; }
        private readonly HttpClient _httpClient;
        public AppViewModel()
        {
            LoadData();
            _httpClient = new HttpClient();
            // Fire and forget
            _ = LoadBookingsAsync();
            _ = LoadNotificationsAsync();
            _ = LoadWorkTeamsAsync();
            _ = LoadServicesAsync();
            _ = LoadUser();
            _ = LoadBookingsAsync();
            _token = SecureStorage.GetAsync("auth_token").Result;
            DeleteAccountCommand = new Command(async () => await DeleteAccountAsync());
            ConfirmCommand = new Command(async () => await SendCodeAsync());
            ProviderDays = new ObservableCollection<DayItem>();
            SelectDayCommand = new Command<DayItem>(OnSelectDay);
            LoadCurrentWeekDays();
            ChangePasswordCommand = new Command(async () => await ChangePasswordAsync());
            PostBookingCommand = new AsyncRelayCommand(PostBookingAsync);
            UpdateUserCommand = new Command(async () => await UpdateUserInfo());
            ChangePasswordUserCommand = new Command(async () => await ChangeUserPasswordAsync());
            SelectServiceButtonCommand = new Command<Servies>(service =>
            {
                if (service == null) return;

                // استخدام الـ Id للمقارنة بدلاً من المرجع
                var exists = SelectedServices.Any(s => s.Id == service.Id);

                if (!exists)
                {
                    SelectedServices.Add(service);  // ✨ إضافة للـ Collection
                    CurrentBooking.SelectedServices.Add(service);  // إضافة للـ List
                    Console.WriteLine($"✅ Service added: {service.NameServies}");
                }
                else
                {
                    var serviceToRemove = SelectedServices.First(s => s.Id == service.Id);
                    SelectedServices.Remove(serviceToRemove);  // ✨ حذف من Collection
                    CurrentBooking.SelectedServices.Remove(serviceToRemove);  // حذف من List
                    Console.WriteLine($"❌ Service removed: {service.NameServies}");
                }

                // طباعة القائمة للمراجعة
                Console.WriteLine("📋 Current Selected Services:");
                foreach (var s in SelectedServices)
                    Console.WriteLine($"   - {s.NameServies} (${s.PriceServies})");
            });

        }
        private async Task SetAuthorizationHeaderAsync()
        {
            // استدعاء SecureStorage بشكل غير متزامن
            string? token = await SecureStorage.GetAsync("auth_token");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            }
            else
            {
                // إزالة أي Authorization قديمة إذا لم يكن هناك توكن
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }


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



        //private async Task LoadBookingsAsync()
        //{
        //    if (currentUser == null)
        //        return; // تأكد من وجود المستخدم قبل تحميل الحجوزات

        //    IsloadBooking = true;

        //    try
        //    {
        //        var data = await _apiServices.GetUserAppointmentsAsync(currentUser);

        //        if (data == null || data.Count == 0)
        //            return;

        //        Bookings.Clear();

        //        foreach (var item in data)
        //            Bookings.Add(item);
        //    }
        //    finally
        //    {
        //        IsloadBooking = false;
        //    }
        //}
        public IAsyncRelayCommand LoadAppointmentsCommand { get; }
        private async Task LoadBookingsAsync()
        {
            if (currentUser == null)
                return;

            IsloadBooking = true;

            try
            {
                var data = await _apiServices.GetUserAppointmentsAsync(currentUser);

                if (data == null || data.Count == 0)
                    return;

                Appointments.Clear();

                foreach (var item in data)
                    Appointments.Add(item);
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
                IsWorkTeamLoad = true;

                var data = await _apiServices.GetWorkTeamsAsync();


                if (data == null || data.Count == 0)
                    return;


                WorkTeams.Clear();


                foreach (var member in data)
                    WorkTeams.Add(member);


                FilteredWorkTeams = new ObservableCollection<WorkTeam>(WorkTeams);



                Console.WriteLine($"✅ Loaded {WorkTeams.Count} work team members");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading work teams: {ex}");
            }
            finally
            {
                IsWorkTeamLoad = false;
            }
        }

        [ObservableProperty]
        private WorkTeam selectedWorkTeam;

        public IRelayCommand<WorkTeam> SelectWorkTeamCommand => new RelayCommand<WorkTeam>(OnSelectProvider);

        private async void OnSelectProvider(WorkTeam provider)
        {
            foreach (var p in WorkTeams)
                p.BorderColor = "#202020";

            provider.BorderColor = "#FFD700";
            SelectedProvider = provider;

            if (SelectedDate != default)
                await LoadAvailableSlotsAsync(); // جلب الأوقات بعد اختيار الـ Provider
        }




        public ObservableCollection<Category> Categories { get; set; } = new();


        private async Task LoadServicesAsync()
        {
            try
            {
                IsServicesLoad = true;
                IsCatogory = true;

                var data = await _apiServices.GetServiesAsync();
                if (data == null || data.Count == 0)
                    return;

                Services.Clear();
                foreach (var item in data)
                    Services.Add(item);

                FilteredServices = new ObservableCollection<Servies>(Services);

                // استخراج التصنيفات بدون تكرار
                Categories.Clear();
                var uniqueCategories = Services
                    .Where(s => s.Category != null)
                    .GroupBy(s => s.Category.Name)
                    .Select(g => g.First().Category)
                    .ToList();

                foreach (var cat in uniqueCategories)
                    Categories.Add(cat);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading services: {ex}");
            }
            finally
            {
                IsServicesLoad = false;
                IsCatogory = false;
            }
        }

        [ObservableProperty]
        private Category selectedCategory;
        public void FilterServices(Category category)
        {
            SelectedCategory = category;

            if (category == null || string.IsNullOrWhiteSpace(category.Name) || category.Name == "الكل")
            {
                FilteredServices = new ObservableCollection<Servies>(Services);
            }
            else
            {
                FilteredServices = new ObservableCollection<Servies>(
                    Services.Where(s => s.Category?.Name == category.Name)
                );
            }
        }




        ///post For send Email use on forget password 


        [RelayCommand]
        public async Task PostEmailAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                await App.Current.MainPage.DisplayAlert("خطأ", "يرجى إدخال البريد الإلكتروني", "موافق");
                return;
            }

            using var client = new HttpClient();
            var json = JsonSerializer.Serialize(new { email = Email });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
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
        //public string ConfirmPassword { get; set; }

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

        [ObservableProperty] private TimeSpan selectedTime;



        public Booking CurrentBooking { get; set; } = new Booking { SelectedServices = new List<Servies>() };


        public ICommand PostBookingCommand { get; }

        public async Task PostBookingAsync()
        {
            var booking = CurrentBooking;

            if (booking == null || booking.SelectedServices == null || booking.SelectedServices.Count == 0)
            {
                await Toast.Make(Langue.AppResource.pleaseselectoneservice).Show();
                return;
            }

            using var client = new HttpClient();
            var json = JsonSerializer.Serialize(booking);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("https://example.com/api/bookings", content);
                response.EnsureSuccessStatusCode();
                await Toast.Make(Langue.AppResource.bookingsentsuccessfully).Show();
            }
            catch (Exception ex)
            {
                await Toast.Make(ex.Message).Show();
            }
        }

        ///Section Post User
        [ObservableProperty] private string userName;
        [ObservableProperty] private string password;
        [ObservableProperty] private string confirmPassword;
        [ObservableProperty] private string? imageUser;
        [ObservableProperty] private string email;
        [ObservableProperty] private string fullName;
        public ICommand UpDateUserCommand { get; }
        //private async Task LoadUser()
        //{
        //    IsLoadUser = true;
        //    var user = await _apiServices.GetUserAsync();

        //    if (user != null)
        //    {
        //        UserName = user.UserName;
        //        Email = user.Email;
        //        FullName = user.FullName;
        //        ImageUser = user.ProfileImageUrl ?? "default_avatar.png";
        //    }
        //    IsLoadUser = false;
        //}
        private User currentUser;

        private async Task LoadUser()
        {
            IsLoadUser = true;
            currentUser = await _apiServices.GetUserAsync(); // تخزين المستخدم الحالي
            if (currentUser != null)
            {
                UserName = currentUser.UserName;
                Email = currentUser.Email;
                FullName = currentUser.FullName;
                ImageUser = currentUser.ProfileImageUrl ?? "default_avatar.png";
            }
            IsLoadUser = false;
        }


        public ICommand LoadCurrentUserCommand { get; }

        public string Name { get; set; }



        public ICommand UpdateUserCommand { get; }




        private async Task UpdateUserInfo()
        {
            try
            {
                bool updated = await _apiServices.UpdateUserAsync(UserName, Email, ImageUser);

                if (updated)
                {

                    var popup = new ConfermChange();
                    await Application.Current.MainPage.ShowPopupAsync(popup);
                }
                else
                {

                    var popup = new NoConfermChange();
                    await Application.Current.MainPage.ShowPopupAsync(popup);
                }
            }
            catch
            {

                var popup = new NoConfermChange();
                await Application.Current.MainPage.ShowPopupAsync(popup);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;


        public ICommand ChangePasswordUserCommand { get; }
        private async Task ChangeUserPasswordAsync()
        {


            await SetAuthorizationHeaderAsync();

            var user = new User
            {
                Password = password,
                ConfirmPassword = confirmPassword
            };
            var response = await _httpClient.PutAsJsonAsync("https://test.center-yazan.com/api/auth/reset-password", user);
            if (response.IsSuccessStatusCode)
            {
                Toast.Make(AppResource.PasswordUpdated, ToastDuration.Short).Show();

            }
            else
            {
                Toast.Make(AppResource.FeildUpdatePassord, ToastDuration.Short).Show();
            }
        }



        public ICommand DeleteAccountCommand { get; }
        private async Task DeleteAccountAsync()
        {
            using var client = new HttpClient { BaseAddress = new Uri("https/eee/RemoveUser") };
            if (!string.IsNullOrWhiteSpace(_token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            try
            {
                var response = await client.DeleteAsync("users/me"); // غيّر المسار حسب الباك ايند
                if (!response.IsSuccessStatusCode)
                {
                    var popup = new ErorRemoveMyAccount();

                    await App.Current.MainPage.ShowPopupAsync(popup);
                    return;
                }


                // تنظيف البيانات المحلية
                SecureStorage.RemoveAll();
                Preferences.Clear();

                // الانتقال لشاشة تسجيل الدخول
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception)
            {
                var popup = new ErorRemoveMyAccount();

                await App.Current.MainPage.ShowPopupAsync(popup);
            }
        }



        [ObservableProperty]
        private ObservableCollection<DayItem> providerDays = new();

        [ObservableProperty]
        private DateTime selectedDate;

        public ICommand SelectDayCommand { get; }


        private void LoadCurrentWeekDays()
        {
            ProviderDays.Clear();

            var today = DateTime.Today;
            int diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;

            var startOfWeek = today.AddDays(-diff);

            for (int i = 0; i < 7; i++)
            {
                var date = startOfWeek.AddDays(i);

                ProviderDays.Add(new DayItem
                {
                    Day = date.ToString("ddd"),
                    Date = date.ToString("dd"),
                    FullDate = date,
                    BorderColor = "#444444",
                    IsAvailable = true
                });
            }
        }

        private async void OnSelectDay(DayItem day)
        {
            foreach (var d in ProviderDays)
                d.BorderColor = "#444444";

            day.BorderColor = "#FFD700";
            SelectedDate = day.FullDate;

            if (SelectedProvider != null)
                await LoadAvailableSlotsAsync(); // جلب الأوقات بعد اختيار اليوم
        }

        [ObservableProperty]
        private ObservableCollection<SlotItem> availableSlots = new();
        public async Task LoadAvailableSlotsAsync()
        {
            if (SelectedProvider == null || SelectedDate == default)
                return;

            try
            {
                Isloadday = true;
                using var client = new HttpClient();
                // إرسال البيانات للباك إند
                var json = JsonSerializer.Serialize(new
                {
                    providerId = SelectedProvider.Id,
                    date = SelectedDate.ToString("yyyy-MM-dd")
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://example.com/api/get-available-slots", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var slots = JsonSerializer.Deserialize<List<SlotItem>>(responseBody);

                    AvailableSlots.Clear();
                    foreach (var slot in slots)
                        AvailableSlots.Add(slot);
                }
                else
                {
                    await Toast.Make("فشل في جلب الأوقات").Show();
                }
            }
            catch (Exception ex)
            {
                await Toast.Make(ex.Message).Show();
            }
            finally
            {
                Isloadday = false;
            }
        }




        public void AddSelectedService(Servies service)
        {
            if (service == null) return;

            var exists = SelectedServices.Any(s => s.Id == service.Id);
            if (!exists)
            {
                SelectedServices.Add(service);
                CurrentBooking.SelectedServices.Add(service);
                Console.WriteLine($"✅ Service added: {service.NameServies}");
            }
        }


        public void RemoveSelectedService(Servies service)
        {
            if (service == null) return;

            var serviceToRemove = SelectedServices.FirstOrDefault(s => s.Id == service.Id);
            if (serviceToRemove != null)
            {
                SelectedServices.Remove(serviceToRemove);
                CurrentBooking.SelectedServices.Remove(serviceToRemove);
                Console.WriteLine($"❌ Service removed: {service.NameServies}");
            }
        }


        [RelayCommand]
        public void ClearSelectedServices()
        {
            SelectedServices.Clear();
            CurrentBooking.SelectedServices.Clear();
            Console.WriteLine("🗑️  All selected services cleared");
        }

        public int GetSelectedServicesCount() => SelectedServices.Count;


        public bool HasSelectedServices() => SelectedServices.Count > 0;


        /// الحصول على إجمالي السعر  لسا ما استملتها 

        public decimal GetTotalPrice()
        {
            return SelectedServices.Sum(s =>
            {
                if (decimal.TryParse(s.PriceServies, out var price))
                    return price;
                return 0m;
            });
        }

        /// مي مستعملة بس احتياطا 
        public int GetTotalDuration()
        {
            return SelectedServices.Sum(s => s.TimeServies);
        }
        ///section PolicyandPrivacy   سياسة الخصوصية يبا 
        public async Task LoadPolicyandPrivacyAsync()
        {
            try
            {
                var data = await _apiServices.GetPolicyandPrivaciesAsync();

                ListpolicyandPrivacy.Clear();
                foreach (var item in data)
                    ListpolicyandPrivacy.Add(item);

              

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Policy and Privacy: {ex.Message}");
            }
        }
    }
}


