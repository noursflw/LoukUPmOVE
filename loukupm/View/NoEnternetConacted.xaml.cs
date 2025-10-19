using CommunityToolkit.Maui.Views;

namespace loukupm.View;

public partial class NoEnternetConacted : Popup
{
	public NoEnternetConacted()
	{
		InitializeComponent();
        this.Color = Colors.Transparent;
    }

	private void CancelClicked(object? sender, EventArgs e)
	{
		Close(false);
    }
}