using CommunityToolkit.Maui.Views;
using loukupm.ViewModel;
namespace loukupm.View.MassgingApp;

public partial class ErorRemoveMyAccount : Popup
{
	public ErorRemoveMyAccount()
	{
		InitializeComponent();
        this.BindingContext = AppViewModel.Instance;

    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Close(true);
    }
}