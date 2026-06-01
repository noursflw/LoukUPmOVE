namespace loukupm.View;
using loukupm.Services;
using loukupm.Langue;
using System.Globalization;
using System.Windows.Input;
using System.ComponentModel;
using OneSignalSDK.DotNet;

public partial class SettingPage : ContentPage, INotifyPropertyChanged
{
	private List<LanguageOption> _languages;
	private ICommand _selectedLanguageChangedCommand;

	public event PropertyChangedEventHandler PropertyChanged;

	public SettingPage()
	{
		InitializeComponent();
		InitializeLanguages();
		_selectedLanguageChangedCommand = new Command<object>(OnLanguageSelected);
		BindingContext = this;
	}

	private void InitializeLanguages()
	{
		_languages = new List<LanguageOption>
		{
			
			new LanguageOption { Name = "Deutsch", CultureName = "de-DE" },
			new LanguageOption { Name = "العربية", CultureName = "ar-AR" }
		};
	}

	public List<LanguageOption> Languages => _languages;

	public ICommand SelectedLanguageChangedCommand => _selectedLanguageChangedCommand;

	public LanguageOption SelectedLanguage
	{
		get
		{
			var savedCulture = Preferences.Get("AppLanguage", "de-DE");
			return _languages.FirstOrDefault(l => l.CultureName == savedCulture) ?? _languages[1];
		}
		set
		{
			if (value != null)
			{
				var currentSaved = Preferences.Get("AppLanguage", "de-DE");
				if (value.CultureName != currentSaved)
				{
					ChangeLanguage(value.CultureName);
					OnPropertyChanged(nameof(SelectedLanguage));
				}
			}
		}
	}

	private void OnLanguageSelected(object parameter)
	{
		if (parameter is LanguageOption languageOption)
		{
			ChangeLanguage(languageOption.CultureName);
			OnPropertyChanged(nameof(SelectedLanguage));
		}
	}

	private void ChangeLanguage(string cultureName)
	{
		try
		{
			var newCulture = new CultureInfo(cultureName);
			Preferences.Set("AppLanguage", newCulture.Name);
			LocalizationResourcesManager.Instanse.SetCulture(newCulture);
			Console.WriteLine($"🌍 Language Changed to {newCulture.DisplayName}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[SettingPage] Language change error: {ex.Message}");
		}
	}

	protected void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public class LanguageOption
	{
		public string Name { get; set; }
		public string CultureName { get; set; }

		public override string ToString() => Name;
	}

	/// <summary>
	/// معالج زر العودة - يستخدم نظام الملاحة المركزي
	/// يتبع القاعدة: صفحات تدفق الملف الشخصي → //ProfilePage مباشرة
	/// </summary>
	protected override bool OnBackButtonPressed()
	{
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(NavigationService.ROUTE_HOME);
        });
        return true;
	}

	
	private async void Button_Clicked_1(object sender, EventArgs e)
	{
		await NavigationService.HandleBackButton(NavigationService.ROUTE_SETTING);
	}

	/// <summary>
	/// OnAppearing - Called when the page is about to appear.
	/// Loads the saved notification preference and sets the Switch state.
	/// </summary>
	protected override void OnAppearing()
	{
		base.OnAppearing();
		try
		{
			// Load saved notification preference (default: true/enabled)
			bool isNotificationsEnabled = Preferences.Get("NotificationsEnabled", true);
			NotificationsSwitch.IsToggled = isNotificationsEnabled;
			Console.WriteLine($"🔔 Loaded notification preference: {isNotificationsEnabled}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[SettingPage] Error loading notification preference: {ex.Message}");
		}
	}

	/// <summary>
	/// OnNotificationsSwitchToggled - Called when the notifications switch is toggled.
	/// Saves the preference and applies the OptIn/OptOut state to OneSignal.
	/// </summary>
	private async void OnNotificationsSwitchToggled(object sender, ToggledEventArgs e)
	{
		try
		{
			bool isEnabled = e.Value;

			// Save preference to device storage
			Preferences.Set("NotificationsEnabled", isEnabled);
			Console.WriteLine($"💾 Notification preference saved: {isEnabled}");

			// Apply the preference to OneSignal
			if (isEnabled)
			{
				// Enable notifications
				OneSignal.User.PushSubscription.OptIn();
				Console.WriteLine("✅ Notifications OptIn triggered");
			}
			else
			{
				// Disable notifications
				OneSignal.User.PushSubscription.OptOut();
				Console.WriteLine("🔕 Notifications OptOut triggered");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[SettingPage] Error toggling notifications: {ex.Message}");
			// Revert the switch to the previous state if an error occurs
			NotificationsSwitch.IsToggled = !e.Value;
		}
	}
}