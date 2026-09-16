using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loukupm.Model;
using loukupm.services;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace loukupm.ViewModel
{
    /// <summary>
    /// Notification ViewModel: handles loading notifications and marking them as read.
    /// NOTE: This ViewModel must not hold or expose badge state; NotificationStateService is the single source of truth.
    /// </summary>
    public partial class NotificationViewModel : ObservableObject, IDisposable
    {
        private readonly NotificationService _notificationService;
        private readonly NotificationStateService _notificationStateService;
        private bool _isLoadingRequested;
        private bool _isHandlingSelection;
        private bool _disposed;

        [ObservableProperty]
        private ObservableCollection<NotificationItem> notifications = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isRefreshing;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private NotificationItem? selectedNotification;

        public NotificationViewModel(NotificationService notificationService, NotificationStateService notificationStateService)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _notificationStateService = notificationStateService ?? throw new ArgumentNullException(nameof(notificationStateService));
        }

        partial void OnSelectedNotificationChanged(NotificationItem? value)
        {
            // NOTE: This is no longer used since SelectionMode is now "None" in XAML.
            // Notifications are marked as read and deleted via SwipeView (swipe left).
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

                // After marking as read on server, fetch authoritative unread count and push into NotificationStateService
                var count = await _notificationService.GetUnreadCountAsync();
                _notificationStateService.SetUnreadCount(count);

                // Update local UI (mark item read) on main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    notification.ReadAt = DateTime.UtcNow;
                    // Force UI to reflect change by replacing collection reference
                    Notifications = new ObservableCollection<NotificationItem>(Notifications);
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
            if (!IsRefreshing)
            {
                Console.WriteLine("⏳ Already refreshing, skipping duplicate request");
                return;
            }

            IsRefreshing = true;
            ErrorMessage = string.Empty;

            try
            {
                Console.WriteLine("🔄 Starting refresh of notifications...");
                await LoadFromApiAsync();
                Console.WriteLine("✅ Notifications refreshed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [NotificationViewModel] Refresh failed: {ex.Message}");
                ErrorMessage = "Failed to refresh notifications.";

                // Log full exception for debugging
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
                Console.WriteLine("📥 Loading notifications from API...");

                var notificationsTask = _notificationService.GetNotificationsAsync();
                var unreadCountTask = _notificationService.GetUnreadCountAsync();

                await Task.WhenAll(notificationsTask, unreadCountTask);

                var notifications = await notificationsTask;
                var unreadCount = await unreadCountTask;

                if (notifications == null)
                {
                    Console.WriteLine("⚠️ API returned null notifications list");
                    notifications = new List<NotificationItem>();
                }

                Console.WriteLine($"📊 Loaded {notifications.Count()} notifications, Unread: {unreadCount}");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Notifications = new ObservableCollection<NotificationItem>(notifications);
                    Console.WriteLine("✅ UI updated with new notifications");
                });

                // Push authoritative unread count into the shared state service (no local math)
                _notificationStateService.SetUnreadCount(unreadCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [NotificationViewModel] LoadFromApiAsync failed: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Mark a specific notification as read (used when opening page via notification tap).
        /// Calls the API then refreshes authoritative unread count from server into NotificationStateService.
        /// </summary>
        public async Task MarkNotificationAsReadAsync(string notificationId)
        {
            if (string.IsNullOrWhiteSpace(notificationId))
                return;

            try
            {
                var success = await _notificationService.MarkAsReadAsync(notificationId);
                if (!success)
                {
                    Console.WriteLine($"🔴 MarkNotificationAsReadAsync failed for {notificationId}");
                    return;
                }

                // Fetch authoritative unread count and push into shared state service
                var count = await _notificationService.GetUnreadCountAsync();
                _notificationStateService.SetUnreadCount(count);

                // Update local list item as read if present
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var item = Notifications.FirstOrDefault(n => n.Id == notificationId);
                    if (item != null)
                    {
                        item.ReadAt = DateTime.UtcNow;
                        Notifications = new ObservableCollection<NotificationItem>(Notifications);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ MarkNotificationAsReadAsync error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete notification by ID only (swipe right - no mark as read).
        /// </summary>
        [RelayCommand]
        private async Task DeleteNotificationAsync(NotificationItem? notification)
        {
            if (notification == null || string.IsNullOrWhiteSpace(notification.Id))
                return;

            try
            {
                Console.WriteLine($"🗑️ Deleting notification (right swipe): {notification.Id}");

                // Remove from local collection on main thread (no need to mark as read)
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var item = Notifications.FirstOrDefault(n => n.Id == notification.Id);
                    if (item != null)
                    {
                        Notifications.Remove(item);
                        Console.WriteLine($"✅ Notification deleted from local list: {notification.Id}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DeleteNotificationAsync error: {ex.Message}");
            }
        }

        /// <summary>
        /// Mark notification as read and delete it (swipe left).
        /// </summary>
        [RelayCommand]
        private async Task MarkAsReadAndDeleteAsync(NotificationItem? notification)
        {
            if (notification == null || string.IsNullOrWhiteSpace(notification.Id))
                return;

            try
            {
                Console.WriteLine($"✉️ Mark as read & delete notification (left swipe): {notification.Id}");

                // Mark as read on server
                var success = await _notificationService.MarkAsReadAsync(notification.Id);
                if (!success)
                {
                    Console.WriteLine($"🔴 Failed to mark notification as read: {notification.Id}");
                    return;
                }

                // Remove from local collection on main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var item = Notifications.FirstOrDefault(n => n.Id == notification.Id);
                    if (item != null)
                    {
                        item.ReadAt = DateTime.UtcNow;
                        Notifications.Remove(item);
                        Console.WriteLine($"✅ Notification marked as read and deleted: {notification.Id}");
                    }
                });

                // Refresh unread count
                var count = await _notificationService.GetUnreadCountAsync();
                _notificationStateService.SetUnreadCount(count);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ MarkAsReadAndDeleteAsync error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            // No persistent subscriptions held by this VM to the state service; if added, unsubscribe here.
            _disposed = true;
        }
    }
}
