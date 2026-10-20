using CommunityToolkit.Maui.Views;

namespace loukupm.View.MassgingApp;

public partial class PleaseEnterCurrentPasswordPopup : Popup
{
	public PleaseEnterCurrentPasswordPopup()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		var POUP =new ErorRemoveMyAccount();
		await App.Current.MainPage.ShowPopupAsync(POUP);
		this.Close();
    }
}