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
        string message = "Karte erfolgreich hinzugefügt.";
        await Toast.Make(message, ToastDuration.Short, 14).Show();
    }

    // تم نقل منطق الدفع إلى PaymentViewModel عبر PayCommand

   



}
