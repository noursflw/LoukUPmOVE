namespace loukupm.Services;

/// <summary>
/// ÎÏãÉ ÇáÊäŞá ÇáãÑßÒíÉ
/// ÊÏíÑ ÌãíÚ ÚãáíÇÊ ÇáÊäŞá Èíä ÇáÕİÍÇÊ
/// æÊÊÚÇãá ãÚ ÒÑ ÇáÑÌæÚ ÈÔßá ÕÍíÍ
/// </summary>
public static class NavigationService
{
    /// <summary>
    /// ŞÇãæÓ áÊÎÒíä ÕİÍÇÊ "ÇáÑÌæÚ Åáì" ÇáÇİÊÑÇÖíÉ
    /// </summary>
    private static readonly Dictionary<string, string> BackNavigationMap = new()
    {
        // Group 1: Back to LoginPage
        ["SinginPage"] = "LoginPage",
        ["PolicyandPrivacyPage"] = "LoginPage",
        ["RestPassword"] = "LoginPage",
        ["TermsAndConditions"] = "LoginPage",
        
        // Group 2: Back to HomePage
        ["ServicesPage"] = "HomePage",
        ["BookingPage"] = "HomePage",
        ["AboutUS"] = "HomePage",
        
        // Group 3: Back to ProfilePage (default)
        ["EditeUserPage"] = "ProfilePage",
        ["EditePasswordPage"] = "ProfilePage",
    };

    /// <summary>
    /// ŞÇãæÓ áÊÊÈÚ ÇáÕİÍÇÊ ÇáÊí Êã ÇáæÕæá ÅáíåÇ ãä ÕİÍÇÊ ÃÎÑì
    /// ÇáãİÊÇÍ: ÇáÕİÍÉ ÇáÍÇáíÉ¡ ÇáŞíãÉ: ÇáÕİÍÉ ÇáÊí ÌÇÁ ãäåÇ
    /// </summary>
    private static readonly Dictionary<string, string> PageSourceMap = new();

    /// <summary>
    /// ÇáÍÕæá Úáì ÇáÕİÍÉ ÇáÊí íÌÈ ÇáÑÌæÚ ÅáíåÇ
    /// </summary>
    public static string GetBackNavigationRoute(string currentPageName)
    {
        // ÅĞÇ ßÇäÊ ÇáÕİÍÉ ÇáÍÇáíÉ İí ÎÑíØÉ ÇáãÕÏÑ¡ ÇÓÊÎÏã ÇáãÕÏÑ
        if (PageSourceMap.TryGetValue(currentPageName, out var source))
        {
            var backRoute = source;
            // ÇÍĞİ ÇáÏÎæá ÈÚÏ ÇáÇÓÊÎÏÇã
            PageSourceMap.Remove(currentPageName);
            return $"//{backRoute}";
        }

        // æÅáÇ ÇÓÊÎÏã ÇáÎÑíØÉ ÇáÇİÊÑÇÖíÉ
        if (BackNavigationMap.TryGetValue(currentPageName, out var defaultBack))
        {
            return $"//{defaultBack}";
        }

        // ÇáÎíÇÑ ÇáÃÎíÑ: ProfilePage ßÇİÊÑÇÖí
        return "//ProfilePage";
    }

    /// <summary>
    /// ÊÓÌíá Ãä ÇáÕİÍÉ Êã ÇáæÕæá ÅáíåÇ ãä ÕİÍÉ ãÍÏÏÉ
    /// ÇÓÊÎÏã åĞÇ ÚäÏ ÇáÇäÊŞÇá Åáì ÕİÍÉ ŞÏ ÊÍÊÇÌ ÇáÑÌæÚ Åáì ÕİÍÇÊ ãÊÚÏÏÉ
    /// </summary>
    public static void RegisterPageSource(string currentPage, string fromPage)
    {
        if (PageSourceMap.ContainsKey(currentPage))
        {
            PageSourceMap[currentPage] = fromPage;
        }
        else
        {
            PageSourceMap.Add(currentPage, fromPage);
        }

        Console.WriteLine($"? Registered: {currentPage} came from {fromPage}");
    }

    /// <summary>
    /// ÇáÊäŞá Åáì ÕİÍÉ ãÚ ÊÓÌíá ãÕÏÑåÇ
    /// </summary>
    public static async Task NavigateToWithSource(string targetPage, string fromPage)
    {
        RegisterPageSource(targetPage, fromPage);
        await Shell.Current.GoToAsync($"//{targetPage}");
    }

    /// <summary>
    /// ãÚÇáÌÉ ÒÑ ÇáÑÌæÚ - ÊÑÌÚ true ÅĞÇ Êã ãÚÇáÌÉ ÇáÍÏË
    /// </summary>
    public static async Task<bool> HandleBackButton(string currentPageName)
    {
        try
        {
            var backRoute = GetBackNavigationRoute(currentPageName);
            await Shell.Current.GoToAsync(backRoute);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Navigation error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// ãÓÍ ÎÑíØÉ ÇáãÕÇÏÑ (ÇÓÊÎÏãåÇ ÚäÏ ÊÓÌíá ÇáÎÑæÌ)
    /// </summary>
    public static void ClearPageSourceMap()
    {
        PageSourceMap.Clear();
        Console.WriteLine("? Page source map cleared");
    }
}
