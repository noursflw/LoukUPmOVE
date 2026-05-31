using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loukupm.Model;
using loukupm.services;
using System.Collections.ObjectModel;

namespace loukupm.ViewModel
{
    /// <summary>
    /// ViewModel for Privacy Policy page
    /// Handles loading CMS content, state management, and error handling
    /// </summary>
    public partial class PrivacyPolicyViewModel : ObservableObject
    {
        private readonly ApiServices _apiServices;

        /// <summary>
        /// Main CMS data container with blocks and metadata
        /// </summary>
        [ObservableProperty]
        private PrivacyPolicyData cmsData;

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

        public PrivacyPolicyViewModel()
        {
            _apiServices = new ApiServices();
        }

        /// <summary>
        /// Load Privacy Policy content from CMS API
        /// </summary>
        [RelayCommand]
        public async Task LoadPrivacyPolicy()
        {
            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = string.Empty;

                var response = await _apiServices.GetPrivacyPolicyAsync();

                // Check only response.Data != null (remove Success flag dependency)
                // The API may have inconsistent Success values, but if Data is present, use it
                if (response?.Data != null)
                {
                    CmsData = response.Data;

                    // Set flow direction based on API response
                    PageDirection = response.Data.Direction?.ToLower() ?? "ltr";
                    ContentFlowDirection = PageDirection == "rtl" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

                    Console.WriteLine($"✅ Privacy Policy loaded successfully");
                    Console.WriteLine($"   Language: {CmsData.Language}");
                    Console.WriteLine($"   Direction: {PageDirection}");
                    Console.WriteLine($"   Blocks: {CmsData.Blocks?.Count ?? 0}");
                }
                else
                {
                    HasError = true;
                    ErrorMessage = response?.Message ?? "Failed to load Privacy Policy";
                    Console.WriteLine($"❌ Privacy Policy API returned error: {ErrorMessage}");
                    Console.WriteLine($"   Response Success: {response?.Success}");
                    Console.WriteLine($"   Response Data: {(response?.Data == null ? "null" : "not null")}");
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "An unexpected error occurred while loading Privacy Policy";
                Console.WriteLine($"❌ Exception in LoadPrivacyPolicy: {ex.Message}");
                Console.WriteLine($"   Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Retry loading Privacy Policy content after an error
        /// </summary>
        [RelayCommand]
        public async Task RetryLoadPrivacyPolicy()
        {
            Console.WriteLine($"🔄 Retrying Privacy Policy load...");
            await LoadPrivacyPolicyCommand.ExecuteAsync(null);
        }
    }
}
