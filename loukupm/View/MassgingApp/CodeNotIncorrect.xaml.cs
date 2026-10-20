using CommunityToolkit.Maui.Views;

namespace loukupm.View.MassgingApp;

public partial class CodeNotIncorrect : Popup
{
    public CodeNotIncorrect()
    {
        InitializeComponent();
    }

    public CodeNotIncorrect(string message) : this()
    {
        if (!string.IsNullOrWhiteSpace(message) && MessageLabel != null)
            MessageLabel.Text = message;
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Close(true);
    }
}
