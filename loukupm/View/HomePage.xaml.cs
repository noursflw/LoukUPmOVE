namespace loukupm.View;

using loukupm.ViewModel;


   public partial class HomePage : ContentPage
{
	public HomePage()
	{
		InitializeComponent();
    	this.BindingContext=new AppViewModel();

    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
		await Navigation.PushAsync(new ServicesPage());   

    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new NotifictionPage());
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AboutUS());
    }
}