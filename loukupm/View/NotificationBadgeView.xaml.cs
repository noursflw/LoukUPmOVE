using loukupm.services;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace loukupm.View
{
    public partial class NotificationBadgeView : ContentView
    {
        private readonly NotificationStateService _notificationStateService;

        // Primary constructor - DI will inject the singleton service when view is resolved from DI
        public NotificationBadgeView(NotificationStateService notificationStateService)
        {
            InitializeComponent();
            _notificationStateService = notificationStateService ?? throw new ArgumentNullException(nameof(notificationStateService));
            BindingContext = _notificationStateService; // bind directly to the service (single source of truth)
        }

        // Parameterless ctor for XAML fallback - resolve from MAUI service provider if possible
        public NotificationBadgeView() : this(ResolveService()) { }

        private static NotificationStateService ResolveService()
        {
            try
            {
                var mauiContext = Application.Current?.Handler?.MauiContext;
                var svc = mauiContext?.Services.GetService(typeof(NotificationStateService)) as NotificationStateService;
                if (svc != null) return svc;
            }
            catch { }

            // As a last resort, create a local instance (should not happen when DI is configured)
            return new NotificationStateService();
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
