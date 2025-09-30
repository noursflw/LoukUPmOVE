using The49.Maui.BottomSheet;

namespace loukupm.View;

public partial class BottomShee : BottomSheet
{
	public BottomShee()
	{
		InitializeComponent();
	}


    private void CloseButton_Clicked(object sender, EventArgs e)
    {
        this.DismissAsync();
    }
    private void CashRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value) // ≈–«  „ «·«Œ Ì«—
        {
            CashFrame.BorderColor = Color.FromArgb("#EBD750");
        }
        else // ≈–«  „ ≈·€«¡ «·«Œ Ì«—
        {
            CashFrame.BorderColor = Colors.Transparent;
        }
    }
    private void CardRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value) // ≈–«  „ «·«Œ Ì«—
        {
            CardFrame.BorderColor = Color.FromArgb("#EBD750");
        }
        else // ≈–«  „ ≈·€«¡ «·«Œ Ì«—
        {
            CardFrame.BorderColor = Colors.Transparent;
        }
    }
     
    private async void Button_Clicked(object sender, EventArgs e)
    {
      if(CashRadio.IsChecked)
        {

            await Application.Current.MainPage.DisplayAlert("Payment Method", "You selected Cash payment.", "OK");
            await Shell.Current.GoToAsync("//HomePage");
            await this.DismissAsync();
           
           

        }
        else if (CardRadio.IsChecked)
        {


            await Shell.Current.GoToAsync(nameof(Paymentgetway));

            await this.DismissAsync();
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Payment Method", "Please select a payment method.", "OK");
        }
    }

}