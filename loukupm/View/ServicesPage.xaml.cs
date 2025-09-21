using loukupm.ViewModel;

namespace loukupm.View;

public partial class ServicesPage : ContentPage
{
	public ServicesPage()
	{
		InitializeComponent();
        this.BindingContext = new AppViewModel();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HomePage");
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new TerminbuchenPage());
    }
}