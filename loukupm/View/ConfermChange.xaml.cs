using CommunityToolkit.Maui.Views;

namespace loukupm.View;

public partial class ConfermChange : Popup
{
	private bool _autoDismissStarted;

	public ConfermChange()
	{
		InitializeComponent();
		this.Color = Colors.Transparent;
		Opened += OnOpened;
	}

    private async void OnOpened(object? sender, EventArgs e)
    {
        if (_autoDismissStarted)
            return;

        _autoDismissStarted = true;

        await Task.WhenAll(
            PopupCard.FadeTo(1, 120, Easing.CubicOut),
            PopupCard.ScaleTo(1, 120, Easing.CubicOut)
        );

        await Task.Delay(1000);

        await Task.WhenAll(
            PopupCard.FadeTo(0, 120, Easing.CubicIn),
            PopupCard.ScaleTo(0.95, 120, Easing.CubicIn)
        );

        Close(null);
    }
}
