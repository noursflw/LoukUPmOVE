using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using loukupm.services;
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

    // زر الرجوع
    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // زر إضافة البطاقة (Toast فقط)
    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        var vm = AppViewModel.Instance;
        var booking = vm.CurrentBooking;

        // تحقق من نوع الدفع
        if (booking.PaymentMethod != "Card")
        {
            await DisplayAlert("Error", "Payment method is not Card.", "OK");
            return;
        }

        // تحقق من إدخال رقم البطاقة
        var cardNumber = CardNumberEntry.Text?.Replace(" ", ""); // إزالة المسافات
        if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
        {
            await DisplayAlert("Error", "Please enter a valid card number.", "OK");
            return;
        }

        // تحقق من اسم صاحب البطاقة
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Error", "Please enter the card holder's name.", "OK");
            return;
        }

        // تحقق من تاريخ الانتهاء
        if (ExpDatePicker.Date < DateTime.Now)
        {
            await DisplayAlert("Error", "Please enter a valid expiration date.", "OK");
            return;
        }

        // تحقق من CVV
        if (string.IsNullOrWhiteSpace(CvvEntry.Text) || CvvEntry.Text.Length < 3)
        {
            await DisplayAlert("Error", "Please enter a valid CVV.", "OK");
            return;
        }

        // ملء بيانات البطاقة في CurrentBooking
        booking.CardHolderName = NameEntry.Text;
        booking.CardNumber = "**** **** **** " + cardNumber.Substring(cardNumber.Length - 4); // إخفاء الأرقام
        booking.ExpirationDate = ExpDatePicker.Date.ToString("MM/yyyy");
        booking.CVV = CvvEntry.Text;

        // هنا يمكنك استدعاء API الدفع أو إرسال الحجز
        // await vm.PostBookingAsync();

        await DisplayAlert("Success", "Payment info saved.", "OK");

        // العودة إلى الصفحة الرئيسية
        await Shell.Current.GoToAsync("//HomePage");
    }





}
