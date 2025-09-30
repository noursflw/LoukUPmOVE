using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;



namespace loukupm.View;

public partial class Paymentgetway : ContentPage
{
	public Paymentgetway()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Navigation.PopAsync();
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        string message = "Karte erfolgreich hinzugefügt .";
        var toast = Toast.Make(message, ToastDuration.Short, 14);
        await toast.Show();

    }
}