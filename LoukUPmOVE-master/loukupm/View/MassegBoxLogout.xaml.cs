
using CommunityToolkit.Maui.Views;

namespace loukupm.View;

public partial class MassegBoxLogout : Popup
{
    public MassegBoxLogout()
    {
        InitializeComponent();
    }

    // YES
    private void Button_Clicked(object sender, EventArgs e)
    {
        Close(true);
    }

    // NO
    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Close(false);
    }
}
