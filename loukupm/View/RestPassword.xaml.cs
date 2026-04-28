using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using loukupm.View.MassgingApp;
using loukupm.ViewModel;
using loukupm.Services;
using System.Text;
using System.Text.Json;


namespace loukupm.View;

public partial class RestPassword : ContentPage
{
    private bool _isLoading = false;

    public RestPassword()
    {
        InitializeComponent();
        Shell.SetNavBarIsVisible(this, false);
        this.BindingContext = AppViewModel.Instance;
    }

    /// <summary>
    /// معالج زر العودة - يستخدم نظام الملاحة المركزي
    /// يتبع القاعدة: صفحات تدفق الملف الشخصي → //ProfilePage مباشرة
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await NavigationService.HandleBackButton(NavigationService.ROUTE_REST_PASSWORD);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RestPassword] Back button error: {ex.Message}");
                }
            });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RestPassword] OnBackButtonPressed crash: {ex.Message}");
            return true;
        }
    }
    private async void Button_Clicked(object sender, EventArgs e)
    {
        SendOtpButton.IsVisible = false;
     
        if (_isLoading) return;

        try
        {
            _isLoading = true;

            var viewModel = BindingContext as AppViewModel;
            if (viewModel == null)
            {
                await DisplayAlert("خطأ", "حدث خطأ في تحميل البيانات", "حسناً");
                return;
                SendOtpButton.IsVisible = true;
            }

           
            if (string.IsNullOrWhiteSpace(viewModel.Email))
            {
                var popup = new EroreInputEmaile();
                await this.ShowPopupAsync(popup);
                SendOtpButton.IsVisible = true;
                return;
               
            }

          
            if (!IsValidEmail(viewModel.Email))
            {
                var popup = new EroreInputEmaile();
                await this.ShowPopupAsync(popup);
                SendOtpButton.IsVisible = true;
                return;

            }

         
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                 var popup = new NoEnternetConacted();
                await this.ShowPopupAsync(popup);
                SendOtpButton.IsVisible = true;
                return;
            }
            ShowLoadingIndicator(true);
            bool success = await SendOtpRequestAsync(viewModel.Email);

            if (success)
            {
                var popup = new CompletSendEmail();
                await this.ShowPopupAsync(popup);
                await Navigation.PushAsync(new Verificationpage());
                SendOtpButton.IsVisible = true;
            }
            else
            {
                var popup = new NoServerResponse();
                await this.ShowPopupAsync(popup);
                SendOtpButton.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            var popup = new NoServerResponse();
            await this.ShowPopupAsync(popup);
            SendOtpButton.IsVisible = true;
        }
        finally
        {
            _isLoading = false;
            ShowLoadingIndicator(false);
            SendOtpButton.IsVisible = true;
        }
    }

 
    private async Task<bool> SendOtpRequestAsync(string email)
    {
        try
        {
            using var client = new HttpClient();

            
            var payload = new { email = email };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(
                "https://test.center-yazan.com/api/auth/request-otp",
                content
            );

          
            if (response.IsSuccessStatusCode)
            {
                var popup = new CompletSendEmail();
                await this.ShowPopupAsync(popup);
                await Navigation.PushAsync(new Verificationpage());
                return true;
            }

           
            var errorContent = await response.Content.ReadAsStringAsync();
           


            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var popup = new EmaileIsNotFound();
                await this.ShowPopupAsync(popup);
             
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var popup = new EroreInputEmaile();
                await this.ShowPopupAsync(popup);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                var popup = new WateResposeOTP();
                await this.ShowPopupAsync(popup);
            }

            return false;
        }
        catch (HttpRequestException ex)
        {
            var popup = new NoServerResponse();
            await this.ShowPopupAsync(popup);
            return false;
        }
        catch (Exception ex)
        {
            var popup = new NoServerResponse();
            await this.ShowPopupAsync(popup);
            return false;
        }
    }
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

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

                if (SendOtpButton != null)
                {
                    SendOtpButton.IsEnabled = !show;
                    SendOtpButton.Opacity = show ? 0.6 : 1.0;
                }

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ShowLoadingIndicator: {ex.Message}");
            }
        });
    }
}