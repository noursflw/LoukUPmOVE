using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loukupm.Langue;
using loukupm.Langue;
using loukupm.Model;
using loukupm.services;
using loukupm.Services;
using loukupm.View;
using loukupm.View.MassgingApp;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using static loukupm.Model.Auth;


namespace loukupm.ViewModel
{
    public partial class AppViewModel : ObservableObject
    {

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
        [ObservableProperty] private bool isLoggingOut;
        [ObservableProperty] private bool isRefreshing = false;
        [ObservableProperty] private bool hasNoHomeData = false;


        [ObservableProperty] private ObservableCollection<Servies> services = new();
        [ObservableProperty] private ObservableCollection<Servies> filteredServices = new();

        [ObservableProperty] private ObservableCollection<Booking> bookings = new();
        [ObservableProperty] private ObservableCollection<Appointment> appointments = new();

        // Appointments divided by status for TabView
        [ObservableProperty] private ObservableCollection<Appointment> upcomingAppointments = new();
        [ObservableProperty] private ObservableCollection<Appointment> previousAppointments = new();
        [ObservableProperty] private ObservableCollection<Appointment> canceledAppointments = new();

        [ObservableProperty] private bool hasNoAppointments = true;

        [ObservableProperty] private ObservableCollection<WorkTeam> filteredWorkTeams = new();
        [ObservableProperty] private ObservableCollection<WorkTeam> workTeams = new();
        [ObservableProperty] private ObservableCollection<Notification> notifications = new();
        [ObservableProperty]
        private int unreadNotificationCount = 0;
        [ObservableProperty] private bool hasMoreNotifications = false;
        [ObservableProperty] private string nextNotificationCursor = null;
        [ObservableProperty] private ObservableCollection<Booking> viewSrves;
        [ObservableProperty] private ObservableCollection<PolicyandPrivacyS> listpolicyandPrivacy = new();

        [ObservableProperty]
        private ObservableCollection<Servies> selectedServices = new();


        [ObservableProperty]
        private decimal totalPrice = 0m;

        [ObservableProperty]
        private bool isBookingInProgress = false;

        [ObservableProperty]
        private string bookingErrorMessage = string.Empty;

        [ObservableProperty]
        private string lastBookingNumber = string.Empty;


        [ObservableProperty]
        private string searchServiceTerm = string.Empty;


        [ObservableProperty]
        private string searchTeamTerm = string.Empty;


        [ObservableProperty]
        private string reminderMinutes = "60";

        [ObservableProperty]
        private TimeSpan reminderTime = new TimeSpan(1, 0, 0);

        [ObservableProperty]
        private int selectedReminderOptionIndex = 0;

        [ObservableProperty]
        private bool isReminderEnabled = false;

        [ObservableProperty]
        private ObservableCollection<ReminderOption> reminderOptions = new ObservableCollection<ReminderOption>();


        [ObservableProperty]
        private string selectedImagePath = string.Empty;


        [ObservableProperty]
        private bool isCancelingBooking = false;

        [ObservableProperty]
        private AboutUsViewModel aboutUsVM;
        [ObservableProperty]
        private PhoneOtpViewModel phoneOtpVM;

        [ObservableProperty]
        private HomeSliderViewModel homeSliderVM;
        public IAsyncRelayCommand RefreshServicesCommand { get; }
        public IAsyncRelayCommand RefreshHomeCommand { get; private set; }

        private readonly ApiServices _apiServices = new ApiServices();
        private readonly services.NotificationStateService _notificationState;

        private static readonly Lazy<AppViewModel> _instance = new(() => new AppViewModel(GetNotificationStateFromServiceProvider(), GetNotificationServiceFromServiceProvider()));
        public static AppViewModel Instance => _instance.Value;

        // Parameterless ctor for views that still call new AppViewModel()
        // It attempts to resolve NotificationStateService and NotificationService from MAUI DI if available,
        // otherwise it falls back to creating local instances (not recommended for shared state).
        public AppViewModel() : this(GetNotificationStateFromServiceProvider(), GetNotificationServiceFromServiceProvider()) { }

        private static services.NotificationStateService GetNotificationStateFromServiceProvider()
        {
            try
            {
                var mauiContext = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext;
                var svc = mauiContext?.Services.GetService(typeof(services.NotificationStateService)) as services.NotificationStateService;
                if (svc != null) return svc;
            }
            catch { }
            return new services.NotificationStateService();
        }

        private static services.NotificationService GetNotificationServiceFromServiceProvider()
        {
            try
            {
                var mauiContext = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext;
                var svc = mauiContext?.Services.GetService(typeof(services.NotificationService)) as services.NotificationService;
                if (svc != null) return svc;
            }
            catch { }
            return new services.NotificationService();
        }
        partial void OnNotificationCountChanged(int value)
        {
            System.Diagnostics.Debug.WriteLine($"[AppViewModel] NotificationCount changed on VM {GetHashCode()}: {value}");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnPropertyChanged(nameof(NotificationCount));
            });

            Console.WriteLine($"🔥 COUNT CHANGED: {value}");
        }
        private string _token;
        public ICommand SelectServiceButtonCommand { get; }

        // ✅ STATIC HttpClient - shared across all requests (proper pattern)
        private static readonly HttpClient _httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public IAsyncRelayCommand EnableReminderTimerCommand { get; private set; }
        public IAsyncRelayCommand<int> CancelBookingCommand { get; private set; }

        public AppViewModel(services.NotificationStateService notificationState, services.NotificationService notificationService = null)
        {
            _notificationState = notificationState ?? GetNotificationStateFromServiceProvider() ?? throw new ArgumentNullException(nameof(notificationState));
            _notificationService = notificationService ?? GetNotificationServiceFromServiceProvider() ?? new services.NotificationService();

            System.Diagnostics.Debug.WriteLine($"[AppViewModel] Created instance (DI): {GetHashCode()}");

            // Subscribe to shared notification state so multiple VM instances reflect same unread count
            _notificationState.UnreadCountChanged += (cnt) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UnreadNotificationCount = cnt;
                    NotificationCount = cnt;
                });
            };

            // Initialize local viewmodel value from shared state
            UnreadNotificationCount = _notificationState.UnreadCount;
            NotificationCount = _notificationState.UnreadCount;

            LoadData();

            EnableReminderTimerCommand = new AsyncRelayCommand(EnableReminderTimerAsync);
            HomeSliderVM = new HomeSliderViewModel();
            phoneOtpVM = new PhoneOtpViewModel(_apiServices);

            // ✅ Do NOT create new HttpClient here - use static instance

            DeleteAccountCommand = new Command<string>(async (password) =>
     await DeleteAccountAsync(password));
            RefreshServicesCommand = new AsyncRelayCommand(LoadServicesAsync);
            ProviderDays = new ObservableCollection<DayItem>();
            SelectDayCommand = new Command<DayItem>(OnSelectDay);
            LoadCurrentWeekDays();
            ChangePasswordCommand = new Command(async () => await ChangePasswordAsync());
            PostBookingCommand = new AsyncRelayCommand(PostBookingAsync);
            UpdateUserCommand = new Command(async () => await UpdateUserInfo());
            ChangePasswordUserCommand = new Command(async () => await ChangeUserPasswordAsync());
            EnableReminderTimerCommand = new AsyncRelayCommand(EnableReminderTimerAsync);
            CancelBookingCommand = new AsyncRelayCommand<int>(CancelBookingAsync);

            SelectServiceButtonCommand = new Command<Servies>(async service =>
            {
                if (service == null) return;

                var exists = SelectedServices.Any(s => s.Id == service.Id);

                if (!exists)
                {
                    service.IsSelected = true;  // تحديث حالة الخدمة
                    SelectedServices.Add(service);
                    CurrentBooking.SelectedServices.Add(service);  // إضافة للـ List

                    await Toast.Make(AppResource.celectedserviesiddone, ToastDuration.Short).Show();
                }
                else
                {
                    // إلغاء الاختيار
                    service.IsSelected = false;
                    SelectedServices.Remove(service);
                    CurrentBooking.SelectedServices.Remove(service);
                    CurrentBooking.RemoveServiceSelections(service.Id);

                    await Toast.Make(AppResource.serviceremoved, ToastDuration.Short).Show();
                    Console.WriteLine($"✅ Service deselected: {service.NameServies}");
                }

                UpdateTotalPrice();

                foreach (var s in SelectedServices)
                    Console.WriteLine($"   - {s.NameServies} (Price: '{s.PriceServies}')");
            });


            LoadAppointmentsCommand = new AsyncRelayCommand(LoadBookingsAsync);
            RefreshHomeCommand = new AsyncRelayCommand(RefreshHomeDataAsync);
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 [InitializeAsync] Starting initial data load...");

                await LoadUser();

                _token = await SecureStorage.GetAsync(TokenManager.AccessTokenKey) ?? string.Empty;

                await LoadBookingsAsync();


                var homeSliderTask = Task.CompletedTask;

                if (HomeSliderVM.Items.Count == 0)
                {
                    homeSliderTask = HomeSliderVM.LoadSlidersAsync();
                }

                await Task.WhenAll(
                    LoadNotificationsAsync(),
                    LoadWorkTeamsAsync(),
                    LoadServicesAsync(),
                    homeSliderTask
                );

                // Evaluate data state after initial load
                bool hasUserData = currentUser != null && !string.IsNullOrWhiteSpace(currentUser.UserName);
                bool hasServices = Services != null && Services.Count > 0;
                bool hasWorkTeams = WorkTeams != null && WorkTeams.Count > 0;

                HasNoHomeData = !hasUserData || (!hasServices && !hasWorkTeams);

                System.Diagnostics.Debug.WriteLine($"✅ [InitializeAsync] Initialization complete. User={hasUserData}, Services={hasServices}, Teams={hasWorkTeams}, Sliders={HomeSliderVM.Items.Count}, NoData={HasNoHomeData}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [InitializeAsync] Error: {ex.Message}");
                HasNoHomeData = true;
            }
        }

        private async Task SetAuthorizationHeaderAsync()
        {

            string? token = await SecureStorage.GetAsync(TokenManager.AccessTokenKey);

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            }
            else
            {

                _httpClient.DefaultRequestHeaders.Authorization = null;
            }


        }


        partial void OnSearchServiceTermChanged(string value)
        {
            PerformServiceSearch(value);
        }


        partial void OnSearchTeamTermChanged(string value)
        {
            PerformWorkTeamSearch(value);
        }

        /// <summary>
        /// Handles PhoneVerified property changes to refresh computed PhoneVerificationStatus.
        /// </summary>
        partial void OnPhoneVerifiedChanged(bool value)
        {
            OnPropertyChanged(nameof(PhoneVerificationStatus));
        }


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

        [RelayCommand]
        public void ClearAllSearches()
        {
            SearchServiceTerm = string.Empty;
            SearchTeamTerm = string.Empty;
            FilteredServices = new ObservableCollection<Servies>(Services);
            FilteredWorkTeams = new ObservableCollection<WorkTeam>(WorkTeams);

        }


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

        private async Task RefreshHomeDataAsync()
        {
            try
            {
                IsRefreshing = true;
                HasNoHomeData = false;

                // تحديث البيانات الثلاث بشكل متزامن + Home Slider
                var sliderRefreshTask = RefreshHomeSliderAsync();

                await Task.WhenAll(
                    LoadUserAsync(),
                    LoadServicesAsync(),
                    LoadWorkTeamsAsync(),
                    sliderRefreshTask
                );

                // التحقق من وجود بيانات
                bool hasUserData = currentUser != null && !string.IsNullOrWhiteSpace(currentUser.UserName);
                bool hasServices = Services != null && Services.Count > 0;
                bool hasWorkTeams = WorkTeams != null && WorkTeams.Count > 0;

                // يعتبر عدم وجود بيانات إذا لم تكن بيانات المستخدم موجودة أو إذا كانت كلا من الخدمات والفرق فارغة
                // يعتبر عدم وجود بيانات إذا لم تكن أي من البيانات الثلاث موجودة
                // أو إذا لم تكن بيانات المستخدم موجودة (بيانات حرجة)
                HasNoHomeData = !hasUserData || (!hasServices && !hasWorkTeams);

                System.Diagnostics.Debug.WriteLine($"🔄 [RefreshHomeDataAsync] Data Status: User={hasUserData}, Services={hasServices}, Teams={hasWorkTeams}, Sliders={HomeSliderVM.Items.Count}, NoData={HasNoHomeData}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [RefreshHomeDataAsync] Error refreshing home data: {ex.Message}");
                HasNoHomeData = true;
            }
            finally
            {
                IsRefreshing = false;
                System.Diagnostics.Debug.WriteLine($"✅ [RefreshHomeDataAsync] Refresh complete. IsRefreshing={IsRefreshing}, HasNoHomeData={HasNoHomeData}");
            }
        }

        /// <summary>
        /// Refresh home slider data independently.
        /// Failures here don't break the main HomePage loading.
        /// </summary>
        private async Task RefreshHomeSliderAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 [RefreshHomeSliderAsync] Starting home slider refresh...");
                await HomeSliderVM.LoadSlidersAsync();
                System.Diagnostics.Debug.WriteLine($"✅ [RefreshHomeSliderAsync] Home slider refresh complete. Items={HomeSliderVM.Items.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ [RefreshHomeSliderAsync] Home slider refresh error (non-blocking): {ex.Message}");
                // Error handling is done inside HomeSliderViewModel
                // We don't rethrow to prevent breaking the entire HomePage refresh
            }
        }

        private async Task LoadUserAsync()
        {
            try
            {
                IsLoadUser = true;
                await LoadUser();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading user: {ex.Message}");
            }
            finally
            {
                IsLoadUser = false;
            }
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
                    UpcomingAppointments.Clear();
                    PreviousAppointments.Clear();
                    CanceledAppointments.Clear();
                    HasNoAppointments = true;
                    return;
                }

                Appointments.Clear();
                UpcomingAppointments.Clear();
                PreviousAppointments.Clear();
                CanceledAppointments.Clear();

                foreach (var item in data)
                {
                    Appointments.Add(item);


                    if (item.Status == "USER_CANCELLED" || item.IsCancelled)
                    {
                        CanceledAppointments.Add(item);
                    }
                    else if (item.Status == "COMPLETED" || item.IsCompleted || item.IsPast)
                    {
                        PreviousAppointments.Add(item);
                    }
                    else if (item.Status == "PENDING" || item.IsUpcoming)
                    {
                        UpcomingAppointments.Add(item);
                    }
                    else
                    {

                        UpcomingAppointments.Add(item);
                    }
                }

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
                IsLoadNotifiction = true;


                var (notificationList, unreadCount, hasMore) = await _apiServices.GetNotificationsAsync(cursor: null, perPage: 15);

                Notifications.Clear();

                if (notificationList != null && notificationList.Count > 0)
                {
                    foreach (var notification in notificationList)
                    {
                        Notifications.Add(notification);
                    }
                }

                UnreadNotificationCount = unreadCount;
                HasMoreNotifications = hasMore;
                NextNotificationCursor = null;


            }
            catch (Exception ex)
            {

                Notifications.Clear();
            }
            finally
            {
                IsLoadNotifiction = false;
            }
        }

        public async Task LoadMoreNotificationsAsync()
        {
            if (!HasMoreNotifications || string.IsNullOrEmpty(NextNotificationCursor))
            {

                return;
            }

            try
            {
                var (notificationList, _, hasMore) = await _apiServices.GetNotificationsAsync(cursor: NextNotificationCursor, perPage: 15);

                if (notificationList != null && notificationList.Count > 0)
                {
                    foreach (var notification in notificationList)
                    {
                        Notifications.Add(notification);
                    }
                }

                HasMoreNotifications = hasMore;

            }
            catch (Exception ex)
            {

            }
        }

        private async Task LoadWorkTeamsAsync()
        {
            try
            {
                IsWorkTeamLoad = true;

                var data = await _apiServices.GetWorkTeamsAsync();


                if (data == null || data.Count == 0)
                {
                    WorkTeams.Clear();
                    FilteredWorkTeams.Clear();
                    return;
                }


                WorkTeams.Clear();


                foreach (var member in data)
                    WorkTeams.Add(member);


                // 🔥 Clear and repopulate FilteredWorkTeams instead of reassigning (triggers UI updates)
                FilteredWorkTeams.Clear();
                foreach (var team in WorkTeams)
                    FilteredWorkTeams.Add(team);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading work teams: {ex.Message}");
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
                p.BorderColor = "Transparent";

            provider.BorderColor = "#FFD700";
            SelectedProvider = provider;




            var previousSelectedServices = new List<Servies>(SelectedServices);


            await LoadProviderServicesAsync(provider.Id);


            if (previousSelectedServices.Count > 0 && FilteredServices.Count > 0)
            {
                SelectedServices.Clear();
                CurrentBooking.SelectedServices.Clear();

                foreach (var service in previousSelectedServices)
                {

                    if (FilteredServices.Any(s => s.Id == service.Id))
                    {
                        SelectedServices.Add(service);
                        CurrentBooking.SelectedServices.Add(service);

                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {

                SelectedServices.Clear();
                CurrentBooking.SelectedServices.Clear();
            }


            if (SelectedDate != default)
                await LoadAvailableSlotsAsync();
        }

        private async Task LoadProviderServicesAsync(int providerId)
        {
            try
            {



                var providerServices = await _apiServices.GetProviderServicesAsync(providerId);

                if (providerServices != null && providerServices.Count > 0)
                {

                    FilteredServices = new ObservableCollection<Servies>(providerServices);

                }
                else
                {

                    FilteredServices = new ObservableCollection<Servies>(Services);
                }
            }
            catch (Exception ex)
            {

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

                Services.Clear();
                FilteredServices.Clear();
                Categories.Clear();

                if (data == null || data.Count == 0)
                {
                    SelectedCategory = null;
                    return;
                }

                foreach (var item in data)
                    Services.Add(item);

                // 🔥 مهم: لا new
                foreach (var item in Services)
                    FilteredServices.Add(item);

                var uniqueCategories = Services
                    .Where(s => s.Category != null)
                    .GroupBy(s => s.Category.Name)
                    .Select(g => g.First().Category);

                foreach (var cat in uniqueCategories)
                    Categories.Add(cat);

                SelectedCategory = null;
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

            IEnumerable<Servies> result = Services;

            if (category != null &&
                !string.IsNullOrWhiteSpace(category.Name) &&
                category.Name != "الكل")
            {
                result = result.Where(s => s.Category?.Name == category.Name);
            }

            FilteredServices.Clear();

            foreach (var item in result)
                FilteredServices.Add(item);

            if (!string.IsNullOrWhiteSpace(SearchServiceTerm))
            {
                PerformServiceSearch(SearchServiceTerm);
            }
        }

        [ObservableProperty]
        private Servies selectedService;


        public string NewPassword { get; set; }


        public ICommand ChangePasswordCommand { get; }

        private async Task ChangePasswordAsync()
        {
            if (string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                await Toast.Make(AppResource.Pleaseenterthepasswordinbothfields).Show();
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                await Toast.Make(AppResource.Passwordsdonotmatch).Show();
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
                    await Toast.Make(AppResource.Passwordupdatedsuccessfully).Show();
                }
                else
                {
                    await Toast.Make(AppResource.Failedtoupdatethepassword).Show();
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("خطأ", ex.Message, "موافق");
            }
        }

        [ObservableProperty] private WorkTeam selectedProvider;
        [ObservableProperty] private string selectedServiceName;

        [ObservableProperty] private TimeSpan selectedTime;

        [ObservableProperty]
        private SlotItem selectedSlot;

        [ObservableProperty]
        private string bookingNotes = string.Empty;


        public Booking CurrentBooking { get; set; } = new Booking { SelectedServices = new List<Servies>() };


        public ICommand PostBookingCommand { get; }


        public async Task PostBookingAsync()
        {
            try
            {
                EnsureCurrentBooking();
                SyncCurrentBookingServices();

                if (!await ValidateBookingInputs())
                    return;

                var (services, totalEndTime) = BuildServicesArray();
                if (services == null || services.Count == 0 || services.Count != SelectedServices.Count)
                {
                    await Toast.Make(AppResource.Errorbuildingtheservicelist).Show();
                    return;
                }

                // Set reminder on booking if enabled
                if (IsReminderEnabled && SelectedReminderOptionIndex < ReminderOptions.Count)
                {
                    var selectedOption = ReminderOptions[SelectedReminderOptionIndex];
                    CurrentBooking.ReminderOffsetHours = selectedOption.OffsetHours;
                    Console.WriteLine($"[PostBookingAsync] Reminder enabled: {selectedOption.OffsetHours} hours");
                }
                else
                {
                    CurrentBooking.ReminderOffsetHours = null;
                    Console.WriteLine("[PostBookingAsync] Reminder disabled");
                }

                string notes = string.IsNullOrWhiteSpace(BookingNotes) ? "ok" : BookingNotes.Trim();
                var bookingData = CurrentBooking.ToCreateBookingPayload(services, notes);
                LogBookingRequest(services, notes, bookingData);
                await SendBookingRequest(bookingData, totalEndTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PostBookingAsync] Exception: {ex.Message}");
                await Toast.Make(AppResource.Connectionerror).Show();
            }
        }

        private void EnsureCurrentBooking()
        {
            CurrentBooking ??= new Booking { SelectedServices = new List<Servies>() };
            CurrentBooking.EnsureMaps();
        }

        private void SyncCurrentBookingServices()
        {
            EnsureCurrentBooking();
            CurrentBooking.SelectedServices = SelectedServices?.ToList() ?? new List<Servies>();
        }

        private bool IsPerServiceBookingMode()
        {
            EnsureCurrentBooking();
            return CurrentBooking.HasPerServiceSelections;
        }

        private async Task<bool> ValidateBookingInputs()
        {
            EnsureCurrentBooking();

            if (SelectedServices == null || SelectedServices.Count == 0)
            {
                await Toast.Make(AppResource.Pleaseselectatleastoneservice).Show();
                return false;
            }

            bool perService = IsPerServiceBookingMode();
            bool allowGlobal = !perService;

            Console.WriteLine($"[ValidateBookingInputs] Per-service mode: {perService}, Allow global fallback: {allowGlobal}");
            Console.WriteLine($"[ValidateBookingInputs] Selected services: {SelectedServices.Count}");
            Console.WriteLine($"[ValidateBookingInputs] ServiceProviderMap count: {CurrentBooking.ServiceProviderMap?.Count ?? 0}");
            Console.WriteLine($"[ValidateBookingInputs] ServiceDateMap count: {CurrentBooking.ServiceDateMap?.Count ?? 0}");
            Console.WriteLine($"[ValidateBookingInputs] ServiceSlotMap count: {CurrentBooking.ServiceSlotMap?.Count ?? 0}");

            foreach (var svc in SelectedServices)
            {
                Console.WriteLine($"[ValidateBookingInputs] Validating service {svc.Id}: {svc.NameServies}");

                if (!CurrentBooking.TryGetProviderId(svc.Id, SelectedProvider?.Id, allowGlobal, out var providerId))
                {
                    Console.WriteLine($"[ValidateBookingInputs] ERROR: No provider for service {svc.Id}");
                    await Toast.Make(AppResource.Pleaseselectaserviceprovider).Show();
                    return false;
                }
                Console.WriteLine($"[ValidateBookingInputs] Service {svc.Id} provider: {providerId}");

                if (!CurrentBooking.TryGetServiceDate(svc.Id, SelectedDate == default ? null : SelectedDate, allowGlobal, out var date))
                {
                    Console.WriteLine($"[ValidateBookingInputs] ERROR: No date for service {svc.Id}");
                    await Toast.Make(AppResource.Pleaseselectadate).Show();
                    return false;
                }
                Console.WriteLine($"[ValidateBookingInputs] Service {svc.Id} date: {date:yyyy-MM-dd}");

                if (!CurrentBooking.TryGetSlot(svc.Id, SelectedSlot?.StartTime, allowGlobal, out var slot))
                {
                    Console.WriteLine($"[ValidateBookingInputs] ERROR: No slot for service {svc.Id}");
                    await Toast.Make(AppResource.Pleaseselectatime).Show();
                    return false;
                }
                Console.WriteLine($"[ValidateBookingInputs] Service {svc.Id} slot: {slot}");
            }

            var serviceIds = SelectedServices.Select(s => s.Id);
            Console.WriteLine($"[ValidateBookingInputs] Resolving root date from {serviceIds.Count()} services");

            if (!CurrentBooking.TryResolveRootDate(serviceIds, SelectedDate == default ? null : SelectedDate, allowGlobal, out var rootDate, out var datesConflict))
            {
                if (datesConflict)
                {
                    Console.WriteLine($"[ValidateBookingInputs] ERROR: Date conflict between services");
                    await Toast.Make(AppResource.Pleaseselectadate).Show();
                    BookingErrorMessage = "All services in one booking must share the same date.";
                    return false;
                }

                Console.WriteLine($"[ValidateBookingInputs] ERROR: Unable to resolve root date");
                await Toast.Make(AppResource.Pleaseselectadate).Show();
                return false;
            }

            Console.WriteLine($"[ValidateBookingInputs] Root date resolved: {rootDate:yyyy-MM-dd}");
            CurrentBooking.Date = rootDate;
            if (SelectedDate == default)
                SelectedDate = rootDate;

            if (!PhoneVerified)
            {
                Console.WriteLine($"[ValidateBookingInputs] Phone not verified, showing OTP popup");
                var popup = new CanyouAcpetPhonenumber();
                await App.Current.MainPage.ShowPopupAsync(popup);
                return false;
            }

            Console.WriteLine($"[ValidateBookingInputs] Validation passed");
            return true;
        }

        private (List<BookingServicePayloadItem> services, TimeSpan totalEndTime) BuildServicesArray()
        {
            EnsureCurrentBooking();
            var services = new List<BookingServicePayloadItem>();
            TimeSpan latestEndTime = TimeSpan.Zero;
            bool allowGlobal = !IsPerServiceBookingMode();

            Console.WriteLine($"[BuildServicesArray] Per-service mode: {CurrentBooking.HasPerServiceSelections}, Allow global fallback: {allowGlobal}");

            foreach (var service in SelectedServices)
            {
                Console.WriteLine($"[BuildServicesArray] Processing service {service.Id}: {service.NameServies}");

                if (!CurrentBooking.TryGetSlot(service.Id, SelectedSlot?.StartTime, allowGlobal, out var startTimeString))
                {
                    Console.WriteLine($"[BuildServicesArray] ERROR: Unable to get slot for service {service.Id}");
                    return (new List<BookingServicePayloadItem>(), TimeSpan.Zero);
                }
                Console.WriteLine($"[BuildServicesArray] Service {service.Id} slot: {startTimeString}");

                if (!CurrentBooking.TryGetProviderId(service.Id, SelectedProvider?.Id, allowGlobal, out var providerId))
                {
                    Console.WriteLine($"[BuildServicesArray] ERROR: Unable to get provider for service {service.Id}");
                    return (new List<BookingServicePayloadItem>(), TimeSpan.Zero);
                }
                Console.WriteLine($"[BuildServicesArray] Service {service.Id} provider: {providerId}");

                TimeSpan startTs = ParseTimeString(startTimeString);

                int serviceDuration = GetServiceDuration(service);
                if (serviceDuration <= 0)
                    serviceDuration = 30;

                TimeSpan endTime = startTs.Add(TimeSpan.FromMinutes(serviceDuration));
                if (endTime > latestEndTime)
                    latestEndTime = endTime;

                services.Add(new BookingServicePayloadItem
                {
                    service_id = service.Id,
                    provider_id = providerId,
                    start_time = startTimeString
                });
            }

            Console.WriteLine($"[BuildServicesArray] Built {services.Count} services successfully");
            return (services, latestEndTime);
        }


        private int GetServiceDuration(Servies service)
        {
            if (service == null)
                return 30;

            int duration = service.TimeServies;
            if (duration <= 0)
            {

                return 30;
            }

            if (duration > 480)
            {

                return 480;
            }

            return duration;
        }


        private decimal CalculateTotalPrice()
        {
            return SelectedServices.Sum(s =>
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
        }

        private void LogBookingRequest(List<BookingServicePayloadItem> services, string notes, object bookingData)
        {
            try
            {
                EnsureCurrentBooking();
                var json = JsonSerializer.Serialize(bookingData);
                Debug.WriteLine("===== BOOKING REQUEST =====");
                Console.WriteLine("===== BOOKING REQUEST =====");
                Console.WriteLine($"Date: {Booking.FormatApiDate(CurrentBooking.Date)}");
                Console.WriteLine($"Payment Method: {Booking.NormalizePaymentMethod(CurrentBooking.PaymentMethod)}");
                Console.WriteLine($"Notes: {(string.IsNullOrWhiteSpace(notes) ? "(empty)" : notes)}");
                Console.WriteLine($"Services Count: {services?.Count ?? 0}");
                Console.WriteLine($"Total Price: {CalculateTotalPrice():C}");
                Console.WriteLine();

                if (services != null && services.Count > 0)
                {
                    for (int i = 0; i < services.Count; i++)
                    {
                        var item = services[i];
                        var service = SelectedServices.FirstOrDefault(s => s.Id == item.service_id);
                        CurrentBooking.ServiceDateMap.TryGetValue(item.service_id, out var svcDate);
                        CurrentBooking.ServiceSlotMap.TryGetValue(item.service_id, out var svcSlot);

                        Console.WriteLine($"Service {i + 1}:");
                        Console.WriteLine($"  Service ID: {item.service_id}");
                        Console.WriteLine($"  Service Name: {service?.NameServies ?? "Unknown"}");
                        Console.WriteLine($"  Service Price: {service?.PriceServies ?? "Unknown"}");
                        Console.WriteLine($"  Service Duration: {service?.TimeServies ?? 0} minutes");
                        Console.WriteLine($"  Provider ID: {item.provider_id}");
                        Console.WriteLine($"  Date: {(svcDate == default ? Booking.FormatApiDate(CurrentBooking.Date) : Booking.FormatApiDate(svcDate))}");
                        Console.WriteLine($"  Selected Slot (UI): {svcSlot ?? "(not stored)"}");
                        Console.WriteLine($"  Start Time (Payload): {item.start_time}");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("===== PAYLOAD =====");
                Console.WriteLine($"Final JSON: {json}");
                Console.WriteLine("===========================");
                Debug.WriteLine($"Final JSON: {json}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LogBookingRequest] Error: {ex.Message}");
            }
        }

        private async Task SendBookingRequest(object bookingData, TimeSpan totalEndTime)
        {
            IsBookingInProgress = true;
            BookingErrorMessage = string.Empty;

            try
            {
                // Attempt to create booking
                var result = await _apiServices.CreateBookingAsync(bookingData);

                // Success (201)
                if (result.Success && result.StatusCode == 201)
                {
                    string bookingNumber = string.Empty;
                    bool reminderFailed = false;  // Track if reminder needs retry

                    try
                    {
                        if (result.ResponseJson != null && result.ResponseJson.RootElement.TryGetProperty("data", out var dataEl))
                        {
                            if (dataEl.TryGetProperty("booking_number", out var bn)) bookingNumber = bn.GetString();
                            else if (dataEl.TryGetProperty("number", out var nb)) bookingNumber = nb.GetString();
                            else if (dataEl.TryGetProperty("id", out var id)) bookingNumber = id.GetRawText();

                            // Check if reminder is null (booking succeeded but reminder failed)
                            if (dataEl.TryGetProperty("reminder", out var reminderEl) && reminderEl.ValueKind == System.Text.Json.JsonValueKind.Null)
                            {
                                reminderFailed = true;
                                Console.WriteLine("[SendBookingRequest] Booking succeeded but reminder is null - will retry reminder creation");
                            }
                        }
                    }
                    catch { }

                    LastBookingNumber = bookingNumber;

                    // If reminder failed but booking succeeded, try to create reminder separately
                    if (reminderFailed && IsReminderEnabled && CurrentBooking.ReminderOffsetHours.HasValue && int.TryParse(bookingNumber, out int appointmentId))
                    {
                        Console.WriteLine($"[SendBookingRequest] Retrying reminder creation for appointment {appointmentId}");
                        await RetryReminderCreation(appointmentId, CurrentBooking.ReminderOffsetHours.Value);
                    }

                    var pouo = new SuccessfullyBookingNow();
                    await App.Current.MainPage.ShowPopupAsync(pouo);

                    // Clear draft and local booking state
                    try { SecureStorage.Remove("booking_draft"); } catch { }
                    ClearBookingData();

                    // Navigate to bookings page to show new booking
                    await NavigationService.NavigateToPage(nameof(BookingPage));
                    return;
                }

                // Unauthorized - try refresh once
                if (result.StatusCode == 401)
                {
                    var refreshed = await _apiServices.RefreshTokenAsync();
                    if (refreshed)
                    {
                        var retry = await _apiServices.CreateBookingAsync(bookingData);
                        if (retry.Success && retry.StatusCode == 201)
                        {
                            LastBookingNumber = "";
                            try
                            {
                                if (retry.ResponseJson != null && retry.ResponseJson.RootElement.TryGetProperty("data", out var dataEl))
                                {
                                    if (dataEl.TryGetProperty("booking_number", out var bn)) LastBookingNumber = bn.GetString();
                                    else if (dataEl.TryGetProperty("number", out var nb)) LastBookingNumber = nb.GetString();

                                    // Check reminder again
                                    if (dataEl.TryGetProperty("reminder", out var reminderEl) && reminderEl.ValueKind == System.Text.Json.JsonValueKind.Null)
                                    {
                                        if (IsReminderEnabled && CurrentBooking.ReminderOffsetHours.HasValue && int.TryParse(LastBookingNumber, out int appointmentId))
                                        {
                                            Console.WriteLine($"[SendBookingRequest] Retrying reminder creation for appointment {appointmentId}");
                                            await RetryReminderCreation(appointmentId, CurrentBooking.ReminderOffsetHours.Value);
                                        }
                                    }
                                }
                            }
                            catch { }

                            await Toast.Make(AppResource.Bookingsuccessful).Show();
                            ClearBookingData();
                            await NavigationService.NavigateToPage(nameof(BookingPage));
                            return;
                        }
                        result = retry; // fallthrough to handle other statuses
                    }
                    else
                    {
                        // No refresh possible: preserve draft and require login
                        try { await SecureStorage.SetAsync("booking_draft", JsonSerializer.Serialize(bookingData)); } catch { }
                        await ShellNavigationManager.NavigateToLoginAndClear();
                        return;
                    }
                }

                // Forbidden - check for OTP requirement
                if (result.StatusCode == 403)
                {
                    bool requiresOtp = false;
                    try
                    {
                        if (result.ResponseJson != null && result.ResponseJson.RootElement.TryGetProperty("requires_otp_verification", out var prop))
                            requiresOtp = prop.GetBoolean();
                    }
                    catch { }

                    if (requiresOtp)
                    {
                        // Preserve draft and navigate to OTP/sign-in flow
                        try { await SecureStorage.SetAsync("booking_draft", JsonSerializer.Serialize(bookingData)); } catch { }
                        await ShellNavigationManager.NavigateToLoginAndClear();
                        var popup = new CanyouAcpetPhonenumber();
                        await App.Current.MainPage.ShowPopupAsync(popup);
                        return;
                    }

                    await Toast.Make(AppResource.Bookingfailed).Show();
                    return;
                }

                // Validation / business errors (422) - handle slot conflicts specially
                if (result.StatusCode == 422)
                {
                    var userMessage = !string.IsNullOrWhiteSpace(result.Message) ? result.Message : AppResource.Bookingfailed;
                    BookingErrorMessage = userMessage;
                    await Toast.Make(userMessage).Show();

                    try
                    {
                        if (SelectedServices != null && SelectedServices.Count > 0)
                        {
                            var service = SelectedServices[0];
                            CurrentBooking.TryGetProviderId(service.Id, SelectedProvider?.Id, !IsPerServiceBookingMode(), out var providerId);
                            CurrentBooking.TryGetServiceDate(service.Id, SelectedDate == default ? null : SelectedDate, !IsPerServiceBookingMode(), out var date);
                            if (providerId > 0 && date != default)
                            {
                                var avail = await _apiServices.GetProviderAvailabilityAsync(service.Id, providerId, date);
                                if (avail?.Data != null)
                                {
                                    AvailableSlots.Clear();
                                    foreach (var s in avail.Data.AvailableSlots)
                                    {
                                        AvailableSlots.Add(new SlotItem
                                        {
                                            StartTime = s.StartTime,
                                            EndTime = s.EndTime,
                                            DisplayTime = s.DisplayTime,
                                            DurationMinutes = s.DurationMinutes
                                        });
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SendBookingRequest] Availability refresh failed: {ex.Message}");
                    }

                    return;
                }

                // Rate limited
                if (result.StatusCode == 429)
                {
                    await Toast.Make(AppResource.Bookingfailed).Show();
                    return;
                }

                // Generic failure: show friendly message
                var fallback = !string.IsNullOrWhiteSpace(result.Message) ? result.Message : AppResource.Bookingfailed;
                BookingErrorMessage = fallback;
                await Toast.Make(fallback).Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SendBookingRequest] Exception: {ex.Message}");
                await Toast.Make(AppResource.Connectionerror).Show();
            }
            finally
            {
                IsBookingInProgress = false;
            }
        }


        private async Task RetryReminderCreation(int appointmentId, int offsetHours)
        {
            try
            {
                Console.WriteLine($"[RetryReminderCreation] Creating reminder for appointment {appointmentId} with offset {offsetHours} hours");

                var (success, statusCode, message, response) = await _apiServices.CreateReminderAsync(appointmentId, offsetHours);

                if (success)
                {
                    Console.WriteLine("[RetryReminderCreation] Reminder created successfully");
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Toast.Make("Reminder set successfully", ToastDuration.Short).Show();
                    });
                }
                else if (statusCode == 422)
                {
                    // Validation error - likely too_late
                    Console.WriteLine($"[RetryReminderCreation] Reminder validation error: {message}");
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Toast.Make("Selected reminder duration has already passed. Please try a shorter duration.", ToastDuration.Long).Show();
                    });
                }
                else
                {
                    Console.WriteLine($"[RetryReminderCreation] Failed with status {statusCode}: {message}");
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Toast.Make($"Could not set reminder: {message}", ToastDuration.Short).Show();
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RetryReminderCreation] Exception: {ex.Message}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Toast.Make($"Error setting reminder: {ex.Message}", ToastDuration.Short).Show();
                });
            }
        }

        private TimeSpan ParseTimeString(string timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                return TimeSpan.Zero;


            if (TimeSpan.TryParse(timeString, CultureInfo.InvariantCulture, out var result))
                return result;


            var parts = timeString.Split(':');
            if (parts.Length >= 2 && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var hours)
                && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var minutes))
            {
                int seconds = parts.Length > 2 && int.TryParse(parts[2], out var sec) ? sec : 0;
                return new TimeSpan(hours, minutes, seconds);
            }

            return TimeSpan.Zero;
        }


        public void ClearBookingData()
        {
            SelectedServices.Clear();
            EnsureCurrentBooking();
            CurrentBooking.SelectedServices.Clear();
            CurrentBooking.ClearServiceSelections();

            foreach (var p in WorkTeams)
                p.BorderColor = "#202020";
            SelectedProvider = null;

            SelectedDate = default;
            SelectedTime = default;
            BookingNotes = string.Empty;
            foreach (var d in ProviderDays)
                d.BorderColor = "#444444";

            foreach (var slot in AvailableSlots)
                slot.IsSelected = false;
            SelectedSlot = null;

            AvailableSlots.Clear();
            BookingErrorMessage = string.Empty;

            UpdateTotalPrice();
        }

        /// <summary>
        /// Gets list of providers capable of offering a specific service
        /// </summary>
        /// <param name="serviceId">The service ID to filter by</param>
        /// <returns>List of WorkTeam providers who offer this service</returns>
        private List<WorkTeam> GetProvidersForService(int serviceId)
        {
            if (WorkTeams == null || WorkTeams.Count == 0)
                return new List<WorkTeam>();

            return WorkTeams
                .Where(provider => provider?.Services != null &&
                                   provider.Services.Any(s => s?.Id == serviceId))
                .ToList();
        }

        /// <summary>
        /// Groups selected services by their most suitable provider
        /// Returns a dictionary where key is provider ID and value is list of services that provider can deliver
        /// </summary>
        /// <returns>Dictionary mapping provider ID to list of services</returns>
        private Dictionary<int, List<Servies>> GroupServicesByProvider()
        {
            var groupedServices = new Dictionary<int, List<Servies>>();

            if (SelectedServices == null || SelectedServices.Count == 0)
                return groupedServices;

            // For each selected service, find a suitable provider
            foreach (var service in SelectedServices)
            {
                var capableProviders = GetProvidersForService(service.Id);

                if (capableProviders.Count == 0)
                {
                    // No provider found for this service - log issue
                    Console.WriteLine($"[ServiceBooking] No provider found for service: {service?.NameServies} (ID: {service?.Id})");
                    continue;
                }

                // Prefer previously selected provider if they can deliver this service
                WorkTeam selectedProvider = null;
                if (SelectedProvider != null && capableProviders.Any(p => p.Id == SelectedProvider.Id))
                {
                    selectedProvider = SelectedProvider;
                }
                else
                {
                    // Auto-select first capable provider
                    selectedProvider = capableProviders.FirstOrDefault();
                    Console.WriteLine($"[ServiceBooking] Auto-assigned service '{service?.NameServies}' to provider '{selectedProvider?.Name}'");
                }

                if (selectedProvider != null)
                {
                    if (!groupedServices.ContainsKey(selectedProvider.Id))
                    {
                        groupedServices[selectedProvider.Id] = new List<Servies>();
                    }
                    groupedServices[selectedProvider.Id].Add(service);
                }
            }

            return groupedServices;
        }

        /// <summary>
        /// Validates that all selected services have available providers
        /// </summary>
        /// <returns>True if all services have at least one capable provider</returns>
        private bool ValidateAllServicesHaveProviders()
        {
            if (SelectedServices == null || SelectedServices.Count == 0)
                return false;

            foreach (var service in SelectedServices)
            {
                var capableProviders = GetProvidersForService(service.Id);
                if (capableProviders.Count == 0)
                {
                    Console.WriteLine($"[ServiceBooking] No provider available for service: {service?.NameServies}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Finds the best available provider for a given service, considering date and slot availability
        /// </summary>
        /// <param name="service">The service to find a provider for</param>
        /// <param name="date">The desired booking date</param>
        /// <returns>The most suitable WorkTeam provider, or null if none available</returns>
        private async Task<WorkTeam> FindBestProviderForServiceAsync(Servies service, DateTime date)
        {
            var capableProviders = GetProvidersForService(service.Id);

            if (capableProviders.Count == 0)
            {
                return null;
            }

            // If previously selected provider can deliver, prefer them
            if (SelectedProvider != null && capableProviders.Any(p => p.Id == SelectedProvider.Id))
            {
                return SelectedProvider;
            }

            // Check availability for each capable provider and return first with available slots
            foreach (var provider in capableProviders)
            {
                var availability = await _apiServices.GetProviderAvailabilityAsync(service.Id, provider.Id, date);
                if (availability?.Data?.AvailableSlots != null && availability.Data.AvailableSlots.Count > 0)
                {
                    Console.WriteLine($"[ServiceBooking] Found available slots for service '{service?.NameServies}' with provider '{provider?.Name}'");
                    return provider;
                }
            }

            // If no provider has available slots, return first capable provider anyway
            // This allows the user to proceed and see availability errors from API
            Console.WriteLine($"[ServiceBooking] No provider has available slots for service '{service?.NameServies}', returning first capable provider");
            return capableProviders.FirstOrDefault();
        }

        /// <summary>
        /// Checks if a specific provider can offer a given service
        /// </summary>
        /// <param name="provider">The WorkTeam provider to check</param>
        /// <param name="serviceId">The service ID to check</param>
        /// <returns>True if provider offers the service, false otherwise</returns>
        private bool CanProviderOfferService(WorkTeam provider, int serviceId)
        {
            if (provider == null || provider.Services == null)
                return false;

            return provider.Services.Any(s => s?.Id == serviceId);
        }

        /// <summary>
        /// Gets a list of services that are selected but NOT offered by the primary provider
        /// These services will need alternative provider selection in the UI
        /// </summary>
        /// <returns>List of services needing alternative providers</returns>
        public List<Servies> GetServicesNeedingAlternativeProvider()
        {
            var result = new List<Servies>();

            if (SelectedServices == null || SelectedServices.Count == 0)
                return result;

            if (SelectedProvider == null)
            {
                // If no primary provider selected, all services need providers
                return SelectedServices.ToList();
            }

            // Find services that primary provider doesn't support
            foreach (var service in SelectedServices)
            {
                if (!CanProviderOfferService(SelectedProvider, service.Id))
                {
                    result.Add(service);
                    Console.WriteLine($"[ServiceBooking] Service '{service?.NameServies}' (ID: {service?.Id}) needs alternative provider (primary provider '{SelectedProvider.Name}' doesn't support it)");
                }
            }

            return result;
        }

        /// <summary>
        /// Gets usable providers for a specific service (filtered list safe for UI binding)
        /// </summary>
        /// <param name="serviceId">The service ID to get providers for</param>
        /// <returns>ObservableCollection of WorkTeam providers that support this service</returns>
        public ObservableCollection<WorkTeam> GetAlternativeProvidersForService(int serviceId)
        {
            var providers = GetProvidersForService(serviceId);
            return new ObservableCollection<WorkTeam>(providers);
        }

        /// <summary>
        /// Checks if the primary provider offers a specific service
        /// </summary>
        /// <param name="serviceId">The service ID to check</param>
        /// <returns>True if primary provider offers it, false otherwise or if no primary provider selected</returns>
        public bool DoesPrimaryProviderSupportService(int serviceId)
        {
            if (SelectedProvider == null)
                return false;

            return CanProviderOfferService(SelectedProvider, serviceId);
        }

        [ObservableProperty] private string userName;
        [ObservableProperty] private string password;
        [ObservableProperty] private string confirmPassword;
        [ObservableProperty] private string? imageUser;
        [ObservableProperty] private string email;
        [ObservableProperty] private string fullName;
        [ObservableProperty] private string phone;
        [ObservableProperty] private bool phoneVerified = false;

        partial void OnIsLoadUserChanged(bool value)
        {
            OnPropertyChanged(nameof(CanUpdateUser));
        }

        [ObservableProperty] private string userFirstName = string.Empty;

        partial void OnUserFirstNameChanged(string value)
        {
            RefreshUserEditState();
            OnPropertyChanged(nameof(CanUpdateUser));
        }

        partial void OnPhoneChanged(string value)
        {
            RefreshUserEditState();
            OnPropertyChanged(nameof(CanUpdateUser));
        }

        partial void OnSelectedImagePathChanged(string value)
        {
            RefreshUserEditState();
            OnPropertyChanged(nameof(CanUpdateUser));
        }

        public ICommand UpDateUserCommand { get; }
        [ObservableProperty]
        private int notificationCount;
        public string PhoneVerificationStatus
        {
            get
            {
                if (PhoneVerified)
                {
                    return AppResource.PhoneNumberAlreadyVerified ?? "Phone Verified";
                }
                else
                {
                    return AppResource.PhoneNumberNotVerified ?? "Phone Not Verified";
                }
            }
        }

        private string avatar;
        private string originalUserFirstName = string.Empty;
        private string originalPhone = string.Empty;
        private string originalAvatar = string.Empty;
        private string originalSelectedImagePath = string.Empty;
        private bool hasUserProfileChanges;

        public bool HasUserProfileChanges
        {
            get => hasUserProfileChanges;
            private set
            {
                if (hasUserProfileChanges != value)
                {
                    hasUserProfileChanges = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanUpdateUser));
                }
            }
        }

        public bool CanUpdateUser => !IsLoadUser && HasUserProfileChanges;

        private void CaptureUserEditSnapshot()
        {
            originalUserFirstName = UserFirstName ?? string.Empty;
            originalPhone = Phone ?? string.Empty;
            originalAvatar = Avatar ?? string.Empty;
            originalSelectedImagePath = SelectedImagePath ?? string.Empty;
            HasUserProfileChanges = false;
            OnPropertyChanged(nameof(CanUpdateUser));
        }

        /// <summary>
        /// Public helper to mark current user-edit state as saved / no pending changes.
        /// This will capture the current values as the original snapshot so subsequent
        /// edits are compared against these values.
        /// </summary>
        public void MarkUserProfileChangesSaved()
        {
            CaptureUserEditSnapshot();
        }

        private void RefreshUserEditState()
        {
            HasUserProfileChanges =
                !string.Equals(UserFirstName ?? string.Empty, originalUserFirstName, StringComparison.Ordinal) ||
                !string.Equals(Phone ?? string.Empty, originalPhone, StringComparison.Ordinal) ||
                !string.Equals(Avatar ?? string.Empty, originalAvatar, StringComparison.Ordinal) ||
                !string.Equals(SelectedImagePath ?? string.Empty, originalSelectedImagePath, StringComparison.Ordinal);
        }

        public string Avatar
        {
            get => avatar;
            set
            {
                if (avatar != value)
                {
                    avatar = value;
                    OnPropertyChanged();
                    RefreshUserEditState();
                    OnPropertyChanged(nameof(CanUpdateUser));
                }
            }
        }

        private User currentUser;

        private async Task LoadUser()
        {
            try
            {
                IsLoadUser = true;

                currentUser = await _apiServices.GetUserAsync();

                if (currentUser != null)
                {
                    UserName = currentUser.UserName;
                    UserFirstName = currentUser.UserName ?? string.Empty;  // ✅ Initialize from API first_name
                    Email = currentUser.Email;
                    FullName = currentUser.FullName;
                    Phone = currentUser.Phone;
                    PhoneVerified = currentUser.PhoneVerified;

                    Avatar = currentUser.ProfileImageUrl ?? "default_avatar.png";
                    CaptureUserEditSnapshot();

                    Console.WriteLine($"✅ [AppViewModel] User initialized: UserFirstName = '{UserFirstName}', PhoneVerified = {PhoneVerified}");
                }
            }
            finally
            {
                IsLoadUser = false;
                OnPropertyChanged(nameof(CanUpdateUser));
            }
        }

        public async Task LoadUserDataAsync()
        {
            try
            {
                IsLoadUser = true;
                OnPropertyChanged(nameof(CanUpdateUser));

                Console.WriteLine("📥 [AppViewModel] Starting LoadUserDataAsync");

                currentUser = await _apiServices.GetUserAsync();

                if (currentUser == null)
                {
                    Console.WriteLine("⚠️ [AppViewModel] GetUserAsync returned null");
                    return;
                }

                Console.WriteLine($"✅ [AppViewModel] User loaded: {currentUser.Email}");

                // Update properties with null checks
                try
                {
                    UserName = currentUser.UserName ?? "";
                    Console.WriteLine($"✅ [AppViewModel] UserName set: {UserName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ [AppViewModel] Error setting UserName: {ex.Message}");
                    UserName = "";
                }

                try
                {
                    Email = currentUser.Email ?? "";
                    Console.WriteLine($"✅ [AppViewModel] Email set: {Email}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ [AppViewModel] Error setting Email: {ex.Message}");
                    Email = "";
                }

                try
                {
                    FullName = currentUser.FullName ?? "";
                    Console.WriteLine($"✅ [AppViewModel] FullName set: {FullName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ [AppViewModel] Error setting FullName: {ex.Message}");
                    FullName = "";
                }

                try
                {
                    Phone = currentUser.Phone ?? "";
                    Console.WriteLine($"✅ [AppViewModel] Phone set");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ [AppViewModel] Error setting Phone: {ex.Message}");
                    Phone = "";
                }

                try
                {
                    Avatar = currentUser.ProfileImageUrl ?? "default_avatar.png";
                    Console.WriteLine($"✅ [AppViewModel] Avatar set: {Avatar}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ [AppViewModel] Error setting Avatar: {ex.Message}");
                    Avatar = "default_avatar.png";
                }

                // ✅ CRITICAL FIX: Only update UserFirstName if it's empty or null
                // This prevents overwriting recently edited values after navigation
                try
                {
                    if (string.IsNullOrWhiteSpace(UserFirstName))
                    {
                        UserFirstName = currentUser.UserName ?? string.Empty;
                        Console.WriteLine($"✅ [AppViewModel] UserFirstName initialized from API: '{UserFirstName}'");
                    }
                    else
                    {
                        Console.WriteLine($"ℹ️ [AppViewModel] UserFirstName already set ('{UserFirstName}'), skipping API override to preserve user edits");
                    }

                    CaptureUserEditSnapshot();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ [AppViewModel] Error setting UserFirstName: {ex.Message}");
                    if (string.IsNullOrWhiteSpace(UserFirstName))
                        UserFirstName = "";
                }

                Console.WriteLine("✅ [AppViewModel] LoadUserDataAsync completed successfully");
                // After loading user, try to restore any saved booking draft (preserve booking flow across login)
                try
                {
                    await RestoreBookingDraftIfPresentAsync();
                }
                catch (Exception rex)
                {
                    Console.WriteLine($"[RestoreBookingDraft] Failed: {rex.Message}");
                }
            }
            catch (NullReferenceException nex)
            {
                Console.WriteLine($"❌ [AppViewModel] NullReferenceException in LoadUserDataAsync: {nex.Message}");
                Console.WriteLine($"   Stack: {nex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AppViewModel] Exception in LoadUserDataAsync: {ex.Message}");
                Console.WriteLine($"   Type: {ex.GetType().Name}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
            }
            finally
            {
                IsLoadUser = false;
                OnPropertyChanged(nameof(CanUpdateUser));
                Console.WriteLine("ℹ️ [AppViewModel] LoadUserDataAsync finished (IsLoadUser = false)");
            }
        }

        private async Task RestoreBookingDraftIfPresentAsync()
        {
            try
            {
                var draftJson = await SecureStorage.GetAsync("booking_draft");
                if (string.IsNullOrWhiteSpace(draftJson))
                    return;

                Console.WriteLine("🔄 Restoring booking draft from SecureStorage");

                using var doc = JsonDocument.Parse(draftJson);
                var root = doc.RootElement;

                // Parse date
                if (root.TryGetProperty("date", out var dateEl) && DateTime.TryParse(dateEl.GetString(), out var dateVal))
                {
                    SelectedDate = dateVal;
                }

                // Parse services
                if (root.TryGetProperty("services", out var servicesEl) && servicesEl.ValueKind == JsonValueKind.Array)
                {
                    SelectedServices.Clear();
                    CurrentBooking.SelectedServices.Clear();

                    foreach (var sEl in servicesEl.EnumerateArray())
                    {
                        if (sEl.TryGetProperty("service_id", out var sidEl) && sidEl.TryGetInt32(out var sid))
                        {
                            // Look in Services or FilteredServices
                            var svc = Services?.FirstOrDefault(x => x.Id == sid) ?? FilteredServices?.FirstOrDefault(x => x.Id == sid);
                            if (svc != null)
                            {
                                SelectedServices.Add(svc);
                                CurrentBooking.SelectedServices.Add(svc);
                            }
                        }
                    }
                }

                // Parse provider & start_time
                int providerId = 0;
                string startTime = null;
                if (root.TryGetProperty("services", out var svs) && svs.ValueKind == JsonValueKind.Array)
                {
                    var first = svs.EnumerateArray().FirstOrDefault();
                    if (first.ValueKind != JsonValueKind.Undefined)
                    {
                        if (first.TryGetProperty("provider_id", out var pidEl))
                            providerId = pidEl.GetInt32();

                        if (first.TryGetProperty("start_time", out var stEl))
                            startTime = stEl.GetString();
                    }
                }

                if (providerId != 0)
                {
                    var provider = WorkTeams?.FirstOrDefault(w => w.Id == providerId);
                    if (provider != null)
                    {
                        SelectedProvider = provider;
                    }
                }

                // Refresh available slots for selected provider/date and select the matching start time
                if (SelectedProvider != null && SelectedDate != default)
                {
                    await LoadAvailableSlotsAsync();

                    if (!string.IsNullOrWhiteSpace(startTime))
                    {
                        var slot = AvailableSlots.FirstOrDefault(s => s.StartTime == startTime || s.DisplayTime == startTime);
                        if (slot != null)
                        {
                            foreach (var s in AvailableSlots) s.IsSelected = false;
                            slot.IsSelected = true;
                            SelectedSlot = slot;

                            if (TimeSpan.TryParse(slot.StartTime, out var parsedTime))
                            {
                                SelectedTime = parsedTime;
                                CurrentBooking.Time = SelectedTime;
                            }
                        }
                    }
                }

                // Update CurrentBooking date/provider/service list
                CurrentBooking.Date = SelectedDate;
                if (SelectedProvider != null) CurrentBooking.ProviderId = SelectedProvider.Id.ToString();

                // Remove draft
                try { SecureStorage.Remove("booking_draft"); } catch { }

                // Navigate to booking page to allow user to continue
                try
                {
                    await NavigationService.NavigateToPage(NavigationService.ROUTE_TERM_BOOKING);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RestoreBookingDraft] Navigation error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestoreBookingDraft] Error parsing draft: {ex.Message}");
            }
        }

        public ICommand LoadCurrentUserCommand { get; }

        public string Name { get; set; }



        public ICommand UpdateUserCommand { get; }

        private async Task UpdateUserInfo()
        {
            try
            {


                if (!string.IsNullOrWhiteSpace(SelectedImagePath))
                {
                    bool fileExists = File.Exists(SelectedImagePath);
                    Console.WriteLine($"📋 Image File Exists: {fileExists}");
                    if (!fileExists)
                    {
                        Console.WriteLine($"❌ File not found at path: {SelectedImagePath}");
                    }
                }
                Console.WriteLine(new string('=', 60) + "\n");

                if (string.IsNullOrWhiteSpace(UserFirstName) && string.IsNullOrWhiteSpace(SelectedImagePath))
                {
                    await Toast.Make(AppResource.Pleaseenterthefirstnameorselectanimage, ToastDuration.Short).Show();
                    return;
                }

                Console.WriteLine("📤 Calling UpdateUserProfileAsync...\n");
                var apiResponse = await _apiServices.UpdateUserProfileAsync(UserFirstName, SelectedImagePath, Phone);


                Console.WriteLine("\n" + new string('=', 60));
                Console.WriteLine("📥 RESPONSE RECEIVED FROM API");
                Console.WriteLine(new string('=', 60));
                Console.WriteLine($"📊 Success: {apiResponse?.Success}");
                Console.WriteLine($"📊 Message: {apiResponse?.Message}");
                Console.WriteLine($"📊 Data: {(apiResponse?.Data != null ? "Present" : "Null")}");
                if (apiResponse?.Data != null)
                {
                    Console.WriteLine($"   - Id: {apiResponse.Data.Id}");
                    Console.WriteLine($"   - FirstName: {apiResponse.Data.FirstName}");
                    Console.WriteLine($"   - ProfileImageUrl: {apiResponse.Data.ProfileImageUrl}");
                }
                Console.WriteLine(new string('=', 60) + "\n");


                if (apiResponse?.Success == true)
                {
                    // ✅ Update UserFirstName from API response
                    if (apiResponse?.Data != null && !string.IsNullOrWhiteSpace(apiResponse.Data.FirstName))
                    {
                        UserFirstName = apiResponse.Data.FirstName;
                        Console.WriteLine($"✅ UserFirstName updated from API response: '{UserFirstName}'");
                    }
                    else if (string.IsNullOrWhiteSpace(UserFirstName))
                    {
                        // Only log warning if both API and local are empty
                        Console.WriteLine($"⚠️ API response FirstName is empty or null, keeping local value: '{UserFirstName}'");
                    }

                    // 🔑 KEY FIX: Use the server URL from the API response instead of local path
                    if (!string.IsNullOrWhiteSpace(apiResponse?.Data?.ProfileImageUrl))
                    {
                        Avatar = apiResponse.Data.ProfileImageUrl;
                        Console.WriteLine($"✅ Profile image updated from API: {apiResponse.Data.ProfileImageUrl}");
                    }

                    SelectedImagePath = string.Empty;
                    var popup = new ConfermChange();
                    await Application.Current.MainPage.ShowPopupAsync(popup);
                    await LoadUserDataAsync();
                    CaptureUserEditSnapshot();

                }
                else if (apiResponse?.Success == false)
                {
                    // ❌ API returned success: false - show error message
                    string errorMessage = apiResponse?.Message ?? "فشل في تحديث البيانات";

                    await Toast.Make(errorMessage, ToastDuration.Short).Show();

                    var popup = new NoConfermChange();
                    await Application.Current.MainPage.ShowPopupAsync(popup);
                }
                else if (apiResponse?.Success == null)
                {
                    // ⚠️ Success is null - treat as success (for backward compatibility)

                    // ✅ Update UserFirstName from API response
                    if (apiResponse?.Data != null && !string.IsNullOrWhiteSpace(apiResponse.Data.FirstName))
                    {
                        UserFirstName = apiResponse.Data.FirstName;
                        Console.WriteLine($"✅ UserFirstName updated from API response (Success=null): '{UserFirstName}'");
                    }

                    if (!string.IsNullOrWhiteSpace(apiResponse?.Data?.ProfileImageUrl))
                    {
                        Avatar = apiResponse.Data.ProfileImageUrl;
                        Console.WriteLine($"✅ Profile image updated from API: {apiResponse.Data.ProfileImageUrl}");
                    }

                    SelectedImagePath = string.Empty;
                    var popup = new ConfermChange();
                    await Application.Current.MainPage.ShowPopupAsync(popup);
                    CaptureUserEditSnapshot();
                }
                else
                {

                    await Toast.Make(AppResource.Anunexpectederroroccurred, ToastDuration.Short).Show();

                    var popup = new NoConfermChange();
                    await Application.Current.MainPage.ShowPopupAsync(popup);
                }
            }
            catch (Exception ex)
            {

                await Toast.Make(AppResource.Anunexpectederroroccurred, ToastDuration.Short).Show();

                var popup = new NoConfermChange();
                await Application.Current.MainPage.ShowPopupAsync(popup);
            }
        }


[RelayCommand]
private async Task Logout()
        {
            try
            {
                Console.WriteLine("[Logout] START");

                NavigationService.BeginLogout();

                // Keep the selected application language
                var appLanguage = Preferences.Get("AppLanguage", "de-DE");

                ResetUser();

                OneSignalService.Logout();

                SecureStorage.RemoveAll();

                Preferences.Clear();

                // Restore language after clearing preferences
                Preferences.Set("AppLanguage", appLanguage);

                App.ResetAuthenticationCheck();

                Console.WriteLine("[Logout] About to navigate to login page");

                await NavigationService.NavigateToLoginAndClear();

                Console.WriteLine("[Logout] Successfully returned to Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logout] ERROR: {ex}");
                Console.WriteLine($"[Logout] Stack trace: {ex.StackTrace}");
            }
            finally
            {
                Console.WriteLine("[Logout] Resetting logout flag");
                NavigationService.ResetLogoutFlag();
                Console.WriteLine("[Logout] END");
            }
        }





      
        public void ResetUser()
        {
            currentUser = null;

            UserName = "";
            UserFirstName = "";
            Email = "";
            FullName = "";
            Phone = "";
            Avatar = "default_avatar.png";

            IsLoadUser = false;
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

            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                await Toast.Make(AppResource.Pleaseenterthecurrentpassword).Show();
                return;
            }

            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                await Toast.Make(AppResource.Pleaseenterthenewpasswordinbothfields).Show();
                return;
            }

            if (password != confirmPassword)
            {
                await Toast.Make(AppResource.Thenewpasswordsdonotmatch).Show();
                return;
            }

            if (password.Length < 8)
            {
                await Toast.Make(AppResource.Thenewpasswordmustbeatleast8characterslong).Show();
                return;
            }

            if (password == CurrentPassword)
            {
                await Toast.Make(AppResource.Thenewpasswordmustbedifferentfromthecurrentone).Show();
                return;
            }

            try
            {
                await SetAuthorizationHeaderAsync();
                var passwordChangeData = new
                {
                    current_password = CurrentPassword,
                    password = password,
                    password_confirmation = confirmPassword
                };

                var json = JsonSerializer.Serialize(passwordChangeData);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    "https://lookupfriseur.com/api/profile/change-password",
                    content);



                var responseBody = await response.Content.ReadAsStringAsync();


                if (response.IsSuccessStatusCode)
                {

                    CurrentPassword = string.Empty;
                    Password = string.Empty;
                    ConfirmPassword = string.Empty;

                    await Toast.Make(AppResource.PasswordUpdated, ToastDuration.Short).Show();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
                {


                    try
                    {
                        var errorOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var errorResponse = JsonSerializer.Deserialize<Auth.ErrorResponse>(responseBody, errorOptions);

                        if (errorResponse?.Errors != null)
                        {
                            var errorMessages = string.Join("\n",
                                errorResponse.Errors.Values.SelectMany(e => e));
                            await Toast.Make(AppResource.Invaliddata).Show();
                        }
                        else if (!string.IsNullOrEmpty(errorResponse?.Message))
                        {
                            await Toast.Make($"خطأ: {errorResponse.Message}").Show();
                        }
                        else
                        {
                            await Toast.Make(AppResource.DataverificationfailedPleasecheckthevalidityofthepasswords).Show();
                        }
                    }
                    catch
                    {
                        await Toast.Make(AppResource.FailedtochangethepasswordPleasechecktheentereddata).Show();
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {

                    await Toast.Make(AppResource.Thecurrentpasswordisincorrect).Show();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {

                    await Toast.Make(AppResource.InvalidrequestPleasechecktheentereddata).Show();
                }
                else
                {


                    await Toast.Make(AppResource.Failedtochangethepassword).Show();
                }
            }
            catch (Exception ex)
            {

                await Toast.Make(AppResource.Connectionerror).Show();
            }
        }



        public ICommand DeleteAccountCommand { get; }

        private async Task DeleteAccountAsync(string currentPassword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    await App.Current.MainPage.ShowPopupAsync(
                        new PleaseEnterCurrentPasswordPopup());

                    return;
                }


                var result = await _apiServices.DeleteCurrentUserProfileAsync(currentPassword);


                if (result == null)
                {
                    await Logout();
                    return;
                }


                if (result == "WrongPassword")
                {
                    await Toast.Make(AppResource.iSPASSWORDNOTCORECED
                        ,
                        ToastDuration.Short).Show();

                    return;
                }


                await Toast.Make(
                    AppResource.DeleteAccountError,
                    ToastDuration.Short).Show();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private readonly services.NotificationService _notificationService;

        private bool _notificationsInitialized = false;

      
        private static CancellationTokenSource _appCancellation = new CancellationTokenSource();
        public static CancellationToken AppCancellationToken => _appCancellation.Token;

        public void CancelRunningTasks()
        {
            try
            {
                Console.WriteLine("[AppViewModel] Cancelling running tasks...");
                try { _appCancellation.Cancel(); } catch { }
                try { _appCancellation.Dispose(); } catch { }
                _appCancellation = new CancellationTokenSource();
                Console.WriteLine("[AppViewModel] Cancellation token recreated");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppViewModel] CancelRunningTasks error: {ex.Message}");
            }
        }

     
        public void ClearViewModelState()
        {
            try
            {
                Console.WriteLine("[AppViewModel] Clearing in-memory viewmodel state...");

                // Clear observable collections
                Services?.Clear();
                FilteredServices?.Clear();
                Bookings?.Clear();
                Appointments?.Clear();
                UpcomingAppointments?.Clear();
                PreviousAppointments?.Clear();
                CanceledAppointments?.Clear();
                SelectedServices?.Clear();
                CurrentBooking?.ClearServiceSelections();
                CurrentBooking?.SelectedServices?.Clear();
                WorkTeams?.Clear();
                FilteredWorkTeams?.Clear();
                Notifications?.Clear();
                ProviderDays?.Clear();
                AvailableSlots?.Clear();

              
                currentUser = null;
                UserName = string.Empty;
                UserFirstName = string.Empty;
                Email = string.Empty;
                FullName = string.Empty;
                Phone = string.Empty;
                Avatar = null;

               
                try
                {
                }
                catch { }

                Console.WriteLine("[AppViewModel] In-memory state cleared");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppViewModel] ClearViewModelState error: {ex.Message}");
            }
        }

        partial void OnUnreadNotificationCountChanged(int value)
        {
            System.Diagnostics.Debug.WriteLine($"[AppViewModel] UnreadNotificationCount changed on VM {GetHashCode()}: {value}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Keep both counts in sync so UI bound to NotificationCount updates
                NotificationCount = value;
            });
        }

        /// <summary>
        /// Called by views to ensure the notification count is loaded before binding occurs.
        /// Reads shared NotificationStateService first; if zero, fetches unread count from the API once.
        /// </summary>
        public async Task InitializeNotificationsAsync()
        {
            if (_notificationsInitialized) return;

            try
            {
                int count = _notificationState?.UnreadCount ?? 0;

                if (count == 0)
                {
                    count = await _notificationService.GetUnreadCountAsync();
                    _notificationState?.SetUnreadCount(count);
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UnreadNotificationCount = count;
                    NotificationCount = count;
                });
                _notificationsInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeNotificationsAsync failed: {ex.Message}");
            }
        }

        public async Task LoadNotificationCountAsync()
        {
            var count = await _notificationService.GetUnreadCountAsync();
            UnreadNotificationCount = count;
            NotificationCount = count;
            _notificationState?.SetUnreadCount(count);
        }


        [ObservableProperty]
        private ObservableCollection<DayItem> providerDays = new();

        [ObservableProperty]
        private DateTime selectedDate;

        public ICommand SelectDayCommand { get; }

        public IRelayCommand<SlotItem> SelectSlotCommand => new RelayCommand<SlotItem>(OnSelectSlot);


        private void LoadCurrentWeekDays()
        {
            ProviderDays.Clear();

            var today = DateTime.Today;


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
                await LoadAvailableSlotsAsync();
        }


        private void OnSelectSlot(SlotItem slot)
        {
            if (slot == null) return;

            try
            {

                foreach (var s in AvailableSlots)
                    s.IsSelected = false;


                slot.IsSelected = true;
                SelectedSlot = slot;


                if (TimeSpan.TryParse(slot.StartTime, out var parsedTime))
                {
                    SelectedTime = parsedTime;
                    CurrentBooking.Time = SelectedTime;
                }
                else
                {

                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Toast.Make(AppResource.Errorreadingthetime, ToastDuration.Short).Show();
                    });
                    return;
                }

                CurrentBooking.Date = SelectedDate;

                if (SelectedProvider != null)
                    CurrentBooking.ProviderId = SelectedProvider.Id.ToString();


            }
            catch (Exception ex)
            {

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Toast.Make($"خطأ: {ex.Message}", ToastDuration.Short).Show();
                });
            }
        }

        [ObservableProperty]
        private ObservableCollection<SlotItem> availableSlots = new();

        [ObservableProperty]
        private AvailabilityData currentAvailability;


        public bool HasNoAvailableSlots => AvailableSlots?.Count == 0;


        [ObservableProperty]
        private bool hasAvailabilityError = false;

        public async Task LoadAvailableSlotsAsync()
        {
            if (SelectedProvider == null || SelectedDate == default)
            {

                return;
            }

            try
            {
                Isloadday = true;
                HasAvailabilityError = false;
                await SetAuthorizationHeaderAsync();


                var selectedService = SelectedServices.FirstOrDefault();

                if (selectedService == null)
                {

                    selectedService = FilteredServices.FirstOrDefault();

                    if (selectedService == null)
                    {

                        await Toast.Make(AppResource.Pleaseselectaservice).Show();
                        return;
                    }


                }


                int providerId = SelectedProvider.Id;
                int serviceId = selectedService.Id;
                string dateStr = SelectedDate.ToString("yyyy-MM-dd");




                string url = $"https://lookupfriseur.com/api/availability/provider" +
                    $"?provider_id={providerId}" +
                    $"&service_id={serviceId}" +
                    $"&date={dateStr}" +
                    $"&branch_id=1";



                var response = await _httpClient.GetAsync(url);



                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var availabilityResponse = JsonSerializer.Deserialize<AvailabilityResponse>(responseBody, options);

                    if (availabilityResponse?.Success == true && availabilityResponse.Data?.AvailableSlots != null)
                    {

                        CurrentAvailability = availabilityResponse.Data;


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


                       // await Toast.Make($"{AppResource.Fetchedsuccessfully} {AvailableSlots.Count} {AppResource.Availabletime}").Show();
                    }
                    else
                    {

                        AvailableSlots.Clear();
                        HasAvailabilityError = true;
                        await Toast.Make(AppResource.Noavailabletimes).Show();
                    }
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();

                    AvailableSlots.Clear();
                    HasAvailabilityError = true;


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

            }
        }


        public void RemoveSelectedService(Servies service)
        {
            if (service == null) return;

            var serviceToRemove = SelectedServices.FirstOrDefault(s => s.Id == service.Id);
            if (serviceToRemove != null)
            {
                serviceToRemove.IsSelected = false;  // تحديث الحالة
                SelectedServices.Remove(serviceToRemove);
                CurrentBooking.SelectedServices.Remove(serviceToRemove);
                CurrentBooking.RemoveServiceSelections(service.Id);
                UpdateTotalPrice();

            }
        }

        private async Task CancelBookingAsync(int bookingId)
        {
            try
            {
                bool confirmed = await App.Current.MainPage.DisplayAlert(
                   AppResource.ConfirmCancelBooking,
                    AppResource.ConfirmCancelBookingMessage,
                    AppResource.Yess,
                    AppResource.Noo
                );

                if (!confirmed)
                {

                    return;
                }

                IsCancelingBooking = true;

                bool success = await _apiServices.CancelBookingAsync(bookingId);

                if (success)
                {

                    await Toast.Make(AppResource.Bookingcanceledsuccessfully, ToastDuration.Short).Show();

                    await LoadBookingsAsync();
                }
                else
                {

                    await Toast.Make(AppResource.Failedtocancelthebooking, ToastDuration.Short).Show();
                }
            }
            catch (Exception ex)
            {

                await Toast.Make($"خطأ: {ex.Message}", ToastDuration.Short).Show();
            }
            finally
            {
                IsCancelingBooking = false;
            }
        }


        [RelayCommand]
        public void ClearSelectedServices()
        {
            SelectedServices.Clear();
            CurrentBooking.SelectedServices.Clear();
            CurrentBooking.ClearServiceSelections();
            UpdateTotalPrice();

        }

        public int GetSelectedServicesCount() => SelectedServices.Count;


        public bool HasSelectedServices() => SelectedServices.Count > 0;



        private void UpdateTotalPrice()
        {
            TotalPrice = SelectedServices.Sum(s =>
            {
                if (string.IsNullOrWhiteSpace(s?.PriceServies))
                {

                    return 0m;
                }


                string priceStr = s.PriceServies.Trim();


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


        public decimal GetTotalPrice()
        {
            return TotalPrice;
        }


        public int GetTotalDuration()
        {
            return SelectedServices.Sum(s => s.TimeServies);
        }

        private async Task EnableReminderTimerAsync()
        {
            try
            {
                Console.WriteLine("\n=== EnableReminderTimerAsync Started ===");

                // ✅ Check if reminder is enabled
                if (!IsReminderEnabled)
                {
                    await Toast.Make("❌ الرجاء تفعيل التنبيهات أولاً", ToastDuration.Short).Show();
                    return;
                }

                // ✅ Convert selected reminder option to TimeSpan
                TimeSpan reminderDuration = SelectedReminderOptionIndex switch
                {
                    0 => TimeSpan.FromHours(1),      // 1 hour
                    1 => TimeSpan.FromHours(3),      // 3 hours
                    2 => TimeSpan.FromHours(6),      // 6 hours
                    3 => TimeSpan.FromHours(12),     // 12 hours
                    4 => TimeSpan.FromDays(1),       // 1 day
                    5 => TimeSpan.FromDays(1),       // 1 day before
                    _ => TimeSpan.FromHours(1)       // Default: 1 hour
                };

                ReminderTime = reminderDuration;
                var reminderLabel = SelectedReminderOptionIndex < ReminderOptions.Count ? ReminderOptions[SelectedReminderOptionIndex].Label : "Unknown";
                Console.WriteLine($"✅ Reminder duration selected: {reminderLabel} ({ReminderTime:hh\\:mm\\:ss})");

                var now = DateTime.Now;
                Console.WriteLine($"   Current time: {now:yyyy-MM-dd HH:mm:ss}");

                // ✅ Find upcoming appointment with null safety
                var upcomingAppointment = Appointments
                    .Where(a => !string.IsNullOrWhiteSpace(a?.AppointmentDate))  // Filter nulls
                    .Select(a =>
                    {
                        bool parsed = DateTime.TryParse(a.AppointmentDate, out var date);
                        return new { Appointment = a, Date = date, WasParsed = parsed };
                    })
                    .Where(x => x.WasParsed && x.Date > now)  // Only valid future dates
                    .OrderBy(x => x.Date)
                    .FirstOrDefault();

                if (upcomingAppointment == null)
                {
                    Console.WriteLine("❌ No upcoming appointments found");
                    await Toast.Make(AppResource.Noupcomingappointments, ToastDuration.Short).Show();
                    return;
                }

                Console.WriteLine($"   Found appointment: {upcomingAppointment.Date:yyyy-MM-dd HH:mm:ss}");

                // ✅ Validate TimeSpan is not default
                if (ReminderTime == TimeSpan.Zero)
                {
                    await Toast.Make("❌ Reminder time must be greater than zero.", ToastDuration.Short).Show();
                    return;
                }

                // ✅ Calculate reminder datetime with date context (from copilot-instructions)
                var reminderDateTime = upcomingAppointment.Date - ReminderTime;

                // ✅ Auto-adjust if reminder is on same day as appointment or later
                if (reminderDateTime >= upcomingAppointment.Date)
                {
                    reminderDateTime = reminderDateTime.AddDays(-1);
                    Console.WriteLine($"⚠️ Auto-adjusted reminder to previous day");
                }

                if (reminderDateTime <= now)
                {
                    Console.WriteLine($"❌ Reminder time in past: {reminderDateTime:yyyy-MM-dd HH:mm:ss}");
                    await Toast.Make(AppResource.Theremindertimemustbebeforethebookingappointment, ToastDuration.Short).Show();
                    return;
                }

                Console.WriteLine($"✅ Reminder set for: {reminderDateTime:yyyy-MM-dd HH:mm:ss}");
                await SendAppointmentReminder(upcomingAppointment.Appointment, reminderDateTime);
            }
            catch (FormatException fex)
            {
                Console.WriteLine($"❌ Format error: {fex.Message}");
                await Toast.Make("❌ Invalid date or time format.", ToastDuration.Short).Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in EnableReminderTimerAsync: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                await Toast.Make($"❌ Error: {ex.Message}", ToastDuration.Short).Show();
            }
        }
        private async Task SendAppointmentReminder(Appointment appointment, DateTime remindAtDateTime)
        {
            try
            {
                await SetAuthorizationHeaderAsync();


                var reminderData = new
                {
                    appointment_id = appointment.Id,
                    remind_at = remindAtDateTime.ToString("yyyy-MM-ddTHH:mm:ss")
                };

                var json = JsonSerializer.Serialize(reminderData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");



                var response = await _httpClient.PostAsync(
                    "https://lookupfriseur.com/api/appointments/reminders",
                    content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    await Toast.Make(AppResource.Remindersentsuccessfully, ToastDuration.Short).Show();
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();

                    await Toast.Make(AppResource.Failedtosend, ToastDuration.Short).Show();
                }
            }
            catch (Exception ex)
            {

                await Toast.Make(AppResource.EROR, ToastDuration.Short).Show();
            }
        }


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
                Message = string.Empty;

                // ✅ Validate input
                if (string.IsNullOrWhiteSpace(Phone))
                {
                    Toast.Make(AppResource.NotFoundPhoneNumber).Show();
                    return;
                }

                // ✅ Call API method with structured response
                var (success, statusCode, errorMessage, retryAfter) =
                    await _apiServices.SendPhoneOtpAsync(Phone);

                // ========================
                // 🟢 SUCCESS CASE (200)
                // ========================
                if (success)
                {
                    OtpSent = true;
                    Toast.Make(AppResource.completesendotppone).Show();

                    // Navigate to OTP verification page
                    await NavigationService.NavigateToPage(
                        NavigationService.ROUTE_OTP_PHONE_NUMBER);

                    return;
                }

                // ========================
                // 🔴 ERROR HANDLING
                // ========================

                Console.WriteLine($"❌ Send OTP failed: Status={statusCode}, Error={errorMessage}");

                // HTTP 400 - Validation errors / Already verified
                if (statusCode == 400)
                {
                    // Check if phone is already verified
                    if (errorMessage?.ToLower().Contains("verified") == true ||
                        errorMessage?.ToLower().Contains("already") == true)
                    {
                        Toast.Make(AppResource.PhoneNumberAlreadyVerified).Show();
                        OtpSent = true; // Consider it as sent since already verified
                    }
                    else
                    {
                        // Generic validation error
                        Toast.Make(errorMessage ?? AppResource.Failedtosendotp).Show();
                    }
                    return;
                }

                // HTTP 429 - Too Many Requests (Rate Limit)
                if (statusCode == 429)
                {
                    Console.WriteLine($"⏳ Rate limited. Retry after {retryAfter} seconds");

                    Message = retryAfter.HasValue
                        ? $"Please wait {retryAfter} seconds before retrying"
                        : "Too many attempts. Please wait before retrying.";

                    Toast.Make("Too many attempts").Show();

                    return;
                }

                // Other HTTP errors
                Toast.Make(AppResource.Failedtosendotp).Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error during send OTP: {ex.Message}\n{ex.StackTrace}");
                Toast.Make("Error sending OTP").Show();
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        private async Task VerifyOtp()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                Message = string.Empty;

                // تحقق من الصيغة
                if (string.IsNullOrWhiteSpace(Otp))
                {
                    Toast.Make(AppResource.Pleaseentertheotp).Show();
                    return;
                }

                // استدعاء API
                var (success, statusCode, errorMessage, retryAfter) =
                    await _apiServices.VerifyPhoneOtpAsync(Phone, Otp);

                // نجاح - 200
                if (success)
                {
                    IsVerified = true;
                    Otp = string.Empty;
                    Toast.Make(AppResource.OTPverifiedsuccessfully).Show();

                    LoadData();

                    // الرجوع للصفحة السابقة
                    await Shell.Current.GoToAsync("..", true);

                }
                // خطأ - 400
                else if (statusCode == 400)
                {
                    Toast.Make(AppResource.PhoneNumberAlreadyVerified).Show();
                    IsVerified = true;
                }
                // أي خطأ آخر
                else
                {
                    Toast.Make(AppResource.FailedToVerifyOtp).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.Make("حدث خطأ").Show();
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
                Message = string.Empty;

                // Call API method with structured response
                var (success, statusCode, errorMessage, retryAfter) =
                    await _apiServices.SendPhoneOtpAsync(Phone);

                if (success)
                {
                    Message = "تم إعادة إرسال الرمز";
                }
                else
                {
                    Message = errorMessage ?? "فشل إعادة الإرسال";
                }
            }
            catch (Exception ex)
            {
                Message = "حدث خطأ";
            }
            finally
            {
                IsBusy = false;
            }
        }


    }
}

