using CommunityToolkit.Maui.Views;
using loukupm.Langue;

namespace loukupm.View.MassgingApp;

public partial class EmaileUsed : Popup
{
	private readonly Func<Task>? _onLogin;

	public bool GoToLogin { get; private set; }

	public EmaileUsed(Func<Task>? onLogin = null)
	{
		_onLogin = onLogin;
		InitializeComponent();
		DialogRoot.FlowDirection = LocalizationResourcesManager.Instanse.GetFlowDirection();
	}

	private void CloseClicked(object? sender, EventArgs e)
	{
		Close();
	}

	private async void LoginClicked(object? sender, EventArgs e)
	{
		GoToLogin = true;
		Close();

		if (_onLogin == null)
			return;

		try
		{
			await MainThread.InvokeOnMainThreadAsync(_onLogin);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[EmaileUsed] Login navigation failed: {ex}");
		}
	}
}
