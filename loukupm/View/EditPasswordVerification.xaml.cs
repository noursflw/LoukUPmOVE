using loukupm.ViewModel;

namespace loukupm.View;

public partial class EditPasswordVerification : ContentPage
{
    public EditPasswordVerification()
    {
        InitializeComponent();
        BindingContext = new AppViewModel();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Verificationpage");
    }       
}
