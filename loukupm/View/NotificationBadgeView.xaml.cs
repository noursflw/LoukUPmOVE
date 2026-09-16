using loukupm.services;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace loukupm.View
{
    public partial class NotificationBadgeView : ContentView
    {
        private NotificationStateService _notificationStateService;

        // Primary constructor - DI will inject the singleton service when view is resolved from DI
        public NotificationBadgeView(NotificationStateService notificationStateService)
        {
            InitializeComponent();
            _notificationStateService = notificationStateService ?? throw new ArgumentNullException(nameof(notificationStateService));
            BindingContext = _notificationStateService; // bind directly to the service (single source of truth)
        }

        // Parameterless ctor for XAML fallback - resolve when handler is available
        public NotificationBadgeView()
        {
            InitializeComponent();

            // Try immediate resolution (may fail during XAML inflate). If unavailable, wait for handler to be set.
            var svc = TryResolveService();
            if (svc != null)
            {
                _notificationStateService = svc;
                BindingContext = _notificationStateService;
            }
            else
            {
                this.HandlerChanged += NotificationBadgeView_HandlerChanged;
            }
        }

        private void NotificationBadgeView_HandlerChanged(object sender, EventArgs e)
        {
            this.HandlerChanged -= NotificationBadgeView_HandlerChanged;
            var svc = TryResolveService();
            if (svc != null)
            {
                _notificationStateService = svc;
                BindingContext = _notificationStateService;
            }
        }

        private static NotificationStateService TryResolveService()
        {
            try
            {
                var mauiContext = Application.Current?.Handler?.MauiContext;
                var svc = mauiContext?.Services.GetService(typeof(NotificationStateService)) as NotificationStateService;
                if (svc != null) return svc;
            }
            catch { }
            return null;
        }

        private async void OnIconClicked(object sender, EventArgs e)
        {
            await HandleNavigationAsync();
        }

        private async Task HandleNavigationAsync()
        {
            try
            {
                // Prefer NavigationService route if available
                await loukupm.Services.NavigationService.NavigateToPage(loukupm.Services.NavigationService.ROUTE_NOTIFICATION);
            }
            catch
            {
                try
                {
                    // Fallback: push notification page directly if route navigation fails
                    await Application.Current?.MainPage?.Navigation?.PushAsync(new NotifictionPage());
                }
                catch
                {
                    // Swallow - navigation failures should not crash the app
                }
            }
        }
    }
}
