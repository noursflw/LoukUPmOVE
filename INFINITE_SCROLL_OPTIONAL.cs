// Optional: Add this to AppViewModel.cs for infinite scroll support
// This is an advanced feature that auto-loads more notifications when user scrolls near bottom

using CommunityToolkit.Mvvm.Input;

namespace loukupm.ViewModel
{
    public partial class AppViewModel : ObservableObject
    {
        // Add this RelayCommand for infinite scroll
        [RelayCommand]
        public async Task LoadMoreNotificationsCommand()
        {
            await LoadMoreNotificationsAsync();
        }

        // Alternative: If you want to track scroll position
        [ObservableProperty]
        private bool isLoadingMoreNotifications = false;

        public async Task OnNotificationsRemainingItemsThresholdReached()
        {
            if (IsLoadingMoreNotifications || !HasMoreNotifications)
                return;

            IsLoadingMoreNotifications = true;
            try
            {
                await LoadMoreNotificationsAsync();
            }
            finally
            {
                IsLoadingMoreNotifications = false;
            }
        }
    }
}

// ============================================
// UPDATE XAML FOR INFINITE SCROLL
// ============================================
// Replace the CollectionView in NotifictionPage.xaml with:

/*
<CollectionView ItemsSource="{Binding Notifications}"
               RemainingItemsThreshold="3"
               RemainingItemsThresholdReachedCommand="{Binding LoadMoreNotificationsCommand}"
               IsVisible="{Binding Notifications.Count}">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <!-- Card layout as before -->
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>

<!-- Loading indicator at bottom when fetching more -->
<ActivityIndicator IsRunning="{Binding IsLoadingMoreNotifications}" 
                  IsVisible="{Binding IsLoadingMoreNotifications}"
                  Color="White" Margin="0,10" />
*/

// ============================================
// EXPLAINING THE CHANGES:
// ============================================
// 
// RemainingItemsThreshold="3"
//   - When user scrolls to 3 items from bottom, trigger load
//   - Lower value = trigger sooner (more responsive)
//   - Higher value = user scrolls further (more data at once)
//
// RemainingItemsThresholdReachedCommand
//   - Executed when threshold is reached
//   - Linked to LoadMoreNotificationsCommand in ViewModel
//   - Automatically appends to collection
//
// ActivityIndicator
//   - Shows loading animation at bottom of list
//   - IsRunning="{Binding IsLoadingMoreNotifications}"
//   - User knows more data is being fetched
