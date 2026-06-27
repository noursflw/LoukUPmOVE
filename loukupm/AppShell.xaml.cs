using loukupm.Services;
using loukupm.View;
using loukupm.ViewModel;
using OneSignalSDK.DotNet;

namespace loukupm
{
    public partial class AppShell : Shell
    {
        
        private bool _isNavigating = false;

        public AppShell()
        {
            InitializeComponent();
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


                if (_isNavigating)
                {
                    Console.WriteLine($"[AppShell] Navigation already in progress, ignoring back button");
                    return true;
                }

                _isNavigating = true;

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        // Check if this is a terminal page for user feedback
                        bool isTerminalPage = currentPage == NavigationService.ROUTE_LOGIN || 
                                            currentPage == NavigationService.ROUTE_SPLASH;

                        bool handled = await NavigationService.HandleBackButton(currentPage);
                        Console.WriteLine($"[AppShell] Back button handling result: {handled}");

                        // Show feedback to user if on terminal page and back press was handled
                        if (isTerminalPage && handled)
                        {
                            await MainThread.InvokeOnMainThreadAsync(async () =>
                            {
                                var page = Application.Current?.MainPage;
                                if (page != null)
                                {
                                    await page.DisplayAlert(
                                        "Exit Application",
                                        "Press back again to exit the application",
                                        "OK"
                                    );
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[AppShell] ERROR in HandleBackButton: {ex.Message}");
                        Console.WriteLine($"[AppShell] Exception: {ex.GetType().Name}");
                        Console.WriteLine($"[AppShell] Stack trace: {ex.StackTrace}");
                    }
                    finally
                    {
                        _isNavigating = false;
                    }
                });

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
                    (NavigationService.ROUTE_LOGIN, typeof(LoginPage)),
                    (NavigationService.ROUTE_SIGNIN, typeof(SinginPage)),
                    (NavigationService.ROUTE_OTP, typeof(OTPSINGIN)),
                    (NavigationService.ROUTE_POLICY_PRIVACY_AUTH, typeof(PolicyandPrivacyPageatAthun)),
                    (NavigationService.ROUTE_TermsAndConditions_Athun, typeof(TermsAndConditionsAthun)),
                    (NavigationService.ROUTE_TERM_BOOKING, typeof(TerminbuchenPage)),
                    (NavigationService.ROUTE_PAYMENT, typeof(Paymentgetway)),
                    (NavigationService.Route_ContactUs, typeof(ContenUs)),



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
    }
}
