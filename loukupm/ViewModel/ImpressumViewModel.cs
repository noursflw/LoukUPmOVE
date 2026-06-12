using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loukupm.Model;
using loukupm.Services;
using System.Collections.ObjectModel;

namespace loukupm.ViewModel
{
    /// <summary>
    /// ViewModel for Impressum (Legal Information) page
    /// Handles loading CMS content, state management, and error handling
    /// </summary>
    public partial class ImpressumViewModel : ObservableObject
    {
        private readonly ApiServices _apiServices;

        /// <summary>
        /// Main CMS data container with blocks and metadata
        /// </summary>
        [ObservableProperty]
        private ImpressumData cmsData;

        /// <summary>
        /// Loading state indicator
        /// </summary>
        [ObservableProperty]
        private bool isLoading = false;

        /// <summary>
        /// Error state indicator
        /// </summary>
        [ObservableProperty]
        private bool hasError = false;

        /// <summary>
        /// Error message for user display
        /// </summary>
        [ObservableProperty]
        private string errorMessage = string.Empty;

        /// <summary>
        /// RTL/LTR direction (rtl or ltr)
        /// </summary>
        [ObservableProperty]
        private string pageDirection = "ltr";

        /// <summary>
        /// Computed FlowDirection based on API direction
        /// </summary>
        [ObservableProperty]
        private FlowDirection contentFlowDirection = FlowDirection.LeftToRight;

        public ImpressumViewModel()
        {
            _apiServices = new ApiServices();
        }

        /// <summary>
        /// Load Impressum content from CMS API
        /// </summary>
        [RelayCommand]
        public async Task LoadImpressum()
        {
            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = string.Empty;

                var response = await _apiServices.GetImpressumAsync();

                // Check only response.Data != null (remove Success flag dependency)
                // The API may have inconsistent Success values, but if Data is present, use it
                if (response?.Data != null)
                {
                    CmsData = response.Data;

                    // Set flow direction based on API response
                    PageDirection = response.Data.Direction?.ToLower() ?? "ltr";
                    ContentFlowDirection = PageDirection == "rtl" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

                    Console.WriteLine($"✅ Impressum loaded successfully");
                    Console.WriteLine($"   Language: {CmsData.Language}");
                    Console.WriteLine($"   Direction: {PageDirection}");
                    Console.WriteLine($"   Blocks: {CmsData.Blocks?.Count ?? 0}");
                }
                else
                {
                    HasError = true;
                    ErrorMessage = response?.Message ?? "Failed to load Impressum";
                    Console.WriteLine($"❌ Impressum API returned error: {ErrorMessage}");
                    Console.WriteLine($"   Response Success: {response?.Success}");
                    Console.WriteLine($"   Response Data: {(response?.Data == null ? "null" : "not null")}");
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "An unexpected error occurred while loading Impressum";
                Console.WriteLine($"❌ Exception in LoadImpressum: {ex.Message}");
                Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Retry loading Impressum content after an error
        /// </summary>
        [RelayCommand]
        public async Task RetryLoadImpressum()
        {
            Console.WriteLine($"🔄 Retrying Impressum load...");
            await LoadImpressumCommand.ExecuteAsync(null);
        }
    }
}
