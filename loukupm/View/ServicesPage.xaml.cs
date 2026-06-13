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

  
    private void Button_Clicked_1(object sender, EventArgs e)
    {
        var service = (sender as Button)?.BindingContext as Servies;
        if (service == null) return;

        var vm = BindingContext as AppViewModel;
        if (vm?.SelectServiceButtonCommand is ICommand command && command.CanExecute(service))
        {
            command.Execute(service);
        }
    }

   
    private Frame _lastSelectedFrame;

    private readonly List<Frame> _categoryFrames = new();

    private async void OnCategoryTapped(object sender, TappedEventArgs e)
    {
        if (sender is Frame tappedFrame && tappedFrame.BindingContext is Category selectedCategory)
        {
            var vm = BindingContext as AppViewModel;
            vm?.FilterServices(selectedCategory);

            
            if (!_categoryFrames.Contains(tappedFrame))
                _categoryFrames.Add(tappedFrame);

           
            if (_lastSelectedFrame != null)
            {
                _lastSelectedFrame.BorderColor = Color.FromArgb("#444444");
                _lastSelectedFrame.BackgroundColor = Color.FromArgb("#444444");

                if (_lastSelectedFrame.Content is Label oldLabel)
                    oldLabel.TextColor = Color.FromArgb("#999999");
            }

         
            tappedFrame.BorderColor = Color.FromArgb("#C9A24A");
            tappedFrame.BackgroundColor = Color.FromArgb("#C9A24A");

           
            if (tappedFrame.Content is Label label)
            {
                label.TextColor = Color.FromArgb("#000000");
            }

            _lastSelectedFrame = tappedFrame;

            tappedFrame.AnchorX = 0.5;
            tappedFrame.AnchorY = 0.5;

            await tappedFrame.ScaleTo(1.05, 100, Easing.CubicOut);
            await tappedFrame.ScaleTo(1, 100, Easing.CubicIn);
        }
    }

    private void ResetCategoriesUI()
    {
        _lastSelectedFrame = null;

        foreach (var frame in _categoryFrames)
        {
            frame.BorderColor = Color.FromArgb("#444444");
            frame.BackgroundColor = Color.FromArgb("#444444");

            if (frame.Content is Label label)
                label.TextColor = Color.FromArgb("#999999");
        }
    }

  
    protected override bool OnBackButtonPressed()
    {
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

  
    private void Button_Clicked_3(object sender, EventArgs e)
    {
        ResetCategoriesUI();
    }
}