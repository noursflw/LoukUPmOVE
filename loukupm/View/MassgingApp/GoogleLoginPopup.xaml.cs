using CommunityToolkit.Maui.Views;
using Firebase.Auth;
using Firebase.Auth.Providers;

namespace loukupm.View.MassgingApp;

public partial class GoogleLoginPopup : Popup
{
    public GoogleLoginPopup()
    {
        InitializeComponent();

        //  ‘€Ì· «·ﬂÊœ »⁄œ ŸÂÊ— «·‹ Popup
        this.Dispatcher.Dispatch(async () =>
        {
            try
            {
                var provider = MauiProgram.firebaseconfig.Providers[0].ProviderType;

                //  ”ÃÌ· «·œŒÊ· »«” Œœ«„ Redirect
                var userCredential = await MauiProgram.firebaseclient.SignInWithRedirectAsync(provider, async uri =>
                {
                    // uri ÂÊ —«»ÿ  ”ÃÌ· «·œŒÊ· „‰ Firebase° ‰⁄—÷Â ›Ì WebView
                    SetUrl(uri);

                    // ‰‰ Ÿ— «·—«»ÿ «·‰Â«∆Ì »⁄œ  ”ÃÌ· «·œŒÊ·
                    string finalUrl = await WaitForNavigationToUrlAsync(LoginWebView,
                        "https://test-23def.web.app/__/auth/handler");

                    return finalUrl;
                });

                // ≈€·«ﬁ Popup »⁄œ  ”ÃÌ· «·œŒÊ·
                this.Close();

                // ⁄—÷ —”«·…  —ÕÌ» »«·„” Œœ„
                if (userCredential != null)
                {
                    var user = userCredential.User;
                    await Application.Current.MainPage.DisplayAlert("Sign In", "Welcome: " + user.Info.DisplayName, "OK");
                    LoginPage.IsLogged = true;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                this.Close();
            }
        });
    }

    // ‰Ã⁄· WebView ⁄«„… ··Ê’Ê· ·Â« Œ«—ÃÌ« ≈–« «Õ Ã 
    public WebView LoginWebView => GoogleWebView;

    //  ⁄ÌÌ‰ —«»ÿ ··‹ WebView
    public void SetUrl(string url)
    {
        GoogleWebView.Source = url;
    }

    // œ«·… «‰ Ÿ«— Õ Ï Ì’· WebView ··—«»ÿ «·‰Â«∆Ì
    private async Task<string> WaitForNavigationToUrlAsync(WebView webView, string targetUrl)
    {
        var tcs = new TaskCompletionSource<string>();

        void Handler(object s, WebNavigatedEventArgs e)
        {
            if (e.Url.StartsWith(targetUrl))
            {
                tcs.TrySetResult(e.Url);
                webView.Navigated -= Handler; // ≈“«·… «·ÕœÀ »⁄œ «·Õ’Ê· ⁄·Ï «·—«»ÿ
            }
        }

        webView.Navigated += Handler;

        return await tcs.Task;
    }
}
