using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loukupm.Model;
using loukupm.Services;
using System.Collections.ObjectModel;

namespace loukupm.ViewModel
{
    /// <summary>
    /// ViewModel for managing Home Slider data
    /// Handles loading, caching, and language changes
    /// </summary>
    public partial class HomeSliderViewModel : ObservableObject
    {
        private readonly ApiServices _apiServices;

        [ObservableProperty]
        private ObservableCollection<HomeSliderItem> items = new();

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string currentLanguage = "en";

        public HomeSliderViewModel()
        {
            _apiServices = new ApiServices();
        }

        /// <summary>
        /// Load home slider data from API
        /// </summary>
        [RelayCommand]
        public async Task LoadSlidersAsync()
        {
            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = string.Empty;

                var response = await _apiServices.GetHomeSlidersAsync();

                if (response?.Success == true && response.Data?.Items != null)
                {
                    // Update collections on main thread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Items.Clear();

                        // Sort by sort_order
                        var sortedSliders = response.Data.Items
                            .OrderBy(s => s.SortOrder)
                            .ToList();

                        foreach (var slider in sortedSliders)
                        {
                            Items.Add(slider);
                        }

                        Console.WriteLine($"✅ Home Sliders loaded successfully: {Items.Count} items");
                    });
                }
                else
                {
                    HasError = true;
                    ErrorMessage = response?.Message ?? "Failed to load Home Sliders";
                    Console.WriteLine($"❌ Home Sliders loading failed: {ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Error loading sliders: {ex.Message}";
                Console.WriteLine($"❌ Exception in LoadSlidersAsync: {ex}");
            }
            finally
            {
                // Ensure IsLoading is set on main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    IsLoading = false;
                });
            }
        }

        /// <summary>
        /// Update current language and notify UI
        /// </summary>
        public void SetLanguage(string languageCode)
        {
            CurrentLanguage = languageCode;
            OnPropertyChanged(nameof(Items));
        }
    }
}
