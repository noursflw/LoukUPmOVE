using CommunityToolkit.Maui.Views;
using loukupm.Model;
using loukupm.ViewModel;
using Microsoft.Maui.Controls;
namespace loukupm.View;

public partial class TerminbuchenPage : ContentPage
{
	public TerminbuchenPage()
	{
		InitializeComponent();
        this.BindingContext= AppViewModel.Instance;    
        MonthYearLabel.Text = DateTime.Now.ToString("MMMM yyyy"); 
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Navigation.PopAsync();
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        
        var sheet = new BottomShee(); 

       
        sheet.BindingContext = this.BindingContext; 

       
        await sheet.ShowAsync(); 
    }


    


    private async void OnNext_Clicked(object sender, EventArgs e)
    {

        var vm = AppViewModel.Instance;
        int idd = vm.SelectedProvider.Id;
        vm.CurrentBooking.ProviderId =idd.ToString();
        vm.CurrentBooking.Date =vm.SelectedDate;
        vm.CurrentBooking.Time =vm.SelectedTime;

        await Navigation.PushAsync(new Paymentgetway());

    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}