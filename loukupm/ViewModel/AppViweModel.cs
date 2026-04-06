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

       

        [ObservableProperty] private ObservableCollection<Servies> services = new();
        [ObservableProperty] private ObservableCollection<Servies> filteredServices = new();

        [ObservableProperty] private ObservableCollection<Booking> bookings = new();
        [ObservableProperty] private ObservableCollection<Appointment> appointments = new();

        [ObservableProperty] private bool hasNoAppointments = true;

        [ObservableProperty] private ObservableCollection<WorkTeam> filteredWorkTeams = new();
        [ObservableProperty] private ObservableCollection<WorkTeam> workTeams = new();
        [ObservableProperty] private ObservableCollection<Notification> notifications = new();
        [ObservableProperty] private int unreadNotificationCount = 0;
        [ObservableProperty] private bool hasMoreNotifications = false;
        [ObservableProperty] private string nextNotificationCursor = null;
        [ObservableProperty] private ObservableCollection<Booking> viewSrves;
        [ObservableProperty] private ObservableCollection<PolicyandPrivacyS> listpolicyandPrivacy = new();

        [ObservableProperty]
        private ObservableCollection<Servies> selectedServices = new();
        
        
        [ObservableProperty]
        private decimal totalPrice = 0m;

       
        [ObservableProperty]
        private string searchServiceTerm = string.Empty;

        
        [ObservableProperty]
        private string searchTeamTerm = string.Empty;

       
        [ObservableProperty]
        private string reminderMinutes = "60";

        [ObservableProperty]
        private TimeSpan reminderTime = new TimeSpan(1, 0, 0);

       
        [ObservableProperty]
        private string selectedImagePath = string.Empty;

      
        [ObservableProperty]
        private bool isCancelingBooking = false;

        private static readonly Lazy<AppViewModel> _instance = new(() => new AppViewModel());
        public static AppViewModel Instance => _instance.Value;

        private readonly ApiServices _apiServices = new ApiServices();

        private string _token;
        public ICommand SelectServiceButtonCommand { get; }
        private readonly HttpClient _httpClient;
        public IAsyncRelayCommand EnableReminderTimerCommand { get; private set; }
        public IAsyncRelayCommand<int> CancelBookingCommand { get; private set; }
        public AppViewModel()
        {
            LoadData();
            _httpClient = new HttpClient();

           
            DeleteAccountCommand = new Command(async () => await DeleteAccountAsync());
           
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
                    SelectedServices.Add(service);  
                    CurrentBooking.SelectedServices.Add(service);  // إضافة للـ List
                  
                    
                   
                    await Toast.Make(AppResource.celectedserviesiddone, ToastDuration.Short).Show();
                }
                else
                {
                    
                    Console.WriteLine($"⚠️ Service already selected: {service.NameServies}");
                    await Toast.Make(AppResource.theserviewasdone, ToastDuration.Short).Show();
                }

               
                UpdateTotalPrice();

               
                foreach (var s in SelectedServices)
                    Console.WriteLine($"   - {s.NameServies} (Price: '{s.PriceServies}')");
               
            });

           
            LoadAppointmentsCommand = new AsyncRelayCommand(LoadBookingsAsync);
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
            
            string? token = await SecureStorage.GetAsync("auth_token");

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
                    return;


                WorkTeams.Clear();


                foreach (var member in data)
                    WorkTeams.Add(member);


                FilteredWorkTeams = new ObservableCollection<WorkTeam>(WorkTeams);

                IsWorkTeamLoad = false;

                
            }
            catch (Exception ex)
            {
               
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
                if (data == null || data.Count == 0)
                    return;

                Services.Clear();
                foreach (var item in data)
                    Services.Add(item);

                FilteredServices = new ObservableCollection<Servies>(Services);

               
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
                return;
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
                
                if (!ValidateBookingInputs())
                    return;

               
                var (services, totalEndTime) = BuildServicesArray();
                if (services == null || services.Count == 0)
                {
                    await Toast.Make(AppResource.Errorbuildingtheservicelist).Show();
                    return;
                }

               
                decimal totalAmount = CalculateTotalPrice();

                
                string notes = string.IsNullOrWhiteSpace(BookingNotes) ? "ok" : BookingNotes;

              
                var bookingData = BuildBookingRequest(services, totalAmount, notes);

              
                await SendBookingRequest(bookingData, totalEndTime);
            }
            catch (Exception ex)
            {
               return;
            }
        }

        private bool ValidateBookingInputs()
        {
            if (SelectedServices?.Count == 0)
            {
                Toast.Make(AppResource.Pleaseselectatleastoneservice).Show();
                return false;
            }

            if (SelectedProvider == null)
            {
                Toast.Make(AppResource.Pleaseselectaserviceprovider).Show();
                return false;
            }

            if (SelectedDate == default)
            {
                Toast.Make(AppResource.Pleaseselectadate).Show();
                return false;
            }

            if (SelectedSlot == null)
            {
                Toast.Make(AppResource.Pleaseselectatime).Show();
                return false;
            }

            // Verify all services are available from selected provider
            var unavailableServices = SelectedServices
                .Where(s => !FilteredServices.Any(fs => fs.Id == s.Id))
                .ToList();

            if (unavailableServices.Count > 0)
            {
                string serviceList = string.Join(", ", unavailableServices.Select(s => s.NameServies));
                Toast.Make($"الخدمات غير متاحة من {SelectedProvider.Name}: {serviceList}", ToastDuration.Long).Show();
              
                return false;
            }

            return true;
        }

        
        private (List<object> services, TimeSpan totalEndTime) BuildServicesArray()
        {
            var services = new List<object>();
            TimeSpan currentStartTime = ParseTimeString(SelectedSlot.StartTime);

            foreach (var service in SelectedServices)
            {
                
                int serviceDuration = GetServiceDuration(service);
                if (serviceDuration <= 0)
                    serviceDuration = 30;

                TimeSpan endTime = currentStartTime.Add(TimeSpan.FromMinutes(serviceDuration));

               
                services.Add(new
                {
                    service_id = service.Id,
                    provider_id = SelectedProvider.Id,
                    start_time = currentStartTime.ToString(@"hh\:mm")
                });

           
                currentStartTime = endTime;
            }

            return (services, currentStartTime);
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

        private object BuildBookingRequest(List<object> services, decimal totalAmount, string notes)
        {
            var bookingData = new
            {
                date = SelectedDate.ToString("yyyy-MM-dd"),
                payment_method = "cash",
                notes = notes,
                services = services
            };

           
            return bookingData;
        }

        
       
        
        private async Task SendBookingRequest(object bookingData, TimeSpan totalEndTime)
        {
            await SetAuthorizationHeaderAsync();

            var json = JsonSerializer.Serialize(bookingData);
            Console.WriteLine($"📋 JSON: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(
                    "https://test.center-yazan.com/api/bookings",
                    content);

                if (response.IsSuccessStatusCode)
                {
                  
                    await Toast.Make(AppResource.Bookingsuccessful).Show();

                    ClearBookingData();
                    await Shell.Current.GoToAsync(nameof(BookingPage));
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                   
                    await Toast.Make(AppResource.Bookingfailed).Show();
                }
            }
            catch (Exception ex)
            {
              
                await Toast.Make(AppResource.Connectionerror).Show();
            }
        }

       
        private TimeSpan ParseTimeString(string timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
                return TimeSpan.Zero;

           
            if (TimeSpan.TryParse(timeString, out var result))
                return result;

           
            var parts = timeString.Split(':');
            if (parts.Length >= 2 && int.TryParse(parts[0], out var hours) && int.TryParse(parts[1], out var minutes))
            {
                int seconds = parts.Length > 2 && int.TryParse(parts[2], out var sec) ? sec : 0;
                return new TimeSpan(hours, minutes, seconds);
            }

            return TimeSpan.Zero;
        }

        
        private void ClearBookingData()
        {
            
            SelectedServices.Clear();
            CurrentBooking.SelectedServices.Clear();

            
            foreach (var p in WorkTeams)
                p.BorderColor = "#202020";
            SelectedProvider = null;

          
            SelectedDate = default;
            foreach (var d in ProviderDays)
                d.BorderColor = "#444444";

         
            foreach (var slot in AvailableSlots)
                slot.IsSelected = false;
            SelectedSlot = null;

            
            AvailableSlots.Clear();

          
            UpdateTotalPrice();
        }

       
        [ObservableProperty] private string userName;
        [ObservableProperty] private string password;
        [ObservableProperty] private string confirmPassword;
        [ObservableProperty] private string? imageUser;
        [ObservableProperty] private string email;
        [ObservableProperty] private string fullName;

        [ObservableProperty] private string userFirstName = string.Empty;

        public ICommand UpDateUserCommand { get; }
        
       
        public string Avatar 
        { 
            get => ImageUser; 
            set => ImageUser = value; 
        }
        
        private User currentUser;

        private async Task LoadUser()
        {
            IsLoadUser = true;
            currentUser = await _apiServices.GetUserAsync(); 
            if (currentUser != null)
            {
                UserName = currentUser.UserName;
                Email = currentUser.Email;
                FullName = currentUser.FullName;
                ImageUser = currentUser.ProfileImageUrl ?? "default_avatar.png";
            }
            IsLoadUser = false;
        }

       
        public async Task LoadUserDataAsync()
        {
            try
            {
                IsLoadUser = true;
              

              
                currentUser = await _apiServices.GetUserAsync();
                if (currentUser != null)
                {
                    UserName = currentUser.UserName;
                    Email = currentUser.Email;
                    FullName = currentUser.FullName;
                    ImageUser = currentUser.ProfileImageUrl ?? "default_avatar.png";
                   
                }

                IsLoadUser = false;
            }
            catch (Exception ex)
            {
               
                IsLoadUser = false;
            }
        }

        public ICommand LoadCurrentUserCommand { get; }

        public string Name { get; set; }



        public ICommand UpdateUserCommand { get; }

        private async Task UpdateUserInfo()
        {
            try
            {
                
                if (string.IsNullOrWhiteSpace(UserFirstName) && string.IsNullOrWhiteSpace(SelectedImagePath))
                {
                    await Toast.Make(AppResource.Pleaseenterthefirstnameorselectanimage, ToastDuration.Short).Show();
                    return;
                }

              
                var apiResponse = await _apiServices.UpdateUserProfileAsync(UserFirstName, SelectedImagePath);

                
                if (apiResponse?.Success == true)
                {
                    
                    if (!string.IsNullOrWhiteSpace(UserFirstName))
                    {
                        UserFirstName = UserFirstName.Trim();
                       
                    }

                    if (!string.IsNullOrWhiteSpace(SelectedImagePath))
                    {
                        Avatar = SelectedImagePath;
                      
                    }

                   
                    var popup = new ConfermChange();
                    await Application.Current.MainPage.ShowPopupAsync(popup);
                   
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
                    

                    if (!string.IsNullOrWhiteSpace(UserFirstName))
                    {
                        UserFirstName = UserFirstName.Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(SelectedImagePath))
                    {
                        Avatar = SelectedImagePath;
                    }

                    var popup = new ConfermChange();
                    await Application.Current.MainPage.ShowPopupAsync(popup);
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
                    "https://test.center-yazan.com/api/profile/change-password",
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


                
                SecureStorage.RemoveAll();
                Preferences.Clear();

              
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

               

              
                string url = $"https://test.center-yazan.com/api/availability/provider" +
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


                        await Toast.Make($"{AppResource.Fetchedsuccessfully} {AvailableSlots.Count} {AppResource.Availabletime}").Show();
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
                SelectedServices.Remove(serviceToRemove);
                CurrentBooking.SelectedServices.Remove(serviceToRemove);
                UpdateTotalPrice();
                
            }
        }

        private async Task CancelBookingAsync(int bookingId)
        {
            try
            {
                bool confirmed = await App.Current.MainPage.DisplayAlert(
                    "تأكيد الإلغاء",
                    "هل تريد بالفعل إلغاء هذا الحجز؟",
                    "نعم",
                    "لا"
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

        private async Task EnableReminderTimerAsync()
        {
            try
            {
               
                var reminderTime = ReminderTime;
                var now = DateTime.Now;
                var upcomingAppointment = Appointments
                    .FirstOrDefault(a => 
                    {
                        if (DateTime.TryParse(a.AppointmentDate, out var appointmentDate))
                            return appointmentDate > now;
                        return false;
                    });

                if (upcomingAppointment == null)
                {
                    await Toast.Make(AppResource.Noupcomingappointments, ToastDuration.Short).Show();
                   
                    return;
                }

              
                if (!DateTime.TryParse(upcomingAppointment.AppointmentDate, out var appointmentDateTime))
                {
                    await Toast.Make(AppResource.Errorreadingthebookingappointment, ToastDuration.Short).Show();
                    return;
                }
                var reminderDateTimeOnAppointmentDay = appointmentDateTime.Date.Add(reminderTime);

               
                var reminderDateTime = reminderDateTimeOnAppointmentDay >= appointmentDateTime
                    ? reminderDateTimeOnAppointmentDay.AddDays(-1)  
                    : reminderDateTimeOnAppointmentDay;              

                if (reminderDateTime >= appointmentDateTime)
                {
                   
                    await Toast.Make(AppResource.Theremindertimemustbebeforethebookingappointment, ToastDuration.Short).Show();
                    return;
                }

              

                var remindAtDateTime = reminderDateTime;


                // Send to API
                await SendAppointmentReminder(upcomingAppointment, remindAtDateTime);
            }
            catch (Exception ex)
            {
                await Toast.Make(AppResource.EROR, ToastDuration.Short).Show();
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
                    "https://test.center-yazan.com/api/appointments/reminders",
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

      
        
    }
}

