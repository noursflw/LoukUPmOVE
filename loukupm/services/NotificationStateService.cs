using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;

namespace loukupm.services
{
    public class NotificationStateService : INotifyPropertyChanged
    {
        private int _unreadCount;

        public int UnreadCount
        {
            get => _unreadCount;
            private set
            {
                if (_unreadCount == value) return;
                _unreadCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnreadCount)));
                UnreadCountChanged?.Invoke(_unreadCount);
            }
        }

        // Event for subscribers (ViewModels) to react immediately
        public event Action<int> UnreadCountChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public NotificationStateService() { }

        public void SetUnreadCount(int count)
        {
            // Ensure main thread for UI consumers
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UnreadCount = count;
            });
        }

        public void Increment() => SetUnreadCount(UnreadCount + 1);
        public void Decrement() => SetUnreadCount(Math.Max(0, UnreadCount - 1));
    }
}
