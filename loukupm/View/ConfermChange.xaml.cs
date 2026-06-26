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
            PopupCard.FadeTo(1, 180, Easing.CubicOut),
            PopupCard.ScaleTo(1, 180, Easing.CubicOut)
        );

        await Task.Delay(1500);

        await Task.WhenAll(
            PopupCard.ScaleTo(0.92, 150, Easing.CubicIn),
            PopupCard.FadeTo(0, 150, Easing.CubicIn)
        );

        Close(null);
    }
}
