using loukupm.Services;
using loukupm.ViewModel;

namespace loukupm.View;

public partial class OTPPoneNumper : ContentPage
{
	public OTPPoneNumper()
	{
		InitializeComponent();
		BindingContext = AppViewModel.Instance;
    }

    private async void OnVerifyClicked(object sender, EventArgs e)
    {
        try
        {
            var verified = await AppViewModel.Instance.ConfirmPhoneOtpAsync();
            if (!verified)
                return;

            await NavigationService.LeavePhoneOtpPageAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OTPPoneNumper] Verify navigation failed: {ex}");
        }
    }

    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await NavigationService.LeavePhoneOtpPageAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OTPPoneNumper] Back navigation failed: {ex.Message}");
            }
        });

        return true;
    }
}