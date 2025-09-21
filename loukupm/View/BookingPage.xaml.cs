using System.Threading.Tasks;

namespace loukupm.View;

public partial class BookingPage : ContentPage
{
	public BookingPage()
	{
		InitializeComponent();
	}

    private void Button_Clicked(object sender, EventArgs e)
    {

    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}