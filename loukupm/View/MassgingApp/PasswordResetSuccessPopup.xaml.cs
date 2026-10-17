using CommunityToolkit.Maui.Views;

namespace loukupm.View.MassgingApp;

public partial class PasswordResetSuccessPopup : Popup
{
    public PasswordResetSuccessPopup()
    {
        InitializeComponent();
    }

    private void Ok_Clicked(object sender, EventArgs e)
    {
        Close(true);
    }
}
