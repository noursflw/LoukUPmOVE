using CommunityToolkit.Maui.Views;
using loukupm.Services;
using loukupm.View;
using loukupm.ViewModel;
using OneSignalSDK.DotNet;

namespace loukupm
{
    public partial class AppShell : Shell
    {
        
        public AppShell()
        {
            InitializeComponent();
            ShowHomeShell();
            RegisterAllRoutes();
            ValidateNavigation();
            SetupNotificationTapHandler();
            this.BindingContext = AppViewModel.Instance;
        }

        
        private void SetupNotificationTapHandler()
        {
            try
            {
                
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Task.Delay(500);
                    Console.WriteLine("✅ [AppShell] OneSignal notification tap handler ready for foreground/background");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ [AppShell] Error setting up notification handler: {ex.Message}");
            }
        }

       
        protected override bool OnBackButtonPressed()
        {
            try
            {
                var currentPage = NavigationService.GetCurrentPageName();
                Console.WriteLine($"[AppShell] OnBackButtonPressed triggered from page: {currentPage}");
                _ = NavigationService.HandleBackButton(currentPage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppShell] CRITICAL ERROR in OnBackButtonPressed: {ex.Message}");
                Console.WriteLine($"[AppShell] Stack trace: {ex.StackTrace}");
                return true;
            }
        }

       

        private void RegisterAllRoutes()
        {
            try
            {
              
                var routesToRegister = new[]
                {

                    (NavigationService.ROUTE_MAIN_PAGE, typeof(MainPage)),
                    (NavigationService.ROUTE_SIGNIN, typeof(SinginPage)),
                    (NavigationService.ROUTE_OTP, typeof(OTPSINGIN)),
                    (NavigationService.ROUTE_POLICY_PRIVACY_AUTH, typeof(PolicyandPrivacyPageatAthun)),
                    (NavigationService.ROUTE_TermsAndConditions_Athun, typeof(TermsAndConditionsAthun)),
                    (NavigationService.ROUTE_TERM_BOOKING, typeof(TerminbuchenPage)),
                    (NavigationService.ROUTE_PAYMENT, typeof(Paymentgetway)),
                    (NavigationService.Route_ContactUs, typeof(ContenUs)),
                    (NavigationService.ROUTE_Delet_Acount, typeof(Areyousuredeletyouraccountpage)),



                    (NavigationService.ROUTE_POLICY_PRIVACY, typeof(PolicyandPrivacyPage)),
                    (NavigationService.ROUTE_REST_PASSWORD, typeof(RestPassword)),
                    (NavigationService.ROUTE_TERMS_CONDITIONS, typeof(TermsAndConditions)),

                   
                    (NavigationService.ROUTE_EDIT_USER, typeof(EditeUserPage)),
                    (NavigationService.ROUTE_EDIT_PASSWORD, typeof(EditePasswordPage)),
                    (NavigationService.ROUTE_EDIT_PASSWORD_VERIFICATION, typeof(EditPasswordVerification)),
                    (NavigationService.ROUTE_CHACKOUT, typeof(ChackoutPage)),
                    (NavigationService.ROUTE_ABOUT_US, typeof(AboutUS)),
                    (NavigationService.ROUTE_NOTIFICATION, typeof(NotifictionPage)),
                    (NavigationService.ROUTE_SETTING, typeof(SettingPage)),
                    (NavigationService.ROUTE_OTP_PHONE_NUMBER,typeof(OTPPoneNumper)),

                   
                    (NavigationService.ROUTE_HOME, typeof(HomePage)),
                    (NavigationService.ROUTE_SERVICES, typeof(ServicesPage)),
                    (NavigationService.ROUTE_BOOKING, typeof(BookingPage)),
                    (NavigationService.ROUTE_PROFILE, typeof(ProfilePage)),
                };

                int successCount = 0;
                foreach (var (route, pageType) in routesToRegister)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(route))
                        {
                            Console.WriteLine($"⚠️ [AppShell] Route key is null or empty for type {pageType.Name}");
                            continue;
                        }

                        Routing.RegisterRoute(route, pageType);
                        successCount++;
                        Console.WriteLine($"✅ Registered route: {route} → {pageType.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to register route {route} ({pageType.Name}): {ex.Message}");
                    }
                }

                Console.WriteLine($"[AppShell] Route registration complete: {successCount}/{routesToRegister.Length} successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppShell] CRITICAL ERROR in RegisterAllRoutes: {ex.Message}");
                throw; 
            }
        }

        private void ValidateNavigation()
        {
            try
            {
                NavigationService.ValidateRoutes();
                Console.WriteLine("[AppShell] Navigation validation passed ✅");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppShell] Navigation validation error: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
            }
        }
        public static void ResetAuthenticationCheck()
        {
            Console.WriteLine("🔄 ResetAuthenticationCheck called (no-op in new flow)");
        }

        public Task GoToLoginAsync()
        {
            ShowLoginShell();
            return Task.CompletedTask;
        }

        public async Task GoToHomeAsync()
        {
            ShowHomeShell();

            if (CurrentPage is HomePage)
                return;

            try
            {
                await GoToAsync($"//{NavigationService.ROUTE_HOME}", animate: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppShell] GoToHomeAsync route fallback: {ex.Message}");
            }

            ShowHomeShell();
        }

        public async Task ShowProfileAsync()
        {
            Console.WriteLine("[AppShell] ShowProfileAsync");

            // Absolute navigation away from the current tab clears the pushed OTP page.
            // Going straight to ProfilePage is ignored while that page is already the active tab.
            await GoToAsync($"//{NavigationService.ROUTE_HOME}", animate: false);
            await GoToAsync($"//{NavigationService.ROUTE_PROFILE}", animate: false);

            FlyoutBehavior = FlyoutBehavior.Flyout;
            FlyoutIsPresented = false;

            if (MainTabBar == null)
                return;

            CurrentItem = MainTabBar;
            var profile = MainTabBar.Items.FirstOrDefault(item =>
                string.Equals(item.Route, NavigationService.ROUTE_PROFILE, StringComparison.OrdinalIgnoreCase));
            if (profile != null)
                MainTabBar.CurrentItem = profile;

            Console.WriteLine($"[AppShell] Profile visible. Current page: {CurrentPage?.GetType().Name}");
        }

        private void ShowHomeShell()
        {
            try
            {
                FlyoutBehavior = FlyoutBehavior.Flyout;
                FlyoutIsPresented = false;

                if (MainTabBar != null)
                {
                    CurrentItem = MainTabBar;

                    var home = MainTabBar.Items.FirstOrDefault(IsHomeSection)
                        ?? MainTabBar.Items.FirstOrDefault();

                    if (home != null)
                    {
                        MainTabBar.CurrentItem = home;
                        if (home.Items.Count > 0)
                            home.CurrentItem = home.Items[0];
                    }
                }

                if (LoginShellItem != null)
                    LoginShellItem.IsVisible = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppShell] ShowHomeShell failed: {ex.Message}");
            }
        }

        private static bool IsHomeSection(ShellSection section)
        {
            if (section == null)
                return false;

            if (string.Equals(section.Route, NavigationService.ROUTE_HOME, StringComparison.OrdinalIgnoreCase))
                return true;

            return section.Items.Any(content =>
                string.Equals(content.Route, NavigationService.ROUTE_HOME, StringComparison.OrdinalIgnoreCase));
        }

        private void ShowLoginShell()
        {
            try
            {
                FlyoutIsPresented = false;
                FlyoutBehavior = FlyoutBehavior.Disabled;

                if (LoginShellItem == null)
                    return;

                LoginShellItem.IsVisible = true;
                CurrentItem = LoginShellItem;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AppShell] ShowLoginShell failed: {ex.Message}");
            }
        }

        private async void MenuItem_Clicked(object sender, EventArgs e)
        {
            var popup = new MassegBoxLogout();
            var result = await this.ShowPopupAsync(popup);
            if (result is not bool confirmed || !confirmed)
                return;

            FlyoutIsPresented = false;
            await AppViewModel.Instance.LogoutCommand.ExecuteAsync(null);
        }
    }
}
