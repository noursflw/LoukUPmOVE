using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using loukupm.Model;
using loukupm.services;
using loukupm.Services;
using System.Collections.ObjectModel;

namespace loukupm.ViewModel
{
    /// <summary>
    /// ViewModel for Terms and Conditions page
    /// Handles loading CMS content, state management, and error handling
    /// </summary>
    public partial class TermsAndConditionsViewModel : ObservableObject
    {
        private readonly ApiServices _apiServices;

        /// <summary>
        /// Main CMS data container with blocks and metadata
        /// </summary>
        [ObservableProperty]
        private TermsConditionsData cmsData;

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

        public TermsAndConditionsViewModel()
        {
            _apiServices = new ApiServices();
        }

        /// <summary>
        /// Load Terms and Conditions content from CMS API
        ///// </summary>
        //[RelayCommand]
        //public async Task LoadTermsAndConditions()
        //{
        //    // Skip if already loaded to avoid redundant requests on navigation
        //    if (CmsData != null)
        //    {
        //        System.Diagnostics.Debug.WriteLine("[TermsAndConditionsViewModel] LoadTermsAndConditions skipped - already loaded");
        //        return;
        //    }

        //    try
        //    {
        //        IsLoading = true;
        //        HasError = false;
        //        ErrorMessage = string.Empty;

        //        // Run API call on thread pool to avoid blocking UI
        //        var response = await Task.Run(async () => 
        //            await _apiServices.GetTermsAndConditionsAsync()
        //        );

        //        // Yield control back to UI thread
        //        await Task.Delay(10);

        //        // Check only response.Data != null (remove Success flag dependency)
        //        // The API may have inconsistent Success values, but if Data is present, use it
        //        if (response?.Data != null)
        //        {
        //            CmsData = response.Data;

        //            // Set flow direction based on API response
        //            PageDirection = response.Data.Direction?.ToLower() ?? "ltr";
        //            ContentFlowDirection = PageDirection == "rtl" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        //            Console.WriteLine($"✅ Terms & Conditions loaded successfully");
        //            Console.WriteLine($"   Language: {CmsData.Language}");
        //            Console.WriteLine($"   Direction: {PageDirection}");
        //            Console.WriteLine($"   Blocks: {CmsData.Blocks?.Count ?? 0}");
        //        }
        //        else
        //        {
        //            HasError = true;
        //            ErrorMessage = response?.Message ?? "Failed to load Terms and Conditions";
        //            Console.WriteLine($"❌ Terms & Conditions API returned error: {ErrorMessage}");
        //            Console.WriteLine($"   Response Success: {response?.Success}");
        //            Console.WriteLine($"   Response Data: {(response?.Data == null ? "null" : "not null")}");
        //        }
        //    }
        //    catch (TaskCanceledException ex)
        //    {
        //        HasError = true;
        //        ErrorMessage = "Request timed out. Please check your internet connection.";
        //        Console.WriteLine($"⏱️ Timeout in LoadTermsAndConditions: {ex.Message}");
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        HasError = true;
        //        ErrorMessage = "Network error. Please check your internet connection.";
        //        Console.WriteLine($"🌐 Network error in LoadTermsAndConditions: {ex.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        HasError = true;
        //        ErrorMessage = "An unexpected error occurred while loading Terms and Conditions";
        //        Console.WriteLine($"❌ Exception in LoadTermsAndConditions: {ex.Message}");
        //        Console.WriteLine($"   StackTrace: {ex.StackTrace}");
        //    }
        //    finally
        //    {
        //        IsLoading = false;
        //    }
        //}
        [RelayCommand(AllowConcurrentExecutions = false)]
        public async Task LoadTermsAndConditions()
        {
            // Prevent multiple simultaneous requests
            if (IsLoading)
            {
                Console.WriteLine("⏳ LoadTermsAndConditions skipped - already loading");
                return;
            }

            // Skip if data already exists
            if (CmsData != null)
            {
                Console.WriteLine("📄 LoadTermsAndConditions skipped - already loaded");
                return;
            }

            try
            {
                IsLoading = true;
                HasError = false;
                ErrorMessage = string.Empty;

                Console.WriteLine("📥 Loading Terms & Conditions...");

                // Execute API request
                var response = await _apiServices.GetTermsAndConditionsAsync();

                if (response?.Data != null)
                {
                    CmsData = response.Data;

                    // Set FlowDirection
                    PageDirection = response.Data.Direction?.ToLower() ?? "ltr";
                    ContentFlowDirection =
                        PageDirection == "rtl"
                        ? FlowDirection.RightToLeft
                        : FlowDirection.LeftToRight;

                    Console.WriteLine("✅ Terms & Conditions loaded successfully");
                    Console.WriteLine($"   Language : {CmsData.Language}");
                    Console.WriteLine($"   Direction: {PageDirection}");
                    Console.WriteLine($"   Blocks   : {CmsData.Blocks?.Count ?? 0}");
                }
                else
                {
                    HasError = true;
                    ErrorMessage = response?.Message ?? "Failed to load Terms and Conditions";

                    Console.WriteLine("❌ Terms & Conditions API returned invalid response");
                    Console.WriteLine($"   Success : {response?.Success}");
                    Console.WriteLine($"   Message : {response?.Message}");
                    Console.WriteLine($"   Data    : {(response?.Data == null ? "NULL" : "NOT NULL")}");
                }
            }
            catch (TaskCanceledException ex)
            {
                HasError = true;
                ErrorMessage = "Request timed out. Please check your internet connection.";

                Console.WriteLine($"⏱️ Timeout: {ex.Message}");
            }
            catch (HttpRequestException ex)
            {
                HasError = true;
                ErrorMessage = "Network error. Please check your internet connection.";

                Console.WriteLine($"🌐 Network error: {ex.Message}");
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "An unexpected error occurred while loading Terms and Conditions.";

                Console.WriteLine($"❌ Exception: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        /// <summary>
        /// Retry loading Terms and Conditions
        /// Force reload by clearing cached data first.
        /// </summary>
        [ObservableProperty]
        private bool isRefreshing;
        [RelayCommand(AllowConcurrentExecutions = false)]
        public async Task RetryLoadTermsAndConditions()
        {
            IsRefreshing = true;

            try
            {
                ClearData();
                await LoadTermsAndConditions();
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        /// <summary>
        /// Clear all data and error state
        /// </summary>
        public void ClearData()
        {
            CmsData = null;
            IsLoading = false;
            HasError = false;
            ErrorMessage = string.Empty;
            PageDirection = "ltr";
            ContentFlowDirection = FlowDirection.LeftToRight;
        }
    }
}
