using loukupm.Model;
using loukupm.services;
using loukupm.Services;
using loukupm.ViewModel;
using System.Text.Json;

namespace loukupm.View;

[QueryProperty(nameof(Data), "data")]
public partial class OTPSINGIN : ContentPage
{
	private readonly ApiServices _apiServices = new();
	private Auth.OtpContext _otpContext;
	private CancellationTokenSource _timerCancellation;
	private int _remainingSeconds = 60;
	private const int RESEND_COUNTDOWN = 60;

	/// <summary>
	/// QueryProperty for receiving serialized OtpContext from navigation.
	/// </summary>
	public string Data
	{
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				try
				{
					// Decode URL-encoded JSON
					string decodedJson = Uri.UnescapeDataString(value);
					_otpContext = JsonSerializer.Deserialize<Auth.OtpContext>(decodedJson, 
						new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

					Console.WriteLine($"✅ [OTPSINGIN] OtpContext deserialized from query parameter: {_otpContext?.MaskedDestination}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"❌ [OTPSINGIN] Failed to deserialize OtpContext: {ex.Message}");
					_otpContext = null;
				}
			}
		}
	}

	public OTPSINGIN()
	{
		try
		{
			InitializeComponent();
			Console.WriteLine("✅ [OTPSINGIN] Page initialized successfully");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"❌ [OTPSINGIN] Initialization failed: {ex.Message}");
			throw;
		}
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);

		try
		{
			// Load OTP context from BindingContext or query parameters
			if (BindingContext is Auth.OtpContext context)
			{
				_otpContext = context;
				InitializePageWithContext();
			}
			else
			{
				Console.WriteLine("⚠️ [OTPSINGIN] No OTP context provided");
				DisplayError("No verification context found. Please try again.");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"❌ [OTPSINGIN] OnNavigatedTo error: {ex.Message}");
		}
	}

	private void InitializePageWithContext()
	{
		try
		{
			if (_otpContext == null)
			{
				Console.WriteLine("❌ [OTPSINGIN] OTP context is null");
				return;
			}

			Console.WriteLine($"📱 [OTPSINGIN] Initializing with: {_otpContext.MaskedDestination}");

			// Display masked destination
			MainThread.BeginInvokeOnMainThread(() =>
			{
				MaskedDestinationLabel.Text = _otpContext.MaskedDestination ?? "your email/phone";
				OtpEntry.Focus();
			});
		}
		catch (Exception ex)
		{
			Console.WriteLine($"❌ [OTPSINGIN] Context initialization error: {ex.Message}");
		}
	}

	private void OnOtpTextChanged(object sender, TextChangedEventArgs e)
	{
		try
		{
			string otp = e.NewTextValue ?? "";

			// Enable verify button only when 6 digits are entered
			MainThread.BeginInvokeOnMainThread(() =>
			{
				VerifyButton.IsEnabled = otp.Length == 6 && otp.All(c => char.IsDigit(c));

				// Clear error when user starts typing
				if (!string.IsNullOrEmpty(otp) && ErrorMessageLabel.IsVisible)
				{
					ErrorMessageLabel.IsVisible = false;
				}
			});
		}
		catch (Exception ex)
		{
			Console.WriteLine($"⚠️ [OTPSINGIN] Text changed error: {ex.Message}");
		}
	}

	private async void OnVerifyClicked(object sender, EventArgs e)
	{
		try
		{
			string otp = OtpEntry.Text?.Trim() ?? "";

			if (string.IsNullOrWhiteSpace(otp) || otp.Length != 6)
			{
				DisplayError("Please enter a valid 6-digit code");
				return;
			}

			if (!otp.All(c => char.IsDigit(c)))
			{
				DisplayError("OTP must contain only digits");
				return;
			}

			// Check network connectivity
			if (Connectivity.NetworkAccess != NetworkAccess.Internet)
			{
				DisplayError("No internet connection. Please check your network.");
				return;
			}

			await VerifyOtpWithApi(otp);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"❌ [OTPSINGIN] Verify clicked error: {ex.Message}");
			DisplayError("An error occurred. Please try again.");
		}
	}

	private async Task VerifyOtpWithApi(string otp)
	{
		try
		{
			// Show loading
			ShowLoading(true);

			Console.WriteLine($"🔐 [OTPSINGIN] Verifying OTP: {otp}");

			// Validate OTP context exists
			if (_otpContext == null)
			{
				Console.WriteLine("❌ [OTPSINGIN] OTP context is not initialized");
				DisplayError("Verification context missing. Please try again.");
				MainThread.BeginInvokeOnMainThread(() => { VerifyButton.IsEnabled = true; });
				return;
			}

			// Call API to verify OTP
			var (success, accessToken, refreshToken, user, statusCode, errorMessage) = 
				await _apiServices.VerifyOtpAsync(
					email: _otpContext.Email,
					phone: _otpContext.Phone,
					otp: otp,
					registrationMethod: _otpContext.RegistrationMethod);

			if (success)
			{
				Console.WriteLine($"✅ [OTPSINGIN] OTP verified successfully for user: {user?.Email ?? "unknown"}");

				// Validate API response contains required tokens
				if (string.IsNullOrEmpty(accessToken))
				{
					Console.WriteLine("❌ [OTPSINGIN] Access token is null or empty in API response");
					DisplayError("Verification incomplete. Please try again.");
					MainThread.BeginInvokeOnMainThread(() => { VerifyButton.IsEnabled = true; });
					return;
				}

				// Save tokens to secure storage ONLY after successful verification
				try
				{
					await SecureStorage.SetAsync("auth_token", accessToken);
					Console.WriteLine("✅ [OTPSINGIN] Auth token saved");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"❌ [OTPSINGIN] Failed to save auth token: {ex.Message}");
					DisplayError("Failed to save authentication. Please try again.");
					MainThread.BeginInvokeOnMainThread(() => { VerifyButton.IsEnabled = true; });
					return;
				}

				if (!string.IsNullOrEmpty(refreshToken))
				{
					try
					{
						await SecureStorage.SetAsync("refresh_token", refreshToken);
						Console.WriteLine("✅ [OTPSINGIN] Refresh token saved");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"⚠️ [OTPSINGIN] Failed to save refresh token: {ex.Message}");
						// Continue anyway as refresh token is optional for initial login
					}
				}

				// Load user data with error handling
				Console.WriteLine("📥 [OTPSINGIN] Loading user data...");
				try
				{
					// Verify AppViewModel instance exists
					if (AppViewModel.Instance == null)
					{
						Console.WriteLine("❌ [OTPSINGIN] AppViewModel.Instance is null");
						DisplayError("Application initialization failed. Please try again.");
						MainThread.BeginInvokeOnMainThread(() => { VerifyButton.IsEnabled = true; });
						return;
					}

					await AppViewModel.Instance.LoadUserDataAsync();
					Console.WriteLine("✅ [OTPSINGIN] User data loaded successfully");
				}
				catch (NullReferenceException nex)
				{
					Console.WriteLine($"❌ [OTPSINGIN] NullReferenceException during LoadUserDataAsync: {nex.Message}");
					Console.WriteLine($"   Stack: {nex.StackTrace}");
					DisplayError("Failed to load user data. Please try again.");
					MainThread.BeginInvokeOnMainThread(() => { VerifyButton.IsEnabled = true; });
					return;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"❌ [OTPSINGIN] Exception during LoadUserDataAsync: {ex.Message}");
					Console.WriteLine($"   Stack: {ex.StackTrace}");
					DisplayError("Failed to load user data. Please try again.");
					MainThread.BeginInvokeOnMainThread(() => { VerifyButton.IsEnabled = true; });
					return;
				}

				// Navigate to Home and clear stack
				Console.WriteLine("🏠 [OTPSINGIN] Navigating to Home...");
				try
				{
					await ShellNavigationManager.NavigateToHomeAndClear();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"❌ [OTPSINGIN] Navigation failed: {ex.Message}");
					DisplayError("Navigation failed. Please try again.");
					MainThread.BeginInvokeOnMainThread(() => { VerifyButton.IsEnabled = true; });
				}
			}
			else
			{
				Console.WriteLine($"❌ [OTPSINGIN] OTP verification failed: {statusCode} - {errorMessage}");

				// Handle different error scenarios
				string errorMsg = statusCode switch
				{
					422 => "Invalid verification code. Please try again.",
					403 => "Account not verified or OTP expired. Please request a new code.",
					429 => "Too many attempts. Please try again later.",
					0 => "Network error. Please check your connection.",
					_ => errorMessage ?? "Verification failed. Please try again."
				};

				DisplayError(errorMsg);

				MainThread.BeginInvokeOnMainThread(() =>
				{
					VerifyButton.IsEnabled = true;
				});
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"❌ [OTPSINGIN] Exception during OTP verification: {ex.Message}");
			Console.WriteLine($"   Stack: {ex.StackTrace}");
			Console.WriteLine($"   Type: {ex.GetType().Name}");
			DisplayError("An unexpected error occurred. Please try again.");

			MainThread.BeginInvokeOnMainThread(() =>
			{
				VerifyButton.IsEnabled = true;
			});
		}
		finally
		{
			ShowLoading(false);
		}
	}

	private async void OnResendClicked(object sender, EventArgs e)
	{
		try
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				ResendButton.IsEnabled = false;
			});

			if (Connectivity.NetworkAccess != NetworkAccess.Internet)
			{
				DisplayError("No internet connection. Please check your network.");
				MainThread.BeginInvokeOnMainThread(() =>
				{
					ResendButton.IsEnabled = true;
				});
				return;
			}

			Console.WriteLine($"📤 [OTPSINGIN] Requesting OTP resend...");

			var (success, statusCode, message, resendAfter) = 
				await _apiServices.ResendOtpAsync(
					email: _otpContext.Email,
					phone: _otpContext.Phone,
					registrationMethod: _otpContext.RegistrationMethod);

			if (success)
			{
				Console.WriteLine($"✅ [OTPSINGIN] OTP resent successfully");
				string dest = _otpContext.RegistrationMethod == "phone" ? "phone" : "email";
				DisplayMessage($"New code sent! Check your {dest}");

				// Start countdown timer
				await StartResendCountdown();
			}
			else
			{
				Console.WriteLine($"❌ [OTPSINGIN] Resend failed: {statusCode} - {message}");

				string errorMsg = statusCode switch
				{
					429 => "Too many resend attempts. Please wait before trying again.",
					_ => message ?? "Failed to resend code. Please try again later."
				};

				DisplayError(errorMsg);

				MainThread.BeginInvokeOnMainThread(() =>
				{
					ResendButton.IsEnabled = true;
				});
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"❌ [OTPSINGIN] Exception during resend: {ex.Message}");
			DisplayError("An error occurred while resending the code.");

			MainThread.BeginInvokeOnMainThread(() =>
			{
				ResendButton.IsEnabled = true;
			});
		}
	}

	private async Task StartResendCountdown()
	{
		try
		{
			_timerCancellation?.Cancel();
			_timerCancellation = new CancellationTokenSource();
			_remainingSeconds = RESEND_COUNTDOWN;

			MainThread.BeginInvokeOnMainThread(() =>
			{
				ResendButton.IsVisible = false;
				TimerLabel.IsVisible = true;
				TimerLabel.Text = $"{_remainingSeconds}s";
			});

			while (_remainingSeconds > 0 && !_timerCancellation.Token.IsCancellationRequested)
			{
				await Task.Delay(1000, _timerCancellation.Token);
				_remainingSeconds--;

				MainThread.BeginInvokeOnMainThread(() =>
				{
					TimerLabel.Text = $"{_remainingSeconds}s";
				});
			}

			// Timer finished, show resend button again
			if (!_timerCancellation.Token.IsCancellationRequested)
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
					TimerLabel.IsVisible = false;
					ResendButton.IsVisible = true;
					ResendButton.IsEnabled = true;
				});
			}
		}
		catch (OperationCanceledException)
		{
			Console.WriteLine("ℹ️ [OTPSINGIN] Countdown cancelled");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"❌ [OTPSINGIN] Countdown error: {ex.Message}");
		}
	}

	private void OnBackClicked(object sender, EventArgs e)
	{
		try
		{
			// Cancel any ongoing timer
			_timerCancellation?.Cancel();

			// Navigate back
			MainThread.BeginInvokeOnMainThread(async () =>
			{
				try
				{
					if (Shell.Current?.Navigation.NavigationStack.Count > 1)
					{
						await Shell.Current.GoToAsync("..");
					}
					else
					{
						// If no previous page, go to login
						await ShellNavigationManager.NavigateToLoginAndClear();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"⚠️ [OTPSINGIN] Navigation error: {ex.Message}");
				}
			});
		}
		catch (Exception ex)
		{
			Console.WriteLine($"❌ [OTPSINGIN] Back clicked error: {ex.Message}");
		}
	}

	protected override bool OnBackButtonPressed()
	{
		MainThread.BeginInvokeOnMainThread(() => OnBackClicked(null, null));
		return true;
	}

	private void DisplayError(string message)
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			try
			{
				ErrorMessageLabel.Text = message;
				ErrorMessageLabel.IsVisible = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"⚠️ [OTPSINGIN] DisplayError failed: {ex.Message}");
			}
		});
	}

	private void DisplayMessage(string message)
	{
		MainThread.BeginInvokeOnMainThread(async () =>
		{
			try
			{
				await DisplayAlert("Success", message, "OK");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"⚠️ [OTPSINGIN] DisplayMessage failed: {ex.Message}");
			}
		});
	}

	private void ShowLoading(bool show)
	{
		MainThread.BeginInvokeOnMainThread(() =>
		{
			try
			{
				LoadingGrid.IsVisible = show;
				LoadingIndicator.IsRunning = show;
				VerifyButton.IsVisible = !show;
				ResendButton.IsEnabled = !show;
				OtpEntry.IsEnabled = !show;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"⚠️ [OTPSINGIN] ShowLoading failed: {ex.Message}");
			}
		});
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		try
		{
			// Cancel timer when page disappears
			_timerCancellation?.Cancel();
			Console.WriteLine("ℹ️ [OTPSINGIN] Page disappearing, timer cancelled");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"⚠️ [OTPSINGIN] OnDisappearing error: {ex.Message}");
		}
	}
}

