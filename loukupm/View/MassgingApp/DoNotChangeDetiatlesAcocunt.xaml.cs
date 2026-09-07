using CommunityToolkit.Maui.Views;
using loukupm.Services;
using loukupm.ViewModel;

namespace loukupm.View.MassgingApp;

public partial class DoNotChangeDetiatlesAcocunt : Popup
{
	public DoNotChangeDetiatlesAcocunt()
	{
		InitializeComponent();
        this.BindingContext = AppViewModel.Instance;
    }


    private async void Button_Clicked(object sender, EventArgs e)
    {
        // User confirmed discard/leave
        try
        {
            this.Close(true);
        }
        catch { this.Close(true); }
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        // User chose to cancel / stay on page
        try
        {
            this.Close(false);
        }
        catch { this.Close(false); }

    }


}