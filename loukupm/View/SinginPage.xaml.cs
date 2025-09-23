namespace loukupm.View;

public partial class SinginPage : ContentPage
{
	public SinginPage()
	{
		InitializeComponent();
	}

	private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{
		await Navigation.PopAsync();
	}
}