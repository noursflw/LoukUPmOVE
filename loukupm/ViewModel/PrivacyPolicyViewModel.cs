using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loukupm.Model;
using loukupm.services;
using loukupm.Services;
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
                await Task.Yield(); 

                HasError = false;
                ErrorMessage = string.Empty;

                var response = await _apiServices.GetPrivacyPolicyAsync();

                if (response?.Data != null)
                {
                    CmsData = response.Data;

                    PageDirection = response.Data.Direction?.ToLower() ?? "ltr";
                    ContentFlowDirection = PageDirection == "rtl"
                        ? FlowDirection.RightToLeft
                        : FlowDirection.LeftToRight;
                }
                else
                {
                    HasError = true;
                    ErrorMessage = response?.Message ?? "Failed to load Privacy Policy";
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "An unexpected error occurred";
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
