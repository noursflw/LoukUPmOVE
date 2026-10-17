using System.Threading;

namespace loukupm.View;

public partial class GoogleAuthPage : ContentPage
{
#if IOS || MACCATALYST
    private const string BrowserUserAgent =
        "Mozilla/5.0 (iPhone; CPU iPhone OS 18_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.0 Mobile/15E148 Safari/604.1";
#elif WINDOWS
    private const string BrowserUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";
#else
    private const string BrowserUserAgent =
        "Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Mobile Safari/537.36";
#endif

    private readonly string _authUri;
    private readonly string _handlerPrefix;
    private readonly TaskCompletionSource<string?> _done = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private CancellationTokenSource? _timeoutCts;
    private int _completed;
    private bool _started;
    private bool _closing;

    public GoogleAuthPage(string authUri, string handlerPrefix)
    {
        InitializeComponent();
        _authUri = authUri;
        _handlerPrefix = handlerPrefix;
        AuthWebView.UserAgent = BrowserUserAgent;
        AuthWebView.Navigating += OnNavigating;
        AuthWebView.Navigated += OnNavigated;
    }

    public static Task<string> CaptureAsync(string authUri, string handlerPrefix)
    {
        var page = new GoogleAuthPage(authUri, handlerPrefix);

        return MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var host = Application.Current?.Windows.FirstOrDefault()?.Page
                       ?? Application.Current?.MainPage
                       ?? throw new InvalidOperationException("No page is available for Google sign-in.");

            await host.Navigation.PushModalAsync(page);
            return await page._done.Task ?? string.Empty;
        });
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        ConfigureBrowser();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ConfigureBrowser();

        if (_started)
            return;

        _started = true;
        _timeoutCts = new CancellationTokenSource();
        _ = WatchTimeoutAsync(_timeoutCts.Token);
        AuthWebView.Source = _authUri;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _closing = true;

        if (Interlocked.CompareExchange(ref _completed, 0, 0) == 0)
            _ = CompleteAsync(null, cancelled: true, error: null);
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        _ = CompleteAsync(null, cancelled: true, error: null);
    }

    private void OnNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (!TryTakeRedirect(e.Url, out var denied))
            return;

        e.Cancel = true;
        var url = e.Url;
        Dispatcher.Dispatch(async () =>
        {
            if (denied)
            {
                await CompleteAsync(
                    null,
                    cancelled: false,
                    error: new InvalidOperationException("Google sign-in was denied."));
            }
            else
            {
                await CompleteAsync(url, cancelled: false, error: null);
            }
        });
    }

    private void OnNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (LoadingIndicator.IsVisible)
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }

        if (!TryTakeRedirect(e.Url, out var denied))
            return;

        if (denied)
            _ = CompleteAsync(null, cancelled: false, error: new InvalidOperationException("Google sign-in was denied."));
        else
            _ = CompleteAsync(e.Url, cancelled: false, error: null);
    }

    private bool TryTakeRedirect(string? url, out bool denied)
    {
        denied = false;

        if (string.IsNullOrWhiteSpace(url) ||
            !url.StartsWith(_handlerPrefix, StringComparison.OrdinalIgnoreCase) ||
            !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var data = (uri.Query + uri.Fragment).ToLowerInvariant();
        if (data.Contains("error=", StringComparison.Ordinal))
        {
            denied = true;
            return true;
        }

        return data.Contains("code=", StringComparison.Ordinal)
               || data.Contains("id_token=", StringComparison.Ordinal)
               || data.Contains("access_token=", StringComparison.Ordinal)
               || data.Contains("oauth_token=", StringComparison.Ordinal);
    }

    private async Task WatchTimeoutAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromMinutes(3), token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        await CompleteAsync(
            null,
            cancelled: false,
            error: new TimeoutException("Google sign-in timed out. Please try again."));
    }

    private async Task CompleteAsync(string? url, bool cancelled, Exception? error)
    {
        if (Interlocked.Exchange(ref _completed, 1) != 0)
            return;

        try
        {
            _timeoutCts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        try
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (!_closing && Navigation.ModalStack.Contains(this))
                {
                    _closing = true;
                    await Navigation.PopModalAsync();
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Google auth page close failed: {ex.Message}");
        }

        if (cancelled)
            _done.TrySetResult(null);
        else if (error != null)
            _done.TrySetException(error);
        else
            _done.TrySetResult(url);
    }

    private void ConfigureBrowser()
    {
        AuthWebView.UserAgent = BrowserUserAgent;

#if ANDROID
        if (AuthWebView.Handler?.PlatformView is Android.Webkit.WebView androidWebView)
        {
            androidWebView.Settings.JavaScriptEnabled = true;
            androidWebView.Settings.DomStorageEnabled = true;
            androidWebView.Settings.UserAgentString = BrowserUserAgent;
        }
#elif IOS || MACCATALYST
        if (AuthWebView.Handler?.PlatformView is WebKit.WKWebView wkWebView)
            wkWebView.CustomUserAgent = BrowserUserAgent;
#endif
    }
}
