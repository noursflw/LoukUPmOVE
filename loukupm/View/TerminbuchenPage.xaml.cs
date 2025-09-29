using loukupm.ViewModel;
namespace loukupm.View;

public partial class TerminbuchenPage : ContentPage
{
	public TerminbuchenPage()
	{
		InitializeComponent();
        this.BindingContext= new AppViewModel();    
        MonthYearLabel.Text = DateTime.Now.ToString("MMMM yyyy"); 
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Navigation.PopAsync();
    }

    private async  void Button_Clicked_1(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Paymentgetway());
    }
}