using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loukupm.Model;
using loukupm.Services;
using loukupm.Langue;
using System.Collections.ObjectModel;

namespace loukupm.ViewModel
{
    public partial class AboutUsViewModel : ObservableObject
    {
        private readonly ApiServices _apiServices;

        [ObservableProperty]
        private AboutUsData aboutUsData;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string currentLanguage = "en";

        [ObservableProperty]
        private string emailLabel = "Email";

        /// <summary>
        /// Localized hero subtitle text property for XAML binding.
        /// Returns the subtitle in the current UI culture language.
        /// </summary>
        public string HeroSubtitle => AboutUsData?.Hero?.Subtitle?.GetText();

        /// <summary>
        /// Localized hero description text property for XAML binding.
        /// Returns the description in the current UI culture language.
        /// </summary>
        public string HeroDescription => AboutUsData?.Hero?.Description?.GetText();

        public AboutUsViewModel()
        {
            _apiServices = new ApiServices();
        }

        [RelayCommand]
        public async Task LoadAboutUsData()
        {
            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = string.Empty;

                var response = await _apiServices.GetAboutUsAsync();

                if (response?.Success == true && response.Data != null)
                {
                    AboutUsData = response.Data;

                    // Notify UI about the new localized text properties
                    OnPropertyChanged(nameof(HeroSubtitle));
                    OnPropertyChanged(nameof(HeroDescription));

                    Console.WriteLine($"✅ AboutUs data loaded successfully");
                }
                else
                {
                    HasError = true;
                    ErrorMessage = response?.Message ?? "Failed to load AboutUs data";
                    Console.WriteLine($"❌ AboutUs data failed: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Error loading data: {ex.Message}";
                Console.WriteLine($"❌ Exception in LoadAboutUsData: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task OpenFacebook()
        {
            try
            {
                await Launcher.Default.OpenAsync(new Uri("https://www.facebook.com/profile.php?id=100093841434497"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error opening Facebook: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task OpenInstagram()
        {
            try
            {
                await Launcher.Default.OpenAsync(new Uri("https://www.instagram.com/lookupfriseur/"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error opening Instagram: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task OpenTikTok()
        {
            try
            {
                await Launcher.Default.OpenAsync(new Uri("https://www.tiktok.com/@lookupfriseur"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error opening TikTok: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task OpenWhatsApp()
        {
            try
            {
                await Launcher.Default.OpenAsync(new Uri("https://wa.me/4917643233977"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error opening WhatsApp: {ex.Message}");
            }
        }

        /// <summary>
        /// Get text from either a MultiLanguageText or a plain string. Returns localized text when MultiLanguageText is supplied.
        /// </summary>
        public string GetText(object value)
        {
            if (value == null) return string.Empty;
            if (value is MultiLanguageText multiText) return multiText.GetText(CurrentLanguage);
            if (value is string s) return s;
            return value.ToString();
        }

        /// <summary>
        /// Update current language and notify UI
        /// </summary>
        public void SetLanguage(string languageCode)
        {
            CurrentLanguage = languageCode;
            OnPropertyChanged(nameof(AboutUsData));
            OnPropertyChanged(nameof(HeroSubtitle));
            OnPropertyChanged(nameof(HeroDescription));
        }

        /// <summary>
        /// Refresh collections when language/culture changes.
        /// This forces CollectionView and CarouselView to re-render all cached items.
        /// Standard MAUI pattern: recreate ObservableCollection instances to trigger full re-render.
        /// </summary>
        public void RefreshCollectionsForLanguageChange()
        {
            if (AboutUsData == null)
            {
                Console.WriteLine("⚠️ AboutUsData is null, skipping collection refresh");
                return;
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    // Refresh Hero Images
                    if (AboutUsData.Hero?.Images != null)
                    {
                        AboutUsData.Hero.Images = CollectionRefreshService.RecreateCollection(AboutUsData.Hero.Images);
                        Console.WriteLine("🔄 Hero.Images collection refreshed");
                    }

                    // Refresh Features collection
                    if (AboutUsData.Features != null)
                    {
                        AboutUsData.Features = CollectionRefreshService.RecreateCollection(AboutUsData.Features);
                        Console.WriteLine("🔄 Features collection refreshed");
                    }

                    // Refresh Team collection
                    if (AboutUsData.Team != null)
                    {
                        AboutUsData.Team = CollectionRefreshService.RecreateCollection(AboutUsData.Team);
                        Console.WriteLine("🔄 Team collection refreshed");
                    }

                    // Refresh Legal Links
                    if (AboutUsData.Legal?.Links != null)
                    {
                        AboutUsData.Legal.Links = CollectionRefreshService.RecreateCollection(AboutUsData.Legal.Links);
                        Console.WriteLine("🔄 Legal.Links collection refreshed");
                    }

                    // Refresh Social Links (if needed)
                    if (AboutUsData.Social?.Links != null)
                    {
                        AboutUsData.Social.Links = CollectionRefreshService.RecreateCollection(AboutUsData.Social.Links);
                        Console.WriteLine("🔄 Social.Links collection refreshed");
                    }

                    Console.WriteLine("✅ All collections refreshed for language change");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error refreshing collections: {ex.Message}");
                }
            });
        }
    }
}
