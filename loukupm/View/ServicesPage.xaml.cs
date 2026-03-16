using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using loukupm.Langue;
using loukupm.Model;
using loukupm.ViewModel;

namespace loukupm.View;

public partial class ServicesPage : ContentPage
{
	public ServicesPage()
	{
		InitializeComponent();
        this.BindingContext = AppViewModel.Instance;
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        var service = (sender as Button)?.BindingContext as Servies;
        if (service == null) return;

        if (AppViewModel.Instance.CurrentBooking.SelectedServices == null)
            AppViewModel.Instance.CurrentBooking.SelectedServices = new List<Servies>();


        if (!AppViewModel.Instance.CurrentBooking.SelectedServices.Contains(service))
        {
            AppViewModel.Instance.CurrentBooking.SelectedServices.Add(service);
            await Toast.Make(Langue.AppResource.CompletedAddServies).Show();
        }

        else
        {
            AppViewModel.Instance.CurrentBooking.SelectedServices.Remove(service);
            await Toast.Make(Langue.AppResource.theserviewasdone).Show();
        }
        


    }


   

    private Frame _lastSelectedFrame;

    private async void OnCategoryTapped(object sender, TappedEventArgs e)
    {
        if (sender is Frame tappedFrame && tappedFrame.BindingContext is Category selectedCategory)
        {
            var vm = BindingContext as AppViewModel;
            vm?.FilterServices(selectedCategory);

            if (_lastSelectedFrame != null)
                _lastSelectedFrame.BorderColor = Color.FromArgb("#444444");

            tappedFrame.BorderColor = Color.FromArgb("#EBD750");
            _lastSelectedFrame = tappedFrame;

            // 🔹 ضبط AnchorX/AnchorY
            tappedFrame.AnchorX = 0.5;
            tappedFrame.AnchorY = 0.5;

            // تأثير Scale
            await tappedFrame.ScaleTo(1.05, 100, Easing.CubicOut);
            await tappedFrame.ScaleTo(1, 100, Easing.CubicIn);
        }
    }





    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//HomePage");
        return true;
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        var selected = AppViewModel.Instance.CurrentBooking.SelectedServices;

        if (selected == null || selected.Count == 0)
        {
            await Toast.Make(Langue.AppResource.pleaseselectoneservice).Show();
            return;
        }

        await Navigation.PushAsync(new TerminbuchenPage());
    }

    private async void Button_Clicked_3(object sender, EventArgs e)
    {
        await Toast.Make(Langue.AppResource.celectedserviesiddone).Show();
    }
}