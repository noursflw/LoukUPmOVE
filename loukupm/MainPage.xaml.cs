using loukupm.View;
using loukupm.Services;
namespace loukupm
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.InitializeLanguageTracking();
            Shell.SetNavBarIsVisible(this, false);
        }

        private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            await ShellNavigationManager.NavigateToLoginAndClear();
        }
        protected override bool OnBackButtonPressed()
        {
            // Ensure centralized handling
            ShellNavigationManager.LogNavigationState();
            return true;
        }
    }
}