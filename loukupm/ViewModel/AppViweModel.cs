using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using loukupm.Model;
using loukupm.services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

        [ObservableProperty] private ObservableCollection<Servies> services = new();
        [ObservableProperty] private ObservableCollection<Servies> filteredServices = new(); 
    
        [ObservableProperty] private ObservableCollection<Booking> bookings;
        [ObservableProperty] private ObservableCollection<WorkTeam> filteredWorkTeams = new();
        [ObservableProperty] private ObservableCollection<WorkTeam> workTeams = new();
        [ObservableProperty] private ObservableCollection<Notifiction> notifications;
        private static readonly Lazy<AppViewModel> _instance = new(() => new AppViewModel());
        public static AppViewModel Instance => _instance.Value;

        private readonly ApiServices _apiServices = new ApiServices();
       
        private readonly string _token;
        public ICommand SelectServiceButtonCommand { get; }
        public AppViewModel()
        {
            LoadData();

            // Fire and forget
            _ = LoadBookingsAsync();
            _ = LoadNotificationsAsync();
            _ = LoadWorkTeamsAsync();
            _ = LoadServicesAsync();
            _token = SecureStorage.GetAsync("auth_token").Result;
            DeleteAccountCommand = new Command(async () => await DeleteAccountAsync());
            ConfirmCommand = new Command(async () => await SendCodeAsync());
            PostEmailCommand = new Command<string>(async (email) => await PostEmailAsync(email));
            ChangePasswordCommand = new Command(async () => await ChangePasswordAsync());
            PostBookingCommand = new Command(async () => await PostBookingAsync());
            UpDateUserCommand = new Command(async () => await PostUserAsync());
            ChangePasswordUserCommand = new Command(async () => await ChangeUserPasswordAsync());
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



        public ObservableCollection<Category> Categories { get; set; } = new();

        //private async Task LoadServicesAsync()
        //{
        //    try
        //    {
        //        IsServicesLoad = true;
        //        IsCatogory = true;
        //        Console.WriteLine("⏳ Starting service load...");

        //        var data = await _apiServices.GetServiesAsync();
        //        Console.WriteLine($"✅ Data fetched successfully: {data?.Count} items");

        //        if (data == null || data.Count == 0)
        //        {
        //            Console.WriteLine("⚠️ No services returned from API.");
        //            return;
        //        }

        //        Services.Clear();
        //        foreach (var item in data)
        //            Services.Add(item);

        //        FilteredServices.Clear();
        //        foreach (var item in Services)
        //            FilteredServices.Add(item);
        //        IsServicesLoad = false;
        //        IsCatogory = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"❌ Error loading services: {ex}");
        //    }
        //    finally
        //    {

        //        Console.WriteLine("🏁 Done loading services.");
        //    }
        //}
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






        // فلترة الخدمات حسب النوع
        //public void FilterServices(string type)
        //{
        //    if (string.IsNullOrWhiteSpace(type) || type == "الكل")
        //    {
        //        FilteredServices = new ObservableCollection<Servies>(Services);
        //    }
        //    else
        //    {
        //        FilteredServices = new ObservableCollection<Servies>(
        //            Services?.Where(s => s.Category != null && s.Category.Name == type)
        //                     ?? Enumerable.Empty<Servies>()
        //        );
        //    }
        //}



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
        [ObservableProperty] private string userName;
        [ObservableProperty] private string password;
        [ObservableProperty] private string confirmPassword;
        [ObservableProperty] private string imageUser;
        [ObservableProperty] private string email;
        public ICommand UpDateUserCommand { get; }
        private async Task PostUserAsync()
        {
            using var client = new HttpClient { BaseAddress = new Uri("https://eee/") };
            if (!string.IsNullOrWhiteSpace(_token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var user = new User
            {
                UserName = userName,
                Email = email,
                ImageUser =imageUser,
                Password = password
            };

            var response = await client.PutAsJsonAsync("api/users/update", user);
            if (response.IsSuccessStatusCode)
            {
                await Toast.Make("تم تحديث البيانات بنجاح", ToastDuration.Short).Show();
            }
            else
            {
                await Toast.Make("فشل تحديث البيانات", ToastDuration.Short).Show();
            }

        }


        /// End Section Upate User

        //// Edite Paswwoerd User section
        public ICommand ChangePasswordUserCommand { get;}  
        private async Task ChangeUserPasswordAsync()
        {

            using var client = new HttpClient { BaseAddress = new Uri("https://eee/") };
            if (!string.IsNullOrWhiteSpace(_token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var user = new User
            {
                Password = password,
                confirmPassword = confirmPassword
            };
            var response = await client.PutAsJsonAsync("api/users/change-password", user);
            if (response.IsSuccessStatusCode)
            {
                Toast.Make("تم تحديث كلمة المرور بنجاح", ToastDuration.Short).Show();
            }
            else
            {
                Toast.Make("فشل تحديث كلمة المرور", ToastDuration.Short).Show();
            }
        }


        ///// sexstion Remove User
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
                    await Application.Current.MainPage.DisplayAlert("خطأ", "فشل حذف الحساب.", "موافق");
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
                await Application.Current.MainPage.DisplayAlert("خطأ", "حدث خطأ أثناء العملية.", "موافق");
            }
        }

        ///End Section User 

    }







}


