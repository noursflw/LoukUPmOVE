using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using loukupm.services;
using loukupm.Services;
using loukupm.ViewModel;
using Microsoft.Maui.Controls;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace loukupm.View;

public partial class Paymentgetway : ContentPage
{
    private readonly ApiServices _api = new ApiServices();

    public Paymentgetway()
    {
        InitializeComponent();
        BindingContext = new PaymentViewModel(_api);
    }

    /// <summary>
    /// معالج زر العودة - يستخدم نظام الملاحة المركزي
    /// يتبع القاعدة: جميع الصفحات الأخرى → pop one level
    /// </summary>
    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(NavigationService.ROUTE_PAYMENT);
        });
        return true;
    }


    private async void Button_Clicked(object sender, EventArgs e)
    {
        await NavigationService.HandleBackButton(NavigationService.ROUTE_PAYMENT);
    }

   
    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        var vm = AppViewModel.Instance;
        var booking = vm.CurrentBooking;

       
        if (booking.PaymentMethod != "Card")
        {
            await DisplayAlert("Error", "Payment method is not Card.", "OK");
            return;
        }

       
        var cardNumber = CardNumberEntry.Text?.Replace(" ", ""); 
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
        {
            await DisplayAlert("Error", "Please enter a valid card number.", "OK");
            return;
        }

        
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Error", "Please enter the card holder's name.", "OK");
            return;
        }

       
        if (ExpDatePicker.Date < DateTime.Now)
        {
            await DisplayAlert("Error", "Please enter a valid expiration date.", "OK");
            return;
        }

      
        if (string.IsNullOrWhiteSpace(CvvEntry.Text) || CvvEntry.Text.Length < 3)
        {
            await DisplayAlert("Error", "Please enter a valid CVV.", "OK");
            return;
        }

       
        booking.CardHolderName = NameEntry.Text;
        booking.CardNumber = "**** **** **** " + cardNumber.Substring(cardNumber.Length - 4); 
        booking.ExpirationDate = (ExpDatePicker.Date ?? DateTime.Now).ToString("MM/yyyy");
        booking.CVV = CvvEntry.Text;

        await vm.PostBookingAsync();

        await DisplayAlert("Success", "Payment info saved.", "OK");

        // ✅ الذهاب إلى صفحة رئيسية (HomePage) باستخدام NavigationService
        await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_HOME);
    }
}
