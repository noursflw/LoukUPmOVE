using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using loukupm.Langue;
using loukupm.ViewModel;

namespace loukupm.View.MassgingApp;

public partial class ErorRemoveMyAccount : Popup
{
	public ErorRemoveMyAccount()
	{
		InitializeComponent();
        this.BindingContext = AppViewModel.Instance;
    }

    private async void ConfirmDeleteClicked(object sender, EventArgs e)
    {
        var password = PasswordEntry?.Text;
        if (string.IsNullOrWhiteSpace(password))
        {
            await Toast.Make(AppResource.Pleaseenterthecurrentpassword).Show();
            return;
        }

        if (BindingContext is AppViewModel viewModel)
        {
            viewModel.CurrentPassword = password;
            viewModel.DeleteAccountCommand.Execute(password);
        }

        Close(true);
    }

    private void CancelClicked(object sender, EventArgs e)
    {
        Close(false);
    }
}
