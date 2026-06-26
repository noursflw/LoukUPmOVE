using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loukupm.Model;
using loukupm.services;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Threading;

namespace loukupm.ViewModel
{
    public partial class NotificationViewModel : ObservableObject
    {
        private readonly NotificationService _notificationService;
        private readonly NotificationStateService _notificationStateService;
        private bool _isLoadingRequested;
        private bool _isHandlingSelection;

        [ObservableProperty]
        private ObservableCollection<NotificationItem> notifications = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasNotifications))]
        private int unreadCount;

        [ObservableProperty]
        private NotificationItem? selectedNotification;

        public bool HasNotifications => UnreadCount > 0;

        public NotificationViewModel(NotificationService notificationService, NotificationStateService notificationStateService)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _notificationStateService = notificationStateService ?? throw new ArgumentNullException(nameof(notificationStateService));
            UnreadCount = _notificationStateService.UnreadCount;
            _notificationStateService.UnreadCountChanged += HandleUnreadCountChanged;
        }

        private void HandleUnreadCountChanged(int count)
        {
            MainThread.BeginInvokeOnMainThread(() => UnreadCount = count);
        }

        partial void OnSelectedNotificationChanged(NotificationItem? value)
        {
            // Called on UI thread by binding. Fire-and-wait for selection handling, but guard reentrancy.
            _ = HandleSelectionAsync(value);
        }

        private async Task HandleSelectionAsync(NotificationItem? notification)
        {
            if (notification == null)
                return;

            // Prevent double handling
            if (_isHandlingSelection)
                return;

            _isHandlingSelection = true;

            try
            {
                Console.WriteLine($"🟣 ITEM SELECTED: {notification.Id}");

                if (notification.ReadAt != null)
                    return;

                Console.WriteLine($"🟡 MARK AS READ CALLING: {notification.Id}");
                var success = await _notificationService.MarkAsReadAsync(notification.Id);

                if (!success)
                {
                    Console.WriteLine($"🔴 MARK AS READ FAILED: {notification.Id}");
                    return;
                }

                Console.WriteLine($"🟢 MARK AS READ SUCCESS: {notification.Id}");

                // Update UI state on main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    notification.ReadAt = DateTime.UtcNow;
                    UnreadCount = Math.Max(0, UnreadCount - 1);
                    _notificationStateService.SetUnreadCount(UnreadCount);

                    // Force UI to reflect change by raising notifications collection replacement
                    var list = new ObservableCollection<NotificationItem>(Notifications);
                    Notifications = list;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [NotificationViewModel] HandleSelectionAsync failed: {ex.Message}");
            }
            finally
            {
                // Clear selection so same item can be tapped again
                MainThread.BeginInvokeOnMainThread(() => SelectedNotification = null);
                _isHandlingSelection = false;
            }
        }

        [RelayCommand]
        private async Task LoadNotificationsAsync()
        {
            if (_isLoadingRequested)
            {
                return;
            }

            _isLoadingRequested = true;
            IsLoading = true;
            ErrorMessage = string.Empty;

            Console.WriteLine("🔵 LOAD START");

            try
            {
                await LoadFromApiAsync();
                Console.WriteLine("🟢 LOAD SUCCESS");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 LOAD FAILED: {ex.Message}");
                ErrorMessage = "Failed to load notifications.";
            }
            finally
            {
                IsLoading = false;
                _isLoadingRequested = false;
            }
        }

        [RelayCommand]
        private async Task RefreshNotificationsAsync()
        {
            if (IsRefreshing)
                return;

            IsRefreshing = true;
            ErrorMessage = string.Empty;

            try
            {
                await LoadFromApiAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [NotificationViewModel] Refresh failed: {ex.Message}");
                ErrorMessage = "Failed to refresh notifications.";
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task LoadFromApiAsync()
        {
            try
            {
                var notificationsTask = _notificationService.GetNotificationsAsync();
                var unreadCountTask = _notificationService.GetUnreadCountAsync();

                await Task.WhenAll(notificationsTask, unreadCountTask);

                var notifications = await notificationsTask;
                var unreadCount = await unreadCountTask;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Notifications = new ObservableCollection<NotificationItem>(notifications);
                    UnreadCount = unreadCount;
                    _notificationStateService.SetUnreadCount(unreadCount);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [NotificationViewModel] LoadFromApiAsync failed: {ex.Message}");
                throw;
            }
        }
    }
}
