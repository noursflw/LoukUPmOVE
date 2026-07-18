using CommunityToolkit.Maui.Views;
using loukupm.Services;
using loukupm.ViewModel;


namespace loukupm.View;

public partial class MassegBoxLogout : Popup
{
	public MassegBoxLogout()
	{
        BindingContext= new AppViewModel();
        InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        Close(true);
      
        await Task.Delay(300);
        
        try
        {
            await ShellNavigationManager.NavigateToLoginAndClear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Logout navigation error: {ex.Message}");
        }
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Close(true);
    }
}