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
}