namespace loukupm.Services;

/// <summary>
/// Base class for handling back button navigation in content pages
/// íæİÑ äãØ ãæÍÏ áãÚÇáÌÉ ÒÑ ÇáÑÌæÚ İí ÌãíÚ ÇáÕİÍÇÊ
/// </summary>
public abstract class NavigationAwarePage : ContentPage
{
    /// <summary>
    /// ÇáÍÕæá Úáì ÇÓã ÇáÕİÍÉ ÇáÍÇáíÉ
    /// </summary>
    protected virtual string PageName => this.GetType().Name;

    /// <summary>
    /// ãÚÇáÌÉ ÒÑ ÇáÑÌæÚ - íÌÈ Ãä íÊã ÇÓÊÏÚÇÄå ãä OnBackButtonPressed
    /// </summary>
    protected async Task<bool> HandleBackNavigation()
    {
        return await NavigationService.HandleBackButton(PageName);
    }

    /// <summary>
    /// ÇáÊäŞá ãÚ ÊÓÌíá ÇáãÕÏÑ - ááÕİÍÇÊ ÇáÊí ŞÏ ÊõÒÇÑ ãä ÕİÍÇÊ ãÊÚÏÏÉ
    /// </summary>
    protected async Task NavigateToWithSource(string targetPage)
    {
        await NavigationService.NavigateToWithSource(targetPage, PageName);
    }
}
