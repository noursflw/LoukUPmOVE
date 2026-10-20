using CommunityToolkit.Maui.Views;
using loukupm.ViewModel;
namespace loukupm.View.MassgingApp;

public partial class Itsphonenumber : Popup
{
	public Itsphonenumber()
	{
		InitializeComponent();
        this.BindingContext = AppViewModel.Instance;
    }

   
    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Close("edit");
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Close(true);
    }
}