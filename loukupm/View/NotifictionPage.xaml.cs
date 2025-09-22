using loukupm.ViewModel;
namespace loukupm.View;

public partial class NotifictionPage : ContentPage
{
	public NotifictionPage()
	{
		InitializeComponent();
		this.BindingContext = new AppViewModel();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Navigation.PopAsync();
    }
}