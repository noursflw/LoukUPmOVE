using System;
using System.ComponentModel;
using System.Threading;
using Microsoft.Maui.Dispatching;

namespace loukupm.services
{
    /// <summary>
    /// Singleton UI state for notifications. This is the single source of truth for badge state.
    /// </summary>
    public sealed class NotificationStateService : INotifyPropertyChanged
    {
        private readonly object _lock = new object();
        private int _unreadCount;

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<int> UnreadCountChanged;

        public NotificationStateService() { }

        /// <summary>
        /// Read-only UnreadCount. Thread-safe read.
        /// </summary>
        public int UnreadCount
        {
            get
            {
                lock (_lock)
                {
                    return _unreadCount;
                }
            }
            private set
            {
                bool changed = false;
                lock (_lock)
                {
                    if (_unreadCount != value)
                    {
                        _unreadCount = value;
                        changed = true;
                    }
                }

                if (!changed)
                    return;

                // Notify on main thread for UI safety
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnreadCount)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasUnread)));
                    UnreadCountChanged?.Invoke(_unreadCount);
                });
            }
        }

        /// <summary>
        /// Convenience boolean for binding. Always raises PropertyChanged when UnreadCount changes.
        /// </summary>
        public bool HasUnread => UnreadCount > 0;

        /// <summary>
        /// Set the unread count. This is the ONLY mutator for unread state.
        /// Marshals to the MainThread and is thread-safe.
        /// </summary>
        /// <param name="count">non-negative unread count from backend</param>
        public void SetUnreadCount(int count)
        {
            if (count < 0) count = 0;

            // Always update on main thread to keep UI consumers safe.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Use the property setter to get consistent notifications
                UnreadCount = count;
            });
        }
    }
}
