using System.Collections.ObjectModel;

namespace loukupm.Langue
{
    /// <summary>
    /// Service to manage and coordinate collection refresh when app language/culture changes.
    /// 
    /// Why this is needed:
    /// CollectionView and CarouselView in MAUI cache their DataTemplate cells for performance.
    /// When language changes, the converter is called, but MAUI doesn't re-render cached cells.
    /// This service coordinates forcing a full collection refresh by recreating ObservableCollection instances.
    /// </summary>
    public class CollectionRefreshService
    {
        private static CollectionRefreshService _instance;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static CollectionRefreshService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new CollectionRefreshService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Event fired when collections should refresh their items due to language/culture change
        /// </summary>
        public event Action? CollectionsNeedRefresh;

        /// <summary>
        /// Trigger collection refresh across the app
        /// Call this when language/culture changes to force CollectionView/CarouselView to re-render items
        /// </summary>
        public void TriggerCollectionRefresh()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CollectionsNeedRefresh?.Invoke();
                Console.WriteLine($"🔄 CollectionRefreshService: Collections refresh triggered");
            });
        }

        /// <summary>
        /// Recreates an ObservableCollection to force UI re-render.
        /// This is the standard MAUI pattern for forcing CollectionView/CarouselView refresh.
        /// </summary>
        /// <typeparam name="T">Item type in collection</typeparam>
        /// <param name="originalCollection">Source collection with items</param>
        /// <returns>New ObservableCollection with same items</returns>
        public static ObservableCollection<T> RecreateCollection<T>(ObservableCollection<T> originalCollection)
        {
            if (originalCollection == null || originalCollection.Count == 0)
                return new ObservableCollection<T>();

            // Create new instance with same items - forces MAUI to re-render all cells
            return new ObservableCollection<T>(originalCollection);
        }

        /// <summary>
        /// Safely recreates a collection and reassigns it.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="currentCollection">Current collection</param>
        /// <returns>New recreated collection or empty if input is null</returns>
        public static ObservableCollection<T> RefreshCollection<T>(ObservableCollection<T> currentCollection)
        {
            return RecreateCollection(currentCollection);
        }
    }
}
