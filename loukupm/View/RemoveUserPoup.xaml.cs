using System;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Views;

namespace loukupm.View;

public partial class RemoveUserPopup : Popup
{
    public RemoveUserPopup()
    {
        InitializeComponent();
        this.Color= Colors.Transparent;
    }

    // Ì⁄Ìœ true ≈–« √ﬂœ «·„” Œœ„
    private async void YesClicked(object? sender, EventArgs e)
    {
        //await SecureStorage.Default.RemoveAll("user_token");

        // «·«‰ ﬁ«· ·’›Õ…  ”ÃÌ· «·œŒÊ·
        await Shell.Current.GoToAsync("SinginPage");

        // ≈–« ﬂ‰  œ«Œ· Popup „‰ CommunityToolkit
        Close(true);
    }


    private void CancelClicked(object? sender, EventArgs e)
    {
        Close(false);
    }
}
