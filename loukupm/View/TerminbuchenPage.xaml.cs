using CommunityToolkit.Maui.Views;
using loukupm.ViewModel;
using Microsoft.Maui.Controls;
namespace loukupm.View;

public partial class TerminbuchenPage : ContentPage
{
	public TerminbuchenPage()
	{
		InitializeComponent();
        this.BindingContext= new AppViewModel();    
        MonthYearLabel.Text = DateTime.Now.ToString("MMMM yyyy"); 
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
		await Navigation.PopAsync();
    }

    private async void Button_Clicked_1(object sender, EventArgs e)
    {
        // √‰‘∆ «·‹ Bottom Sheet
        var sheet = new BottomShee(); //  √ﬂœ «”„ «·ﬂ·«” ’ÕÌÕ

        // ﬁ„ » ⁄ÌÌ‰ BindingContext ≈–« ﬂ‰   ” Œœ„ »Ì«‰« 
        sheet.BindingContext = this.BindingContext; // ‰›” ViewModel «·’›Õ…

        // ⁄—÷ «·‹ Bottom Sheet
        await sheet.ShowAsync(); // »œÊ‰  „—Ì— Window
    }

}