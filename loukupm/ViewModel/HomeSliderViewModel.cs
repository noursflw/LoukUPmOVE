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

        [ObservableProperty]
        private int currentIndex;

        [ObservableProperty]
        private ObservableCollection<SliderDot> dots = new();

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
                await MainThread.InvokeOnMainThreadAsync(() => IsLoading = true);
                HasError = false;
                ErrorMessage = string.Empty;

                var response = await _apiServices.GetHomeSlidersAsync();

                if (response?.Success == true && response.Data?.Items != null)
                {
                    var sortedSliders = response.Data.Items
                        .OrderBy(s => s.SortOrder)
                        .ToList();
                    var fresh = new ObservableCollection<HomeSliderItem>(sortedSliders);

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Items = fresh;
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
                await MainThread.InvokeOnMainThreadAsync(() => IsLoading = false);
            }
        }

        partial void OnItemsChanged(ObservableCollection<HomeSliderItem> value)
        {
            CurrentIndex = 0;
            SyncDots();
        }

        public void SetCurrentIndex(int index)
        {
            var count = Items?.Count ?? 0;
            if (count == 0)
            {
                CurrentIndex = 0;
                SyncDots();
                return;
            }

            CurrentIndex = Math.Clamp(index, 0, count - 1);
            SyncDots();
        }

        void SyncDots()
        {
            var count = Items?.Count ?? 0;

            while (Dots.Count > count)
                Dots.RemoveAt(Dots.Count - 1);

            while (Dots.Count < count)
                Dots.Add(new SliderDot());

            for (var i = 0; i < Dots.Count; i++)
                Dots[i].IsActive = i == CurrentIndex;
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
