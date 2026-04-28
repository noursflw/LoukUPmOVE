using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using loukupm.Langue;
using loukupm.Model;
using loukupm.Services;
using loukupm.ViewModel;
using System.Windows.Input;

namespace loukupm.View;

public partial class ServicesPage : ContentPage
{
	public ServicesPage()
	{
		InitializeComponent();
        this.BindingContext = AppViewModel.Instance;
    }

    /// <summary>
    /// Service selection handler - delegates to ViewModel command for unified logic
    /// This ensures ServicesPage uses the same selection behavior as HomePage
    /// </summary>
    private void Button_Clicked_1(object sender, EventArgs e)
    {
        var service = (sender as Button)?.BindingContext as Servies;
        if (service == null) return;

        // Delegate to the ViewModel's SelectServiceButtonCommand for unified logic
        var vm = BindingContext as AppViewModel;
        if (vm?.SelectServiceButtonCommand is ICommand command && command.CanExecute(service))
        {
            command.Execute(service);
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
        // TabBar page: Delegate to centralized back button logic
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(NavigationService.ROUTE_SERVICES);
        });
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

    /// <summary>
    /// Toast notifications for service selection are now handled in the ViewModel command
    /// This method is no longer needed as the notification is centralized
    /// </summary>
}