using CommunityToolkit.Mvvm.Input;
using loukupm.Services;
using loukupm.services;
using loukupm.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace loukupm.View;

public partial class NotifictionPage : ContentPage
{
	public NotifictionPage()
	{
		InitializeComponent();
		BindingContext = ResolveViewModel();
	}

	private static NotificationViewModel ResolveViewModel()
	{
		try
		{
			var services = Application.Current?.Handler?.MauiContext?.Services;
			if (services?.GetService(typeof(NotificationViewModel)) is NotificationViewModel viewModel)
			{
				return viewModel;
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"❌ [NotifictionPage] Failed to resolve NotificationViewModel: {ex.Message}");
		}

		return new NotificationViewModel(new NotificationService(), new NotificationStateService());
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		if (BindingContext is NotificationViewModel viewModel && viewModel.Notifications.Count == 0)
		{
			await viewModel.LoadNotificationsCommand.ExecuteAsync(null);
		}
	}

	protected override bool OnBackButtonPressed()
	{
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			await NavigationService.HandleBackButton(NavigationService.ROUTE_NOTIFICATION);
		});
		return true;
	}

	private async void Button_Clicked(object sender, EventArgs e)
	{
		await NavigationService.HandleBackButton(NavigationService.ROUTE_NOTIFICATION);
	}
}
