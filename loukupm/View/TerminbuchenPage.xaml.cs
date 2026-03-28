using CommunityToolkit.Maui.Views;
using loukupm.Model;
using loukupm.ViewModel;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

namespace loukupm.View;

public partial class TerminbuchenPage : ContentPage
{
    public ObservableCollection<DayItem> ProviderDays { get; set; }

    public Command<DayItem> SelectDayCommand { get; set; }

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

    }
    
    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        var vm = AppViewModel.Instance;
        int idd = vm.SelectedProvider.Id;
        vm.CurrentBooking.ProviderId =idd.ToString();
        vm.CurrentBooking.Date =vm.SelectedDate;
        vm.CurrentBooking.Time =vm.SelectedTime;

        await Navigation.PushAsync(new BookingPage());
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // ? ãÚÇáÌ ÍÐÝ ÇáÎÏãÇÊ
    private void OnRemoveService(object sender, EventArgs e)
    {
        try
        {
            if (sender is Button button && button.BindingContext is Servies service)
            {
                var viewModel = this.BindingContext as AppViewModel;
                if (viewModel != null)
                {
                    viewModel.RemoveSelectedService(service);
                    
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Toast.Make($"Removed: {service.NameServies}", ToastDuration.Short).Show();
                    });
                }
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Error", ex.Message, "OK");
            });
        }
    }
}