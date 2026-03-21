namespace loukupm.Services;

/// <summary>
/// Base class for subpages (pages outside the TabBar).
///
/// Provides two things:
///   1. Automatic system back-button handling that pops exactly one level ("..").
///   2. A helper for in-app back buttons / gestures.
///
/// DO NOT use this base class for TabBar pages (HomePage, ServicesPage, etc.).
/// Back-button handling for TabBar pages is done globally in AppShell.
/// </summary>
public abstract class NavigationAwarePage : ContentPage
{
    /// <summary>
    /// Simple page name used as the route key.
    /// Override in derived classes if the class name differs from the registered route.
    /// </summary>
    protected virtual string PageName => GetType().Name;

    /// <summary>
    /// Intercepts the system/hardware back button on this subpage.
    /// Navigates one step back ("..") via NavigationService.HandleBackButton.
    /// Returns true so the OS does not perform its own default action.
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(PageName);
        });

        return true; // Event handled — suppress the OS default
    }

    /// <summary>
    /// Call this from an in-app back button or swipe gesture.
    /// Equivalent to the system back button: pops one level.
    /// </summary>
    protected async Task GoBack()
    {
        await NavigationService.HandleBackButton(PageName);
    }

    /// <summary>
    /// Navigate forward to another page and push it onto the stack.
    /// </summary>
    protected async Task NavigateTo(string targetRoute)
    {
        await NavigationService.NavigateToPage(targetRoute);
    }
}
