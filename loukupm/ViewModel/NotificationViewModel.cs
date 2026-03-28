using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loukupm.Model;
using loukupm.services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace loukupm.ViewModel
{
    /// <summary>
    /// ViewModel for the NotificationPage
    /// Handles loading, refreshing, and displaying notifications
    /// </summary>
    public partial class NotificationViewModel : ObservableObject
    {
        /// <summary>
        /// Collection of notifications to display
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<NotificationItem> notifications = new();

        /// <summary>
        /// Flag indicating if notifications are currently being refreshed
        /// </summary>
        [ObservableProperty]
        private bool isRefreshing = false;

        /// <summary>
        /// Flag indicating if notifications are still loading for the first time
        /// </summary>
        [ObservableProperty]
        private bool isLoading = false;

        /// <summary>
        /// Error message to display if loading fails
        /// </summary>
        [ObservableProperty]
        private string errorMessage = string.Empty;

        private readonly NotificationService _notificationService;

        public NotificationViewModel()
        {
            _notificationService = new NotificationService();

            // Load notifications automatically when ViewModel is created
            _ = LoadNotificationsAsync();
        }

        /// <summary>
        /// Load all notifications from the API
        /// Used for both initial load and refresh
        /// </summary>
        [RelayCommand]
        public async Task LoadNotifications()
        {
            try
            {
                this.IsRefreshing = true;
                this.ErrorMessage = string.Empty;

                Console.WriteLine("🔄 Loading notifications...");

                var notificationsList = await _notificationService.GetAllNotificationsAsync();

                // Clear existing notifications
                this.Notifications.Clear();

                // Add all notifications to the collection
                if (notificationsList != null && notificationsList.Count > 0)
                {
                    foreach (var notification in notificationsList)
                    {
                        this.Notifications.Add(notification);
                    }

                    Console.WriteLine($"✅ Loaded {this.Notifications.Count} notifications");
                }
                else
                {
                    Console.WriteLine("ℹ️ No notifications found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading notifications: {ex.Message}");
                this.ErrorMessage = $"Failed to load notifications: {ex.Message}";
                this.Notifications.Clear();
            }
            finally
            {
                this.IsRefreshing = false;
            }
        }

        /// <summary>
        /// Internal method for loading notifications (without setting IsRefreshing)
        /// Used for initial load
        /// </summary>
        private async Task LoadNotificationsAsync()
        {
            try
            {
                this.IsLoading = true;
                this.ErrorMessage = string.Empty;

                Console.WriteLine("🔄 Initial load of notifications...");

                var notificationsList = await _notificationService.GetAllNotificationsAsync();

                // Clear existing notifications
                this.Notifications.Clear();

                // Add all notifications to the collection
                if (notificationsList != null && notificationsList.Count > 0)
                {
                    foreach (var notification in notificationsList)
                    {
                        this.Notifications.Add(notification);
                    }

                    Console.WriteLine($"✅ Initially loaded {this.Notifications.Count} notifications");
                }
                else
                {
                    Console.WriteLine("ℹ️ No notifications found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading notifications: {ex.Message}");
                this.ErrorMessage = $"Failed to load notifications: {ex.Message}";
                this.Notifications.Clear();
            }
            finally
            {
                this.IsLoading = false;
            }
        }
    }
}
