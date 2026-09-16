using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using loukupm.Langue;

namespace loukupm.Behaviors
{
    /// <summary>
    /// Behavior that listens to language changes and forces refresh
    /// for CollectionView and CarouselView (MAUI cached UI fix).
    /// </summary>
    public class LanguageAwareCollectionBehavior : Behavior<Microsoft.Maui.Controls.View>
    {
        private Microsoft.Maui.Controls.View _attachedView;

        protected override void OnAttachedTo(Microsoft.Maui.Controls.View view)
        {
            base.OnAttachedTo(view);

            _attachedView = view;

            // Subscribe to language change events
            LocalizationResourcesManager.Instanse.LanguageChanged += OnLanguageChanged;
            CollectionRefreshService.Instance.CollectionsNeedRefresh += OnCollectionsNeedRefresh;

            System.Diagnostics.Debug.WriteLine(
                $"✅ LanguageAwareCollectionBehavior attached to {view?.GetType().Name}"
            );
        }

        protected override void OnDetachingFrom(Microsoft.Maui.Controls.View view)
        {
            LocalizationResourcesManager.Instanse.LanguageChanged -= OnLanguageChanged;
            CollectionRefreshService.Instance.CollectionsNeedRefresh -= OnCollectionsNeedRefresh;

            _attachedView = null;

            base.OnDetachingFrom(view);

            System.Diagnostics.Debug.WriteLine(
                $"❌ LanguageAwareCollectionBehavior detached from {view?.GetType().Name}"
            );
        }

        /// <summary>
        /// Fired when language changes globally
        /// </summary>
        private void OnLanguageChanged(System.Globalization.CultureInfo culture)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                System.Diagnostics.Debug.WriteLine(
                    $"🌍 Language changed → forcing UI refresh: {culture?.Name}"
                );

                ForceRefresh();
            });
        }

        /// <summary>
        /// Manual refresh trigger (if needed from ViewModel)
        /// </summary>
        private void OnCollectionsNeedRefresh()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                System.Diagnostics.Debug.WriteLine(
                    "📋 Manual collection refresh triggered"
                );

                ForceRefresh();
            });
        }

        /// <summary>
        /// Core refresh logic for MAUI cached views
        /// </summary>
        private void ForceRefresh()
        {
            if (_attachedView == null)
                return;

            try
            {
                // CollectionView / CarouselView workaround
                if (_attachedView is CollectionView collectionView)
                {
                    var items = collectionView.ItemsSource;
                    collectionView.ItemsSource = null;
                    collectionView.ItemsSource = items;
                }
                else if (_attachedView is CarouselView carouselView)
                {
                    var items = carouselView.ItemsSource;
                    carouselView.ItemsSource = null;
                    carouselView.ItemsSource = items;
                }

                System.Diagnostics.Debug.WriteLine("🔄 UI refresh completed");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"❌ Refresh failed: {ex.Message}"
                );
            }
        }
    }
}