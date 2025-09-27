using CommunityToolkit.Mvvm.ComponentModel;
using loukupm.services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using loukupm.Model;
using System;

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

        // Collections
        
        [ObservableProperty] private ObservableCollection<Servies> services;
        [ObservableProperty] private ObservableCollection<Servies> filteredServices;
        [ObservableProperty] private ObservableCollection<Booking> bookings;
        [ObservableProperty] private ObservableCollection<WorkTeam> workTeams;
        [ObservableProperty] private ObservableCollection<Notifiction> notifications;

        private readonly ApiServices _apiServices = new ApiServices();

        public AppViewModel()
        {
            LoadData();

            // Fire and forget
            _ = LoadBookingsAsync();
            _ = LoadNotificationsAsync();
            _ = LoadWorkTeamsAsync();
            _ = LoadServicesAsync();
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
                    Services?.Where(s => s.Catgery== type) ?? Enumerable.Empty<Servies>()
                );
            }
        }
    }
}
