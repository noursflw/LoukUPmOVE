using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
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

        var vm = AppViewModel.Instance;

        vm.CurrentBooking = new Booking
        {
            ServiceName = service.NameServies,
            ServiceType = service.Category?.Name
        };

        await Navigation.PushAsync(new TerminbuchenPage());
    }

    //private void OnCategoryTapped(object sender, TappedEventArgs e)
    //{
    //    if (sender is Frame frame && frame.BindingContext is Category selectedCategory)
    //    {
    //        var vm = BindingContext as AppViewModel;
    //        vm?.FilterServices(selectedCategory.Name);

    //        // 🔥 تغيير لون الفريم لتوضيح التحديد (اختياري)
    //        frame.BackgroundColor = Color.FromArgb("#666666");

    //        // ممكن نضيف تأثير بصري بسيط
    //        Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(50));
    //    }
    //}

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
}