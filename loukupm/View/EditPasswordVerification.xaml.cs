using loukupm.ViewModel;
using System.Text;
using System.Text.Json;

namespace loukupm.View;

public partial class EditPasswordVerification : ContentPage
{
    private bool _isProcessing = false;

    public EditPasswordVerification()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);
        this.BindingContext = AppViewModel.Instance;
    }

    /// <summary>
    /// ãÚÇáÌ ÒÑ ÊÍÏíË ßáãÉ ÇáãÑæÑ
    /// ? ÇáÊÍŞŞ ãä ÇáÈíÇäÇÊ
    /// ? ÅÑÓÇá ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ
    /// ? ÇáÇäÊŞÇá ÚäÏ ÇáäÌÇÍ
    /// ? ÚÑÖ ÑÓÇáÉ ÎØÃ ÚäÏ ÇáİÔá
    /// </summary>
    private async void Button_Clicked(object sender, EventArgs e)
    {
        // ãäÚ ÇáÖÛØ ÇáãÊßÑÑ
        if (_isProcessing) return;

        try
        {
            _isProcessing = true;
            
            // ÚÑÖ ãÄÔÑ ÇáÊÍãíá
            ShowLoadingIndicator(true);

            var viewModel = BindingContext as AppViewModel;
            if (viewModel == null)
            {
                await DisplayAlert("ÎØÃ", "ÍÏË ÎØÃ İí ÊÍãíá ÇáÈíÇäÇÊ", "ÍÓäÇğ");
                return;
            }

            // ? ÇáÊÍŞŞ ãä ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ
            if (string.IsNullOrWhiteSpace(viewModel.NewPassword))
            {
                await DisplayAlert("ÊäÈíå", "íÑÌì ÅÏÎÇá ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ", "ÍÓäÇğ");
                return;
            }

            // ? ÇáÊÍŞŞ ãä ÊÃßíÏ ßáãÉ ÇáãÑæÑ
            if (string.IsNullOrWhiteSpace(viewModel.ConfirmPassword))
            {
                await DisplayAlert("ÊäÈíå", "íÑÌì ÊÃßíÏ ßáãÉ ÇáãÑæÑ", "ÍÓäÇğ");
                return;
            }

            // ? ÇáÊÍŞŞ ãä ÊØÇÈŞ ßáãÇÊ ÇáãÑæÑ
            if (viewModel.NewPassword != viewModel.ConfirmPassword)
            {
                await DisplayAlert("ÎØÃ", "ßáãÇÊ ÇáãÑæÑ ÛíÑ ãÊØÇÈŞÉ", "ÍÓäÇğ");
                return;
            }

            // ? ÇáÊÍŞŞ ãä Øæá ßáãÉ ÇáãÑæÑ (íÌÈ Ãä Êßæä 6 ÃÍÑİ Úáì ÇáÃŞá)
            if (viewModel.NewPassword.Length < 6)
            {
                await DisplayAlert("ÎØÃ", "ßáãÉ ÇáãÑæÑ íÌÈ Ãä Êßæä 6 ÃÍÑİ Úáì ÇáÃŞá", "ÍÓäÇğ");
                return;
            }

            // ? ÇáÊÍŞŞ ãä ÇáÇÊÕÇá ÈÇáÅäÊÑäÊ
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                await DisplayAlert("ÎØÃ", "áÇ íæÌÏ ÇÊÕÇá ÈÇáÅäÊÑäÊ", "ÍÓäÇğ");
                return;
            }

            // ÅÑÓÇá ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ
            bool success = await ResetPasswordAsync(viewModel);

            if (success)
            {
                // ? äÌÇÍ - ÚÑÖ ÑÓÇáÉ æÇäÊŞÇá
                await DisplayAlert(
                    "Êã ÈäÌÇÍ",
                    "Êã ÊÍÏíË ßáãÉ ÇáãÑæÑ ÈäÌÇÍ",
                    "ÍÓäÇğ"
                );

                // ÇáÇäÊŞÇá áÕİÍÉ ÇáÔßÑ/ÇáäÌÇÍ
                await Navigation.PushAsync(new ChackoutPage());
            }
            else
            {
                // ? İÔá - ÚÑÖ ÑÓÇáÉ ÎØÃ
                await DisplayAlert(
                    "İÔá",
                    "İÔá ÊÍÏíË ßáãÉ ÇáãÑæÑ. íÑÌì ÇáãÍÇæáÉ ãÑÉ ÃÎÑì",
                    "ÍÓäÇğ"
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Exception: {ex.Message}");
            await DisplayAlert("ÎØÃ", "ÍÏË ÎØÃ ÛíÑ ãÊæŞÚ", "ÍÓäÇğ");
        }
        finally
        {
            _isProcessing = false;
            ShowLoadingIndicator(false);
        }
    }

    /// <summary>
    /// ÚÑÖ/ÅÎİÇÁ ãÄÔÑ ÇáÊÍãíá æÊÚØíá ÇáÒÑ
    /// </summary>
    private void ShowLoadingIndicator(bool show)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                if (LoadingIndicator != null)
                {
                    LoadingIndicator.IsVisible = show;
                    LoadingIndicator.IsRunning = show;
                }

                if (SavePasswordButton != null)
                {
                    SavePasswordButton.IsEnabled = !show;
                    SavePasswordButton.Opacity = show ? 0.6 : 1.0;
                }

                Console.WriteLine(show ? "? Loading... (Button disabled)" : "? Done loading (Button enabled)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error in ShowLoadingIndicator: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// ÅÑÓÇá ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ Åáì ÇáÎÇÏã
    /// ? ÇáÍÕæá Úáì ÇáÈÑíÏ ãä ViewModel
    /// ? ÅÑÓÇá ßáãÉ ÇáãÑæÑ ÇáÌÏíÏÉ æÇáÊÃßíÏ
    /// ? ãÚÇáÌÉ ÇáÃÎØÇÁ
    /// </summary>
    private async Task<bool> ResetPasswordAsync(AppViewModel viewModel)
    {
        try
        {
            using var client = new HttpClient();

            // ÇáÊÍŞŞ ãä æÌæÏ ÇáÈÑíÏ
            if (string.IsNullOrWhiteSpace(viewModel.Email))
            {
                Console.WriteLine("? Email not found in ViewModel");
                await DisplayAlert("ÎØÃ", "ÇáÈÑíÏ ÇáÅáßÊÑæäí ÛíÑ ãæÌæÏ", "ÍÓäÇğ");
                return false;
            }

            // ÈäÇÁ ÇáÈíÇäÇÊ ÇáãØáæÈÉ
            var payload = new
            {
                email = viewModel.Email,
                password = viewModel.NewPassword,
                password_confirmation = viewModel.ConfirmPassword
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Console.WriteLine($"?? Sending reset password request for email: {viewModel.Email}");

            // ÅÑÓÇá ÇáØáÈ Åáì API
            var response = await client.PostAsync(
                "https://test.center-yazan.com/api/auth/reset-password",
                content
            );

            // ? ÇáÊÍŞŞ ãä ÇáäÌÇÍ
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("? Password reset successfully");
                return true;
            }

            // ? ãÚÇáÌÉ ÇáÃÎØÇÁ
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"? Error: {response.StatusCode} - {errorContent}");

            // ÚÑÖ ÑÓÇÆá ÎØÃ ãÍÏÏÉ ÍÓÈ ÑãÒ ÇáÎØÃ
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                await DisplayAlert("ÎØÃ", "ÇáÈíÇäÇÊ ÇáãÏÎáÉ ÛíÑ ÕÍíÍÉ", "ÍÓäÇğ");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                await DisplayAlert("ÎØÃ", "ÇáÈÑíÏ ÇáÅáßÊÑæäí ÛíÑ ãæÌæÏ İí ÇáäÙÇã", "ÍÓäÇğ");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                await DisplayAlert("ÎØÃ", "ÍÇæáÊ ãÑÇÊ ßËíÑÉ. íÑÌì ÇáÇäÊÙÇÑ", "ÍÓäÇğ");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await DisplayAlert("ÎØÃ", "ÇáÌáÓÉ ÇäÊåÊ. íÑÌì ÇáãÍÇæáÉ ãÑÉ ÃÎÑì", "ÍÓäÇğ");
            }

            return false;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"? Network error: {ex.Message}");
            await DisplayAlert("ÎØÃ", "İÔá ÇáÇÊÕÇá ÈÇáÎÇÏã", "ÍÓäÇğ");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Exception: {ex.Message}");
            await DisplayAlert("ÎØÃ", "ÍÏË ÎØÃ ÛíÑ ãÊæŞÚ", "ÍÓäÇğ");
            return false;
        }
    }
}
