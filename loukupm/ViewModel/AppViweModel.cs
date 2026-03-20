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
using static loukupm.Model.Auth;


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

        [ObservableProperty] private bool hasNoAppointments = true;

        [ObservableProperty] private ObservableCollection<WorkTeam> filteredWorkTeams = new();
        [ObservableProperty] private ObservableCollection<WorkTeam> workTeams = new();
        [ObservableProperty] private ObservableCollection<Notifiction> notifications;
        [ObservableProperty] private ObservableCollection<Booking> viewSrves;
        [ObservableProperty] private ObservableCollection<PolicyandPrivacyS> listpolicyandPrivacy = new();

        [ObservableProperty]
        private ObservableCollection<Servies> selectedServices = new();
        
        /// <summary>
        /// إجمالي سعر الخدمات المختارة
        /// </summary>
        [ObservableProperty]
        private decimal totalPrice = 0m;

        /// <summary>
        /// Search term for filtering services
        /// </summary>
        [ObservableProperty]
        private string searchServiceTerm = string.Empty;

        /// <summary>
        /// Search term for filtering work teams
        /// </summary>
        [ObservableProperty]
        private string searchTeamTerm = string.Empty;

        private static readonly Lazy<AppViewModel> _instance = new(() => new AppViewModel());
        public static AppViewModel Instance => _instance.Value;

        private readonly ApiServices _apiServices = new ApiServices();

        private string _token;
        public ICommand SelectServiceButtonCommand { get; }
        private readonly HttpClient _httpClient;
        public AppViewModel()
        {
            LoadData();
            _httpClient = new HttpClient();

            // Initialize UI-related commands synchronously
            DeleteAccountCommand = new Command(async () => await DeleteAccountAsync());
            ConfirmCommand = new Command(async () => await SendCodeAsync());
            ProviderDays = new ObservableCollection<DayItem>();
            SelectDayCommand = new Command<DayItem>(OnSelectDay);
            LoadCurrentWeekDays();
            ChangePasswordCommand = new Command(async () => await ChangePasswordAsync());
            PostBookingCommand = new AsyncRelayCommand(PostBookingAsync);
            UpdateUserCommand = new Command(async () => await UpdateUserInfo());
            ChangePasswordUserCommand = new Command(async () => await ChangeUserPasswordAsync());

            SelectServiceButtonCommand = new Command<Servies>(async service =>
            {
                if (service == null) return;

                Console.WriteLine($"🔍 Service clicked: {service.NameServies}, Price: '{service.PriceServies}'");

                // استخدام الـ Id للمقارنة بدلاً من المرجع
                var exists = SelectedServices.Any(s => s.Id == service.Id);

                if (!exists)
                {
                    SelectedServices.Add(service);  // ✨ إضافة للـ Collection
                    CurrentBooking.SelectedServices.Add(service);  // إضافة للـ List
                    Console.WriteLine($"✅ Service added: {service.NameServies}, Price: {service.PriceServies}");
                    
                    // ✨ Toast notification for adding service
                    await Toast.Make(AppResource.celectedserviesiddone, ToastDuration.Short).Show();
                }
                else
                {
                    // ✨ Service is already selected - show special notification
                    Console.WriteLine($"⚠️ Service already selected: {service.NameServies}");
                    await Toast.Make(AppResource.theserviewasdone, ToastDuration.Short).Show();
                }

                // تحديث إجمالي السعر
                UpdateTotalPrice();

                // طباعة القائمة للمراجعة
                Console.WriteLine("📋 Current Selected Services:");
                foreach (var s in SelectedServices)
                    Console.WriteLine($"   - {s.NameServies} (Price: '{s.PriceServies}')");
                Console.WriteLine($"💰 Total Price: {TotalPrice}");
            });

            // command to allow view to trigger reload
            LoadAppointmentsCommand = new AsyncRelayCommand(LoadBookingsAsync);

            // start async initialization (load user first so appointments can be loaded)
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadUser();
            _token = await SecureStorage.GetAsync("auth_token") ?? string.Empty;
            await LoadBookingsAsync();
            _ = LoadNotificationsAsync();
            _ = LoadWorkTeamsAsync();
            _ = LoadServicesAsync();
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

        /// <summary>
        /// Handle search term changes and filter services in real-time
        /// </summary>
        partial void OnSearchServiceTermChanged(string value)
        {
            PerformServiceSearch(value);
        }

        /// <summary>
        /// Handle work team search term changes and filter teams in real-time
        /// </summary>
        partial void OnSearchTeamTermChanged(string value)
        {
            PerformWorkTeamSearch(value);
        }

        /// <summary>
        /// Perform local search on services - filters by name, category, price
        /// </summary>
        private void PerformServiceSearch(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    // If search is empty and a category is selected, show category filtered services
                    if (SelectedCategory != null)
                    {
                        FilterServices(SelectedCategory);
                    }
                    else
                    {
                        // Show all services
                        FilteredServices = new ObservableCollection<Servies>(Services);
                    }
                    Console.WriteLine("🔍 Search cleared - showing all services");
                }
                else
                {
                    // Get the appropriate source (either category filtered or all services)
                    var sourceList = SelectedCategory != null && SelectedCategory.Name != "الكل"
                        ? Services.Where(s => s.Category?.Name == SelectedCategory.Name).ToList()
                        : Services.ToList();

                    // Search within the source list
                    var searchResults = SearchService.SearchServices(sourceList, searchTerm);

                    // Sort by relevance
                    var sortedResults = SearchService.SortByRelevance(searchResults, searchTerm);

                    FilteredServices = new ObservableCollection<Servies>(sortedResults);

                    Console.WriteLine($"🔍 Search performed: '{searchTerm}' - Found {FilteredServices.Count} results");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Search error: {ex.Message}");
                FilteredServices = new ObservableCollection<Servies>(Services);
            }
        }

        /// <summary>
        /// Perform local search on work teams - filters by name
        /// </summary>
        private void PerformWorkTeamSearch(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    FilteredWorkTeams = new ObservableCollection<WorkTeam>(WorkTeams);
                    Console.WriteLine("🔍 Work team search cleared - showing all teams");
                }
                else
                {
                    var searchResults = SearchService.SearchWorkTeams(WorkTeams.ToList(), searchTerm);
                    FilteredWorkTeams = new ObservableCollection<WorkTeam>(searchResults);
                    Console.WriteLine($"🔍 Work team search: '{searchTerm}' - Found {FilteredWorkTeams.Count} results");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Work team search error: {ex.Message}");
                FilteredWorkTeams = new ObservableCollection<WorkTeam>(WorkTeams);
            }
        }

        /// <summary>
        /// Clear all search filters
        /// </summary>
        [RelayCommand]
        public void ClearAllSearches()
        {
            SearchServiceTerm = string.Empty;
            SearchTeamTerm = string.Empty;
            FilteredServices = new ObservableCollection<Servies>(Services);
            FilteredWorkTeams = new ObservableCollection<WorkTeam>(WorkTeams);
            Console.WriteLine("🗑️ All searches cleared");
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
        public IAsyncRelayCommand LoadAppointmentsCommand { get; private set; }
        private async Task LoadBookingsAsync()
        {
            if (currentUser == null)
                return;

            IsloadBooking = true;

            try
            {
                var data = await _apiServices.GetUserAppointmentsAsync(currentUser);

                if (data == null || data.Count == 0)
                {
                    Appointments.Clear();
                    HasNoAppointments = true;
                    return;
                }

                Appointments.Clear();

                foreach (var item in data)
                    Appointments.Add(item);

                HasNoAppointments = false;
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

                IsWorkTeamLoad = false;

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

            Console.WriteLine($"✅ Provider selected: {provider.Name} (ID: {provider.Id})");

            // ✅ احفظ الخدمات المختارة سابقاً
            var previousSelectedServices = new List<Servies>(SelectedServices);

            // ✅ جلب خدمات هذا البروفايدر فقط
            await LoadProviderServicesAsync(provider.Id);

            // ✅ تحقق من أي الخدمات السابقة يقدمها البروفايدر الجديد
            if (previousSelectedServices.Count > 0 && FilteredServices.Count > 0)
            {
                SelectedServices.Clear();
                CurrentBooking.SelectedServices.Clear();

                foreach (var service in previousSelectedServices)
                {
                    // تحقق إذا كان البروفايدر الجديد يقدم هذه الخدمة
                    if (FilteredServices.Any(s => s.Id == service.Id))
                    {
                        SelectedServices.Add(service);
                        CurrentBooking.SelectedServices.Add(service);
                        Console.WriteLine($"✅ Service retained: {service.NameServies}");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ Service not offered by new provider: {service.NameServies}");
                    }
                }
            }
            else
            {
                // إذا ما فيه خدمات محددة سابقاً، امسح الخدمات الحالية
                SelectedServices.Clear();
                CurrentBooking.SelectedServices.Clear();
            }

            // ✅ جلب الأوقات إذا تم اختيار تاريخ
            if (SelectedDate != default)
                await LoadAvailableSlotsAsync();
        }

        /// <summary>
        /// جلب الخدمات الخاصة بالبروفايدر المختار وتحديث الـ FilteredServices
        /// ملاحظة: إذا لم يكن هناك endpoint منفصل للبروفايدر، سيتم عرض جميع الخدمات المتاحة
        /// </summary>
        private async Task LoadProviderServicesAsync(int providerId)
        {
            try
            {
                Console.WriteLine($"📡 Loading services for provider {providerId}...");
                
                // جلب الخدمات (سيرجع خدمات البروفايدر إن وجدت، أو جميع الخدمات كبديل)
                var providerServices = await _apiServices.GetProviderServicesAsync(providerId);

                if (providerServices != null && providerServices.Count > 0)
                {
                    // عرض الخدمات
                    FilteredServices = new ObservableCollection<Servies>(providerServices);
                    Console.WriteLine($"✅ Showing {FilteredServices.Count} services available for this provider");
                }
                else
                {
                    Console.WriteLine($"⚠️ No services found - showing all available services");
                    FilteredServices = new ObservableCollection<Servies>(Services);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading provider services: {ex.Message}");
                // في حالة الخطأ: عرض جميع الخدمات
                FilteredServices = new ObservableCollection<Servies>(Services);
            }
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

            // If there's an active search, reapply it to the newly filtered category
            if (!string.IsNullOrWhiteSpace(SearchServiceTerm))
            {
                PerformServiceSearch(SearchServiceTerm);
            }
        }
        // FOR REMOVE

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





        //FOR REMOVE

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


        //TO REMOVE
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

        [ObservableProperty]
        private SlotItem selectedSlot;



        public Booking CurrentBooking { get; set; } = new Booking { SelectedServices = new List<Servies>() };


        public ICommand PostBookingCommand { get; }

        public async Task PostBookingAsync()
        {
            // ✅ التحقق من البيانات المختارة
            if (SelectedServices == null || SelectedServices.Count == 0)
            {
                await Toast.Make("يرجى اختيار خدمة واحدة على الأقل").Show();
                return;
            }

            if (SelectedProvider == null)
            {
                await Toast.Make("يرجى اختيار مزود الخدمة").Show();
                return;
            }

            if (SelectedDate == default)
            {
                await Toast.Make("يرجى اختيار تاريخ").Show();
                return;
            }

            if (SelectedSlot == null)
            {
                await Toast.Make("يرجى اختيار وقت").Show();
                return;
            }

            try
            {
                // ✅ حساب إجمالي السعر
                decimal totalAmount = SelectedServices.Sum(s =>
                {
                    if (string.IsNullOrWhiteSpace(s?.PriceServies))
                        return 0m;
                    
                    string priceStr = s.PriceServies.Trim();
                    if (priceStr.Contains(','))
                        priceStr = priceStr.Replace(',', '.');
                    
                    if (decimal.TryParse(priceStr, System.Globalization.NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture, out var price))
                        return price;
                    
                    return 0m;
                });

                // ✅ تجميع البيانات بالصيغة الصحيحة للـ API
                // تنسيق بيانات الخدمات حسب صيغة الـ API المتوقعة
                var bookingData = new
                {
                    services = SelectedServices.Select(s => new
                    {
                        service_id = s.Id,
                        provider_id = SelectedProvider.Id,
                        start_time = SelectedSlot.StartTime
                    }).ToList(),
                    // معلومات إضافية للمرجع
                    date = SelectedDate.ToString("yyyy-MM-dd"),
                    end_time = SelectedSlot.EndTime,
                    duration_minutes = SelectedSlot.DurationMinutes,
                    branch_id = 1,
                    payment_method = "cash",
                    total_amount = totalAmount,
                    status = "pending"
                };

                Console.WriteLine("📤 Sending booking data:");
                Console.WriteLine($"   Provider: {SelectedProvider.Name} (ID: {SelectedProvider.Id})");
                Console.WriteLine($"   Date: {SelectedDate:yyyy-MM-dd}");
                Console.WriteLine($"   Start Time: {SelectedSlot.StartTime}");
                Console.WriteLine($"   End Time: {SelectedSlot.EndTime}");
                Console.WriteLine($"   Services: {string.Join(", ", SelectedServices.Select(s => $"{s.NameServies} (ID: {s.Id})"))}");
                Console.WriteLine($"   Total Amount: {totalAmount:F2}");
                Console.WriteLine($"   Payment Method: cash");

                // ✅ إرسال البيانات للـ API
                await SetAuthorizationHeaderAsync();
                
                var json = JsonSerializer.Serialize(bookingData);
                Console.WriteLine($"📋 JSON Payload: {json}");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    "https://test.center-yazan.com/api/bookings", 
                    content);

                Console.WriteLine($"📊 Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"✅ Response: {responseBody}");

                    // ✅ نجح الحجز
                    await Toast.Make("تم الحجز بنجاح! ✅").Show();

                    // ✅ تنظيف البيانات بعد النجاح
                    ClearBookingData();

                    // ✅ الانتقال للصفحة التالية (الدفع)
                    await Shell.Current.GoToAsync(nameof(Paymentgetway));
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Error Response ({response.StatusCode}): {errorBody}");

                    // معالجة الأخطاء
                    try
                    {
                        var errorOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var errorResponse = JsonSerializer.Deserialize<dynamic>(errorBody, errorOptions);
                        Console.WriteLine($"❌ Error Details: {errorBody}");
                        await Toast.Make($"فشل الحجز: {response.StatusCode}").Show();
                    }
                    catch (Exception parseEx)
                    {
                        Console.WriteLine($"❌ Error parsing response: {parseEx.Message}");
                        await Toast.Make($"فشل الحجز: خطأ {response.StatusCode}").Show();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
                await Toast.Make($"خطأ: {ex.Message}").Show();
            }
        }

        /// <summary>
        /// تنظيف بيانات الحجز بعد الإرسال الناجح
        /// </summary>
        private void ClearBookingData()
        {
            // امسح الخدمات المختارة
            SelectedServices.Clear();
            CurrentBooking.SelectedServices.Clear();

            // امسح البروفايدر المختار
            foreach (var p in WorkTeams)
                p.BorderColor = "#202020";
            SelectedProvider = null;

            // امسح التاريخ واليوم المختار
            SelectedDate = default;
            foreach (var d in ProviderDays)
                d.BorderColor = "#444444";

            // امسح الوقت المختار
            foreach (var slot in AvailableSlots)
                slot.IsSelected = false;
            SelectedSlot = null;

            // امسح قائمة الأوقات
            AvailableSlots.Clear();

            // امسح إجمالي السعر
            UpdateTotalPrice();

            Console.WriteLine("🗑️ Booking data cleared");
        }

        ///Section Post User
        [ObservableProperty] private string userName;
        [ObservableProperty] private string password;
        [ObservableProperty] private string confirmPassword;
        [ObservableProperty] private string? imageUser;
        [ObservableProperty] private string email;
        [ObservableProperty] private string fullName;
        public ICommand UpDateUserCommand { get; }
        
        /// <summary>
        /// Avatar property - alias لـ ImageUser للربط مع XAML
        /// </summary>
        public string Avatar 
        { 
            get => ImageUser; 
            set => ImageUser = value; 
        }
        
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



        //رتجع هون
        private async Task UpdateUserInfo()
        {
            try
            {
                bool updated = await _apiServices.UpdateUserAsync(UserName,ImageUser);

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
        private string currentPassword = string.Empty;
        
        public string CurrentPassword
        {
            get => currentPassword;
            set => currentPassword = value;
        }
        
        private async Task ChangeUserPasswordAsync()
        {
            // ✅ التحقق من البيانات
            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                await Toast.Make("يرجى إدخال كلمة المرور الحالية").Show();
                return;
            }

            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                await Toast.Make("يرجى إدخال كلمة المرور الجديدة في كلا الحقلين").Show();
                return;
            }

            if (password != confirmPassword)
            {
                await Toast.Make("كلمات المرور الجديدة غير متطابقة").Show();
                return;
            }

            if (password.Length < 8)
            {
                await Toast.Make("كلمة المرور الجديدة يجب أن تكون 8 أحرف على الأقل").Show();
                return;
            }

            if (password == CurrentPassword)
            {
                await Toast.Make("كلمة المرور الجديدة يجب أن تكون مختلفة عن الحالية").Show();
                return;
            }

            try
            {
                await SetAuthorizationHeaderAsync();

                // ✅ بناء البيانات بالصيغة الصحيحة للـ API
                // الصيغة الأولى: snake_case (الأكثر شيوعاً في APIs)
                var passwordChangeData = new
                {
                    current_password = CurrentPassword,
                    password = password,
                    password_confirmation = confirmPassword
                };

                Console.WriteLine("📤 Sending password change request:");
                Console.WriteLine($"   Current Password: ***");
                Console.WriteLine($"   New Password: ***");
                Console.WriteLine($"   Confirmation: ***");
                Console.WriteLine($"   Password Length: {password.Length}");

                var json = JsonSerializer.Serialize(passwordChangeData);
                Console.WriteLine($"📋 JSON Payload: {json}");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    "https://test.center-yazan.com/api/profile/change-password",
                    content);

                Console.WriteLine($"📊 Response Status: {response.StatusCode}");

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📄 Response Body: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ Password changed successfully");

                    // ✅ تنظيف الحقول بعد النجاح
                    CurrentPassword = string.Empty;
                    Password = string.Empty;
                    ConfirmPassword = string.Empty;

                    await Toast.Make(AppResource.PasswordUpdated, ToastDuration.Short).Show();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                {
                    // معالجة خطأ 422 Unprocessable Entity
                    Console.WriteLine($"❌ Validation Error (422): {responseBody}");
                    
                    try
                    {
                        var errorOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var errorResponse = JsonSerializer.Deserialize<Auth.ErrorResponse>(responseBody, errorOptions);
                        
                        if (errorResponse?.Errors != null)
                        {
                            var errorMessages = string.Join("\n", 
                                errorResponse.Errors.Values.SelectMany(e => e));
                            await Toast.Make($"خطأ في البيانات:\n{errorMessages}").Show();
                        }
                        else if (!string.IsNullOrEmpty(errorResponse?.Message))
                        {
                            await Toast.Make($"خطأ: {errorResponse.Message}").Show();
                        }
                        else
                        {
                            await Toast.Make("فشل التحقق من البيانات. تحقق من صحة كلمات المرور").Show();
                        }
                    }
                    catch
                    {
                        await Toast.Make("فشل تغيير كلمة المرور. تحقق من صحة البيانات المدخلة").Show();
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // 401: كلمة المرور الحالية خاطئة
                    Console.WriteLine($"❌ Unauthorized (401): Wrong current password");
                    await Toast.Make("كلمة المرور الحالية غير صحيحة").Show();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // 400: Bad Request
                    Console.WriteLine($"❌ Bad Request (400): {responseBody}");
                    await Toast.Make("طلب غير صحيح. تحقق من البيانات المدخلة").Show();
                }
                else
                {
                    // أخطاء أخرى
                    Console.WriteLine($"❌ Error Response ({response.StatusCode}): {responseBody}");
                    await Toast.Make($"فشل تغيير كلمة المرور: خطأ {(int)response.StatusCode}").Show();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
                await Toast.Make($"خطأ في الاتصال: {ex.Message}").Show();
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

        /// <summary>
        /// Command لاختيار وقت معين
        /// </summary>
        public IRelayCommand<SlotItem> SelectSlotCommand => new RelayCommand<SlotItem>(OnSelectSlot);


        private void LoadCurrentWeekDays()
        {
            ProviderDays.Clear();

            var today = DateTime.Today;

            // ✅ عرض اليوم الحالي و 6 أيام قادمة (بدون أيام ماضية)
            for (int i = 0; i < 7; i++)
            {
                var date = today.AddDays(i);

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

        /// <summary>
        /// معالج اختيار الوقت المتاح
        /// </summary>
        private void OnSelectSlot(SlotItem slot)
        {
            if (slot == null) return;

            // ✅ امسح الاختيار السابق
            foreach (var s in AvailableSlots)
                s.IsSelected = false;

            // ✅ اختر الوقت الجديد
            slot.IsSelected = true;
            SelectedSlot = slot;

            // ✅ خزّن الوقت والتاريخ والبروفايدر
            SelectedTime = TimeSpan.Parse(slot.StartTime);
            CurrentBooking.Time = SelectedTime;
            CurrentBooking.Date = SelectedDate;
            
            if (SelectedProvider != null)
                CurrentBooking.ProviderId = SelectedProvider.Id.ToString();

            Console.WriteLine($"✅ Slot selected: {slot.DisplayTime}");
            Console.WriteLine($"   Provider: {SelectedProvider?.Name}");
            Console.WriteLine($"   Date: {SelectedDate:yyyy-MM-dd}");
            Console.WriteLine($"   Time: {slot.StartTime}");
        }

        [ObservableProperty]
        private ObservableCollection<SlotItem> availableSlots = new();

        [ObservableProperty]
        private AvailabilityData currentAvailability;

        /// <summary>
        /// property لتتبع إذا كانت قائمة الأوقات فارغة (لإظهار/إخفاء رسالة "لا توجد أوقات")
        /// </summary>
        public bool HasNoAvailableSlots => AvailableSlots?.Count == 0;

        /// <summary>
        /// property لتتبع إذا حدث خطأ في جلب الأوقات (Bad Request أو خطأ آخر)
        /// </summary>
        [ObservableProperty]
        private bool hasAvailabilityError = false;

        public async Task LoadAvailableSlotsAsync()
        {
            if (SelectedProvider == null || SelectedDate == default)
            {
                Console.WriteLine("❌ Missing required data:");
                Console.WriteLine($"   SelectedProvider: {SelectedProvider?.Name ?? "NULL"}");
                Console.WriteLine($"   SelectedDate: {SelectedDate:yyyy-MM-dd}");
                return;
            }

            try
            {
                Isloadday = true;
                HasAvailabilityError = false; // ✅ امسح الخطأ السابق
                await SetAuthorizationHeaderAsync();

                // إذا لم تكن هناك خدمة مختارة، استخدم أول خدمة من البروفايدر
                var selectedService = SelectedServices.FirstOrDefault();
                
                if (selectedService == null)
                {
                    // لو ما فيه خدمة مختارة، استخدم أول خدمة من الخدمات المعروضة
                    selectedService = FilteredServices.FirstOrDefault();
                    
                    if (selectedService == null)
                    {
                        Console.WriteLine("⚠️ No services available for this provider");
                        await Toast.Make("يرجى اختيار خدمة").Show();
                        return;
                    }
                    
                    Console.WriteLine($"⚠️ No service explicitly selected, using first available: {selectedService.NameServies}");
                }

                // التحقق من القيم
                int providerId = SelectedProvider.Id;
                int serviceId = selectedService.Id;
                string dateStr = SelectedDate.ToString("yyyy-MM-dd");

                Console.WriteLine($"📡 Loading availability for:");
                Console.WriteLine($"   Provider ID: {providerId}");
                Console.WriteLine($"   Service ID: {serviceId}");
                Console.WriteLine($"   Date: {dateStr}");

                // بناء رابط الطلب مع provider_id و service_id والتاريخ و branch_id
                string url = $"https://test.center-yazan.com/api/availability/provider" +
                    $"?provider_id={providerId}" +
                    $"&service_id={serviceId}" +
                    $"&date={dateStr}" +
                    $"&branch_id=1";

                Console.WriteLine($"🔗 URL: {url}");

                var response = await _httpClient.GetAsync(url);

                Console.WriteLine($"📊 Response Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"✅ Response Body: {responseBody}");

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var availabilityResponse = JsonSerializer.Deserialize<AvailabilityResponse>(responseBody, options);

                    if (availabilityResponse?.Success == true && availabilityResponse.Data?.AvailableSlots != null)
                    {
                        // حفظ معلومات التوفر الكاملة
                        CurrentAvailability = availabilityResponse.Data;

                        // ملء قائمة الـ slots
                        AvailableSlots.Clear();
                        foreach (var slot in availabilityResponse.Data.AvailableSlots)
                        {
                            AvailableSlots.Add(new SlotItem
                            {
                                StartTime = slot.StartTime,
                                EndTime = slot.EndTime,
                                DisplayTime = slot.DisplayTime,
                                DurationMinutes = slot.DurationMinutes
                            });
                        }

                        Console.WriteLine($"✅ Loaded {AvailableSlots.Count} available slots");
                        await Toast.Make($"تم جلب {AvailableSlots.Count} وقت متاح").Show();
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ No slots available or invalid response: {availabilityResponse?.Message}");
                        AvailableSlots.Clear(); // ✅ امسح الأوقات السابقة
                        HasAvailabilityError = true; // ✅ علّم أنه فيه خطأ
                        await Toast.Make("لا توجد أوقات متاحة").Show();
                    }
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ API Error {response.StatusCode}: {errorBody}");
                    
                    // ✅ في حالة الخطأ (Bad Request، etc): امسح الأوقات وعلّم الخطأ
                    AvailableSlots.Clear();
                    HasAvailabilityError = true;
                    
                    // محاولة استخراج رسالة خطأ من الـ JSON
                    try
                    {
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var errorResponse = JsonSerializer.Deserialize<dynamic>(errorBody, options);
                        await Toast.Make($"خطأ: {response.StatusCode}").Show();
                    }
                    catch
                    {
                        await Toast.Make($"فشل في جلب الأوقات: {response.StatusCode}").Show();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
                
                // ✅ في حالة الاستثناء: امسح الأوقات وعلّم الخطأ
                AvailableSlots.Clear();
                HasAvailabilityError = true;
                
                await Toast.Make($"خطأ: {ex.Message}").Show();
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
                UpdateTotalPrice();
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
                UpdateTotalPrice();
                Console.WriteLine($"❌ Service removed: {service.NameServies}");
            }
        }


        [RelayCommand]
        public void ClearSelectedServices()
        {
            SelectedServices.Clear();
            CurrentBooking.SelectedServices.Clear();
            UpdateTotalPrice();
            Console.WriteLine("🗑️  All selected services cleared");
        }

        public int GetSelectedServicesCount() => SelectedServices.Count;


        public bool HasSelectedServices() => SelectedServices.Count > 0;


        /// <summary>
        /// تحديث إجمالي السعر من الخدمات المختارة
        /// </summary>
        private void UpdateTotalPrice()
        {
            TotalPrice = SelectedServices.Sum(s =>
            {
                if (string.IsNullOrWhiteSpace(s?.PriceServies))
                {
                    Console.WriteLine($"⚠️ Price is null/empty for service: {s?.NameServies}");
                    return 0m;
                }

                // جرب صيغ مختلفة للأرقام (comma و dot as decimal separator)
                string priceStr = s.PriceServies.Trim();
                
                // استبدل الفاصلة بالنقطة إذا لزم الأمر (لتوحيد الصيغة)
                if (priceStr.Contains(','))
                    priceStr = priceStr.Replace(',', '.');

                if (decimal.TryParse(priceStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var price))
                {
                    Console.WriteLine($"✅ Parsed price for {s.NameServies}: {price}");
                    return price;
                }

                Console.WriteLine($"❌ Failed to parse price '{s.PriceServies}' for service: {s.NameServies}");
                return 0m;
            });
            Console.WriteLine($"💰 Total Price Updated: {TotalPrice:F2} (Total items: {SelectedServices.Count})");
        }

        /// <summary>
        /// الحصول على إجمالي السعر
        /// </summary>
        public decimal GetTotalPrice()
        {
            return TotalPrice;
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


