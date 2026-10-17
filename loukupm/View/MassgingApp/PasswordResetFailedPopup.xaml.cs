using CommunityToolkit.Maui.Views;

namespace loukupm.View.MassgingApp;

public partial class PasswordResetFailedPopup : Popup
{
    public PasswordResetFailedPopup()
    {
        InitializeComponent();
    }

    private void Ok_Clicked(object sender, EventArgs e)
    {
        Close(true);
    }
}
