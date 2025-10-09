using loukupm.ViewModel;
namespace loukupm.View;

public partial class NotifictionPage : ContentPage
{
	public NotifictionPage()
	{
		InitializeComponent();
		this.BindingContext = AppViewModel.Instance;
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Navigation.PopAsync();
    }
}