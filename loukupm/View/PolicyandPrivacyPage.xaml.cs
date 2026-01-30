using loukupm.ViewModel;
namespace loukupm.View;

public partial class PolicyandPrivacyPage : ContentPage
{
	public PolicyandPrivacyPage()
	{
		InitializeComponent();
		this.BindingContext = new AppViewModel();

    }
}