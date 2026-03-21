using System;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Views;
using loukupm.Services;

namespace loukupm.View;

public partial class RemoveUserPopup : Popup
{
    public RemoveUserPopup()
    {
        InitializeComponent();
        this.Color= Colors.Transparent;
    }

    // Ì⁄Ìœ true √Ê false ··‰ ÌÃ…
    private async void YesClicked(object? sender, EventArgs e)
    {
        // Õ–› Ã„Ì⁄ «·»Ì«‰«  «·„Õ›ÊŸ…
        SecureStorage.RemoveAll();
        Preferences.Clear();

        // „”Õ Œ—Ìÿ… «· ‰ﬁ·
     //   NavigationService.ClearPageSourceMap();

        // Reset authentication check flag
        App.ResetAuthenticationCheck();

        // Add a small delay to ensure popup closes first
        Close(true);
        await Task.Delay(300);

        // Use the ShellNavigationManager to properly clear the stack
        try
        {
            await ShellNavigationManager.NavigateToLoginAndClear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Navigation error during account removal: {ex.Message}");
        }
    }


    private void CancelClicked(object? sender, EventArgs e)
    {
        Close(false);
    }
}
