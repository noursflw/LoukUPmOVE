using CommunityToolkit.Mvvm.Input;
using loukupm.Services;
using loukupm.services;
using loukupm.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace loukupm.View;

[QueryProperty(nameof(NotificationId), "notificationId")]
[QueryProperty(nameof(Data), "data")]
public partial class NotifictionPage : ContentPage
{
    private string? _notificationIdFromQuery;

    public string NotificationId
    {
        get => _notificationIdFromQuery ?? string.Empty;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            _notificationIdFromQuery = value;
        }
    }

    private string? _data;
    public string Data
    {
        get => _data ?? string.Empty;
        set
        {
            _data = value;
            TryParseDataForNotificationId(value);
        }
    }

    private void TryParseDataForNotificationId(string encoded)
    {
        if (string.IsNullOrWhiteSpace(encoded)) return;
        try
        {
            var decoded = Uri.UnescapeDataString(encoded);
            using var doc = JsonDocument.Parse(decoded);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                if (doc.RootElement.TryGetProperty("notificationId", out var prop))
                {
                    _notificationIdFromQuery = prop.GetString();
                    return;
                }
                if (doc.RootElement.TryGetProperty("id", out var prop2))
                {
                    _notificationIdFromQuery = prop2.GetString();
                    return;
                }
                if (doc.RootElement.TryGetProperty("notification_id", out var prop3))
                {
                    _notificationIdFromQuery = prop3.GetString();
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [NotifictionPage] Failed to parse data query: {ex.Message}");
        }
    }

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

		if (BindingContext is NotificationViewModel viewModel)
		{
			if (viewModel.Notifications.Count == 0)
			{
				await viewModel.LoadNotificationsCommand.ExecuteAsync(null);
			}

			// If page was opened with a notificationId (from push tap), mark it as read via the VM
			if (!string.IsNullOrWhiteSpace(_notificationIdFromQuery))
			{
				try
				{
					await viewModel.MarkNotificationAsReadAsync(_notificationIdFromQuery);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"❌ [NotifictionPage] MarkNotificationAsReadAsync failed: {ex.Message}");
				}
				finally
				{
					_notificationIdFromQuery = null; // prevent repeat
				}
			}
			else
			{
				// ✅ عند الدخول للصفحة بدون notificationId محدد
				// قم بتحديث عداد الإشعارات (الذي سيؤدي إلى اختفاء البدج)
				Console.WriteLine("📄 [NotifictionPage] Refreshing notification count on page appearing");
				try
				{
					await viewModel.RefreshNotificationsCommand.ExecuteAsync(null);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"❌ [NotifictionPage] Refresh failed: {ex.Message}");
				}
			}
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
