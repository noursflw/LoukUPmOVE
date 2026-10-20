using CommunityToolkit.Maui.Views;

namespace loukupm.View.MassgingApp;

public partial class OtpMessagePopup : Popup
{
    public OtpMessagePopup(string title, string message, bool isSuccess)
    {
        InitializeComponent();

        TitleLabel.Text = title;
        MessageLabel.Text = message;
        IconImage.Source = isSuccess ? "erorematchpass.svg" : "icone55.svg";
    }

    private void OnOkClicked(object sender, EventArgs e)
    {
        Close(true);
    }
}
