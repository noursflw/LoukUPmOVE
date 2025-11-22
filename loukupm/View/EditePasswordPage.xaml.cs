using loukupm.ViewModel;

namespace loukupm.View;

public partial class EditePasswordPage : ContentPage
{
    public EditePasswordPage()
    {
        InitializeComponent();
        BindingContext = new AppViewModel();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//ProfilePage");
    }       
}