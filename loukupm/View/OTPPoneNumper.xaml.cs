using loukupm.ViewModel;

namespace loukupm.View;

public partial class OTPPoneNumper : ContentPage
{
	public OTPPoneNumper()
	{
		InitializeComponent();
		BindingContext = AppViewModel.Instance;
    }
}