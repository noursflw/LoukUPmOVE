using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using loukupm.View.MassgingApp;
using loukupm.ViewModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Linq;
using System.Text;
using System.Text.Json;
using UraniumUI.Material.Controls;

namespace loukupm.View
{
    public partial class Verificationpage : ContentPage
    {
        bool _suppress;
        Color _defaultBg;
        private bool _isVerifying = false;

        public Verificationpage()
        {
            InitializeComponent();
            Shell.SetNavBarIsVisible(this, false);
            this.BindingContext = AppViewModel.Instance;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _defaultBg = Digit1.BackgroundColor; 
        }

        private void Digit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppress) return;

            var fields = new[] { Digit1, Digit2, Digit3, Digit4 };
            var current = (TextField)sender;
            int index = System.Array.IndexOf(fields, current);
            if (index == -1) return;

            
            if (!string.IsNullOrEmpty(e.NewTextValue))
                current.BackgroundColor = _defaultBg;

            
            if (!string.IsNullOrEmpty(e.NewTextValue) && e.NewTextValue.Length == 1)
            {
                if (index < fields.Length - 1)
                    fields[index + 1].Focus();
                else
                    current.Unfocus();
                return;
            }
        }

        private int LastFilledIndex(TextField[] fields)
        {
            for (int i = fields.Length - 1; i >= 0; i--)
                if (!string.IsNullOrWhiteSpace(fields[i].Text))
                    return i;
            return -1;
        }

        private void HighlightEmptyFields()
        {
            var fields = new[] { Digit1, Digit2, Digit3, Digit4 };
            foreach (var f in fields)
                f.BackgroundColor = string.IsNullOrWhiteSpace(f.Text) ? Colors.Red : _defaultBg;
        }

        
        private async void ConfirmCode_Clicked(object sender, EventArgs e)
        {
           
            if (_isVerifying) return;

            try
            {
                var fields = new[] { Digit1, Digit2, Digit3, Digit4 };

                
                if (fields.Any(f => string.IsNullOrWhiteSpace(f.Text)))
                {
                    HighlightEmptyFields();
                    await DisplayAlert("تنبيه", "يرجى إدخال الرمز الكامل", "حسناً");
                    return;
                }

               
                string code = string.Concat(fields.Select(f => f.Text));

              
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    var popup = new NoEnternetConacted();
                    await this.ShowPopupAsync(popup);                
                    return;
                }

                _isVerifying = true;

               
                bool isValid = await VerifyOtpAsync(code);

                if (isValid)
                {
                    
                    var popup = new SuccessfullyVerified();
                    await this.ShowPopupAsync(popup);
                    await Navigation.PushAsync(new EditPasswordVerification());
                }
                else
                {
                    
                    var popup = new CodeNotIncorrect();
                    await this.ShowPopupAsync(popup);
                   
                                      
                    
                    foreach (var field in fields)
                    {
                        field.Text = string.Empty;
                        field.BackgroundColor = Colors.Red;
                    }

                   
                    Digit1.Focus();
                }
            }
            catch (Exception ex)
            {
              
                var popup = new NoServerResponse();
                await this.ShowPopupAsync(popup);
            }
            finally
            {
                _isVerifying = false;
            }
        }
        private async Task<bool> VerifyOtpAsync(string code)
        {
            try
            {
                using var client = new HttpClient();

                var viewModel = BindingContext as AppViewModel;
                if (viewModel == null || string.IsNullOrWhiteSpace(viewModel.Email))
                {
                    var popup = new EmaileIsNotFound();
                    await this.ShowPopupAsync(popup);
                    return false;
                }

                
                var payload = new
                {
                    email = viewModel.Email,
                    code = code,
                    type = 1
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

               

              
                var response = await client.PostAsync(
                    "https://test.center-yazan.com/api/auth/verify-otp",
                    content
                );

                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("✅ OTP verified successfully");
                    return true;
                }

              
                var errorContent = await response.Content.ReadAsStringAsync();
             

               
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var popup = new CodeNotIncorrect();
                    await this.ShowPopupAsync(popup);
                    return false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    var popup = new EmaileIsNotFound();
                    await this.ShowPopupAsync(popup);
                    return false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    

                    var popup = new WateResposeOTP();
                    await this.ShowPopupAsync(popup);
                    return false;
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
    }
}

