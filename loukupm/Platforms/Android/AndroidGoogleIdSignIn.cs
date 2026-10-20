using Android.Content;
using AndroidX.Core.Content;
using AndroidX.Credentials;
using Java.Util.Concurrent;
using Xamarin.GoogleAndroid.Libraries.Identity.GoogleId;
using Object = Java.Lang.Object;

namespace loukupm;

/// <summary>
/// Google account picker (Credential Manager). Returns a Google ID token
/// without opening the Firebase auth handler page.
/// </summary>
public static class AndroidGoogleIdSignIn
{
    public static Task<string> GetIdTokenAsync(string webClientId)
    {
        if (string.IsNullOrWhiteSpace(webClientId))
            throw new InvalidOperationException("Google Web client id is not configured.");

        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity
            ?? throw new InvalidOperationException("Google sign-in needs a visible screen.");

        var option = new GetSignInWithGoogleOption.Builder(webClientId).Build();
        var request = new GetCredentialRequest.Builder()
            .AddCredentialOption(option)
            .Build();

        var manager = CredentialManager.Create(activity);
        var completion = new TaskCompletionSource<GetCredentialResponse>();
        IExecutor executor = ContextCompat.GetMainExecutor(activity);
        manager.GetCredentialAsync(activity, request, null, executor, new CredentialCallback(completion));
        return ReadTokenAsync(completion.Task);
    }

    private static async Task<string> ReadTokenAsync(Task<GetCredentialResponse> pending)
    {
        GetCredentialResponse response = await pending;

        var credential = response?.Credential;
        if (credential == null)
            throw new InvalidOperationException("Google did not return an account.");

        var google = credential as GoogleIdTokenCredential
            ?? GoogleIdTokenCredential.CreateFrom(credential.Data);

        if (string.IsNullOrWhiteSpace(google.IdToken))
            throw new InvalidOperationException("Google did not return an ID token.");

        return google.IdToken;
    }

    private sealed class CredentialCallback : Object, ICredentialManagerCallback
    {
        private readonly TaskCompletionSource<GetCredentialResponse> _completion;

        public CredentialCallback(TaskCompletionSource<GetCredentialResponse> completion)
        {
            _completion = completion;
        }

        public void OnResult(Object? result)
        {
            if (result is GetCredentialResponse response)
                _completion.TrySetResult(response);
            else
                _completion.TrySetException(new InvalidOperationException("Google did not return an account."));
        }

        public void OnError(Object error)
        {
            var typeName = error?.GetType().Name ?? string.Empty;

            if (typeName.Contains("Cancellation", StringComparison.Ordinal))
            {
                _completion.TrySetException(new OperationCanceledException());
                return;
            }

            if (typeName.Contains("NoCredential", StringComparison.Ordinal))
            {
                _completion.TrySetException(new InvalidOperationException("No Google account is available on this device."));
                return;
            }

            if (typeName.Contains("ProviderConfiguration", StringComparison.Ordinal))
            {
                _completion.TrySetException(new InvalidOperationException(
                    "Google sign-in is not registered for this app. Add the Android package name and SHA-1 in Firebase."));
                return;
            }

            _completion.TrySetException(new InvalidOperationException(error?.ToString() ?? "Google sign-in failed."));
        }
    }
}
