using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using loukupm.Model;
using loukupm.Services;
using loukupm.ViewModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using System.Collections.Specialized;
using System.Linq;
using static Android.Provider.ContactsContract.CommonDataKinds;

namespace loukupm.View;

public partial class TerminbuchenPage : ContentPage
{
    private Dictionary<int, object> _serviceProviderPickers = new();

    public TerminbuchenPage()
    {
        InitializeComponent();
        this.InitializeLanguageTracking();

        this.BindingContext = AppViewModel.Instance;

        Loaded += async (s, e) =>
        {
            await RefreshProviderSelectorsAsync();
            await LoadReminderOptionsAsync();

            // Debug logs to help diagnose empty Picker issue
            try
            {
                var vm = AppViewModel.Instance;
                System.Diagnostics.Debug.WriteLine($"[TerminbuchenPage] After LoadReminderOptionsAsync: ReminderOptions.Count = {vm?.ReminderOptions?.Count ?? 0}");
                if (vm?.ReminderOptions != null)
                {
                    for (int i = 0; i < vm.ReminderOptions.Count; i++)
                    {
                        var opt = vm.ReminderOptions[i];
                        System.Diagnostics.Debug.WriteLine($"[TerminbuchenPage] ReminderOption[{i}] Label=\"{opt?.Label}\" OffsetHours={opt?.OffsetHours}");
                    }
                }

                // Also call API directly to inspect raw response (debug only)
                try
                {
                    var api = new ApiServices();
                    var resp = await api.GetReminderOptionsAsync();
                    if (resp == null)
                    {
                        System.Diagnostics.Debug.WriteLine("[TerminbuchenPage] ApiServices.GetReminderOptionsAsync returned null");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[TerminbuchenPage] API response: Success={resp.Success}, OptionsCount={(resp.Data?.Options?.Count ?? 0)}, DefaultOffset={resp.Data?.DefaultOffsetHours}");
                        if (resp.Data?.Options != null)
                        {
                            for (int i = 0; i < resp.Data.Options.Count; i++)
                            {
                                var ro = resp.Data.Options[i];
                                System.Diagnostics.Debug.WriteLine($"[TerminbuchenPage] API Option[{i}] Label=\"{ro?.Label}\" OffsetHours={ro?.OffsetHours}");
                            }
                        }
                    }
                }
                catch (Exception apiex)
                {
                    System.Diagnostics.Debug.WriteLine($"[TerminbuchenPage] Error calling GetReminderOptionsAsync for debug: {apiex.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TerminbuchenPage] Debug logging failed: {ex.Message}");
            }
        };

        if (AppViewModel.Instance?.SelectedServices is INotifyCollectionChanged col)
        {
            col.CollectionChanged += async (s, e) =>
            {
                await RefreshProviderSelectorsAsync();
            };
        }
    }

    // ============================================================
    // REFRESH PROVIDER SELECTORS
    // ============================================================

    private async Task RefreshProviderSelectorsAsync()
    {
        try
        {
            var viewModel = this.BindingContext as AppViewModel;
            if (viewModel == null) return;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                ServiceProviderSelectorsContainer.Children.Clear();
                _serviceProviderPickers.Clear();
            });

            var services = viewModel.SelectedServices?.ToList();
            if (services == null || services.Count == 0) return;

            foreach (var service in services)
            {
                var row = CreateProviderSelectorRow(service);
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ServiceProviderSelectorsContainer.Children.Add(row);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TerminbuchenPage] Error refreshing provider selectors: {ex}");
        }
    }

    // ============================================================
    // CREATE PROVIDER SELECTOR ROW (MODERN POPUP TRIGGER)
    // ============================================================

    private Border CreateProviderSelectorRow(Servies service)
    {
        var viewModel = this.BindingContext as AppViewModel;

        var verticalStackLayout = new VerticalStackLayout
        {
            Spacing = 8
        };

        var container = new Border
        {
            Margin = new Thickness(8, 8, 8, 8),
            Padding = new Thickness(12),
            BackgroundColor = Color.FromArgb("#222222"),
            Stroke = Color.FromArgb("#444444"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Content = verticalStackLayout
        };

        // Service Name
        var serviceName = new Label
        {
            Text = service.NameServies,
            FontFamily = "Oswald",
            TextColor = Colors.White,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold
        };
        verticalStackLayout.Children.Add(serviceName);

        var daysContainer = new HorizontalStackLayout { Spacing = 8, Margin = new Thickness(0, 8, 0, 0) };
        var slotsContainer = new FlexLayout
        {
            Direction = FlexDirection.Row,
            Wrap = FlexWrap.Wrap,
            JustifyContent = FlexJustify.Start,
            Margin = new Thickness(0, 8, 0, 0)
        };

        var providers = viewModel?.GetAlternativeProvidersForService(service.Id);
        WorkTeam selectedProviderInstance = null;

        if (viewModel?.CurrentBooking != null &&
            viewModel.CurrentBooking.ServiceProviderMap.TryGetValue(service.Id, out var savedProviderId))
        {
            selectedProviderInstance = providers?.FirstOrDefault(p => p.Id == savedProviderId);
        }

        // Modern Trigger Card (Looks like a modern select box with indicator icon)
        var dropdownTextLabel = new Label
        {
            Text = selectedProviderInstance != null ? selectedProviderInstance.Name : "Tap to select provider...",
            FontFamily = "Oswald",
            FontSize = 13,
            TextColor = selectedProviderInstance != null ? Color.FromArgb("#FFD700") : Color.FromArgb("#888888"),
            VerticalTextAlignment = TextAlignment.Center
        };

        var iconBorder = new Border
        {
            WidthRequest = 26,
            HeightRequest = 26,
            BackgroundColor = Color.FromArgb("#333333"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(13) },
            Content = new Label
            {
                Text = "⚙️",
                FontSize = 11,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var dropdownGrid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            Padding = new Thickness(12, 8)
        };
        dropdownGrid.Add(dropdownTextLabel, 0, 0);
        dropdownGrid.Add(iconBorder, 1, 0);

        var dropdownBox = new Border
        {
            BackgroundColor = Color.FromArgb("#2A2A2A"),
            Stroke = Color.FromArgb("#444444"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new RoundRectangle().CornerRadius },
            Content = dropdownGrid,
            HeightRequest = 48,
            Margin = new Thickness(0, 4, 0, 4)
        };

        // Modern Popup Trigger Action
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            if (providers == null || providers.Count == 0)
            {
                await DisplayAlert("Notice", "No providers available for this service.", "OK");
                return;
            }

            // Show Modern Popup
            var popup = new ProviderSelectionPopup(providers?.ToList(), selectedProviderInstance, service.NameServies);
            var result = await this.ShowPopupAsync(popup);

            if (result is WorkTeam chosenProvider)
            {
                selectedProviderInstance = chosenProvider;
                dropdownTextLabel.Text = chosenProvider.Name;
                dropdownTextLabel.TextColor = Color.FromArgb("#FFD700");

                OnServiceProviderSelected(service, chosenProvider);

                BuildDayButtons();
                slotsContainer.Children.Clear();

                if (viewModel?.CurrentBooking != null &&
                    viewModel.CurrentBooking.ServiceDateMap.TryGetValue(service.Id, out var existingDate))
                {
                    await LoadSlotsAsync(service, chosenProvider, existingDate, slotsContainer);
                }
            }
        };

        dropdownBox.GestureRecognizers.Add(tapGesture);

        void BuildDayButtons()
        {
            daysContainer.Children.Clear();
            var today = DateTime.Today;

            for (int i = 0; i < 7; i++)
            {
                var date = today.AddDays(i);
                var btn = new Button
                {
                    Text = date.ToString("dd\nddd"),
                    FontFamily = "Oswald",
                    FontSize = 12,
                    BackgroundColor = Color.FromArgb("#333333"),
                    TextColor = Colors.White,
                    CornerRadius = 8,
                    Padding = new Thickness(6),
                    HeightRequest = 50,
                    WidthRequest = 55,
                    Margin = new Thickness(0, 0, 4, 0)
                };

                if (viewModel?.CurrentBooking != null &&
                    viewModel.CurrentBooking.ServiceDateMap.TryGetValue(service.Id, out var savedDate) &&
                    savedDate.Date == date.Date)
                {
                    btn.BackgroundColor = Color.FromArgb("#FFD700");
                    btn.TextColor = Colors.Black;
                }

                btn.Clicked += async (s, e) =>
                {
                    foreach (var c in daysContainer.Children.OfType<Button>())
                    {
                        c.BackgroundColor = Color.FromArgb("#333333");
                        c.TextColor = Colors.White;
                    }

                    btn.BackgroundColor = Color.FromArgb("#FFD700");
                    btn.TextColor = Colors.Black;

                    if (selectedProviderInstance != null)
                    {
                        await LoadSlotsAsync(service, selectedProviderInstance, date, slotsContainer);
                    }
                    else
                    {
                        slotsContainer.Children.Clear();
                        slotsContainer.Children.Add(new Label
                        {
                            Text = "Please select a provider first",
                            TextColor = Color.FromArgb("#FF6666"),
                            FontFamily = "Oswald",
                            FontSize = 12
                        });
                    }
                };

                daysContainer.Children.Add(btn);
            }
        }

        if (selectedProviderInstance != null)
        {
            BuildDayButtons();
            if (viewModel?.CurrentBooking != null &&
                viewModel.CurrentBooking.ServiceDateMap.TryGetValue(service.Id, out var initDate))
            {
                _ = LoadSlotsAsync(service, selectedProviderInstance, initDate, slotsContainer);
            }
        }

        _serviceProviderPickers[service.Id] = dropdownBox;

        verticalStackLayout.Children.Add(dropdownBox);

        var daysScrollView = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            Content = daysContainer,
            Margin = new Thickness(0, 8, 0, 8)
        };

        verticalStackLayout.Children.Add(daysScrollView);
        verticalStackLayout.Children.Add(slotsContainer);

        return container;
    }

    // ============================================================
    // LOAD SLOTS
    // ============================================================

    private async Task LoadSlotsAsync(Servies service, WorkTeam selectedProvider, DateTime date, FlexLayout slotsContainer)
    {
        try
        {
            slotsContainer.Children.Clear();
            var api = new ApiServices();
            var avail = await api.GetProviderAvailabilityAsync(service.Id, selectedProvider.Id, date);

            if (avail?.Data?.AvailableSlots != null && avail.Data.AvailableSlots.Count > 0)
            {
                foreach (var slot in avail.Data.AvailableSlots)
                {
                    var slotButton = new Button
                    {
                        Text = slot.DisplayTime,
                        FontFamily = "Oswald",
                        FontSize = 12,
                        BackgroundColor = Color.FromArgb("#333333"),
                        TextColor = Colors.White,
                        CornerRadius = 8,
                        Padding = new Thickness(8, 6),
                        Margin = new Thickness(0, 0, 8, 8),
                        HeightRequest = 40
                    };

                    if (BindingContext is AppViewModel vm &&
                        vm.CurrentBooking != null &&
                        vm.CurrentBooking.ServiceSlotMap.TryGetValue(service.Id, out var savedSlot))
                    {
                        var formattedSavedSlot = Booking.FormatApiTime(slot.StartTime) ?? slot.StartTime;
                        if (string.Equals(savedSlot, formattedSavedSlot, StringComparison.OrdinalIgnoreCase))
                        {
                            slotButton.BackgroundColor = Color.FromArgb("#FFD700");
                            slotButton.TextColor = Colors.Black;
                        }
                    }

                    slotButton.Clicked += (ss, ee) =>
                    {
                        foreach (var c in slotsContainer.Children.OfType<Button>())
                        {
                            c.BackgroundColor = Color.FromArgb("#333333");
                            c.TextColor = Colors.White;
                        }

                        slotButton.BackgroundColor = Color.FromArgb("#FFD700");
                        slotButton.TextColor = Colors.Black;

                        if (BindingContext is AppViewModel viewModel && viewModel.CurrentBooking != null)
                        {
                            viewModel.CurrentBooking.EnsureMaps();
                            var formattedTime = Booking.FormatApiTime(slot.StartTime) ?? slot.StartTime;
                            viewModel.CurrentBooking.ServiceSlotMap[service.Id] = formattedTime;
                            viewModel.CurrentBooking.ServiceDateMap[service.Id] = date.Date;
                        }
                    };

                    slotsContainer.Children.Add(slotButton);
                }
            }
            else
            {
                slotsContainer.Children.Add(new Label
                {
                    Text = "No available slots",
                    TextColor = Color.FromArgb("#FF6666"),
                    FontFamily = "Oswald",
                    FontSize = 12,
                    Margin = new Thickness(0, 5)
                });
            }
        }
        catch (Exception ex)
        {
            slotsContainer.Children.Clear();
            slotsContainer.Children.Add(new Label
            {
                Text = "Error loading slots",
                TextColor = Color.FromArgb("#FF6666"),
                FontFamily = "Oswald",
                FontSize = 12
            });
        }
    }
   

    private void OnServiceProviderSelected(Servies service, WorkTeam provider)
    {
        try
        {
            var viewModel = this.BindingContext as AppViewModel;
            if (viewModel == null) return;

            viewModel.CurrentBooking ??= new Booking();
            viewModel.CurrentBooking.EnsureMaps();
            viewModel.CurrentBooking.RemoveServiceSelections(service.Id);
            viewModel.CurrentBooking.ServiceProviderMap[service.Id] = provider.Id;

            //MainThread.BeginInvokeOnMainThread(async () =>
            //{
            //    await Toast.Make($"Provider selected: {provider.Name}", ToastDuration.Short).Show();
            //});
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TerminbuchenPage] Error: {ex.Message}");
        }
    }

    // Navigation & Standard handlers
    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () => { await NavigationService.HandleBackButton(NavigationService.ROUTE_TERM_BOOKING); });
        return true;
    }

    private async void Button_Clicked(object sender, EventArgs e) => await NavigationService.HandleBackButton(NavigationService.ROUTE_TERM_BOOKING);
    private async void Button_Clicked_1(object sender, EventArgs e) { }
    private async void OnNext_Clicked(object sender, EventArgs e)
    {
        var vm = AppViewModel.Instance;
        vm.CurrentBooking ??= new Booking();
        vm.CurrentBooking.EnsureMaps();
        if (vm.SelectedProvider != null) vm.CurrentBooking.ProviderId = vm.SelectedProvider.Id.ToString();
        if (!vm.CurrentBooking.HasPerServiceSelections || vm.CurrentBooking.ServiceDateMap.Count == 0) vm.CurrentBooking.Date = vm.SelectedDate;
        if (vm.SelectedSlot != null) vm.CurrentBooking.Time = vm.SelectedTime;
        await NavigationService.NavigateToTabBarPage(NavigationService.ROUTE_BOOKING);
    }
    private async void Button_Clicked_2(object sender, EventArgs e) => await Navigation.PopAsync();

    private void OnRemoveService(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Servies service)
        {
            var viewModel = this.BindingContext as AppViewModel;
            viewModel?.RemoveSelectedService(service);
            viewModel?.CurrentBooking?.RemoveServiceSelections(service.Id);
            _ = RefreshProviderSelectorsAsync();
        }
    }

    private async Task LoadReminderOptionsAsync()
    {
        try
        {
            var api = new ApiServices();
            var resp = await api.GetReminderOptionsAsync();

            // Debug: inspect raw response
            System.Diagnostics.Debug.WriteLine($"[TerminbuchenPage] GetReminderOptionsAsync returned: {(resp == null ? "null" : "non-null")}");
            if (resp != null)
            {
                System.Diagnostics.Debug.WriteLine($"[TerminbuchenPage] resp.Success={resp.Success}, Data present={(resp.Data != null)}");
                if (resp.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[TerminbuchenPage] resp.Data.DefaultOffsetHours={resp.Data.DefaultOffsetHours}, optionsCount={(resp.Data.Options?.Count ?? 0)}");
                    if (resp.Data.Options != null)
                    {
                        for (int i = 0; i < resp.Data.Options.Count; i++)
                        {
                            var ro = resp.Data.Options[i];
                            System.Diagnostics.Debug.WriteLine($"[TerminbuchenPage] Resp Option[{i}] Label=\"{ro?.Label}\" OffsetHours={ro?.OffsetHours}");
                        }
                    }
                }
            }

            if (resp != null && resp.Success && resp.Data != null)
            {
                var vm = AppViewModel.Instance;
                var options = resp.Data.Options ?? new System.Collections.Generic.List<ReminderOption>();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    vm.ReminderOptions.Clear();
                    foreach (var opt in options)
                        vm.ReminderOptions.Add(opt);

                    // Select default if present
                    int defaultIndex = options.FindIndex(o => o.OffsetHours == resp.Data.DefaultOffsetHours);
                    vm.SelectedReminderOptionIndex = defaultIndex >= 0 ? defaultIndex : 0;

                    // Debug: confirm UI-bound collection updated
                    System.Diagnostics.Debug.WriteLine($"[TerminbuchenPage] VM.ReminderOptions.Count after update = {vm.ReminderOptions.Count}");
                    for (int i = 0; i < vm.ReminderOptions.Count; i++)
                    {
                        var opt = vm.ReminderOptions[i];
                        System.Diagnostics.Debug.WriteLine($"[TerminbuchenPage] VM Option[{i}] Label=\"{opt?.Label}\" OffsetHours={opt?.OffsetHours}");
                    }
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[TerminbuchenPage] No reminder options to display (resp null or unsuccessful)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TerminbuchenPage] LoadReminderOptionsAsync failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[TerminbuchenPage] LoadReminderOptionsAsync failed: {ex.Message}");
        }
    }
}

// ============================================================
// MODERN POPUP CLASS FOR PROVIDER SELECTION
// ============================================================

public class ProviderSelectionPopup : Popup
{
    public ProviderSelectionPopup(List<WorkTeam> providers, WorkTeam currentSelected, string serviceName)
    {
        this.Color = Microsoft.Maui.Graphics.Colors.Transparent;
        CanBeDismissedByTappingOutsideOfPopup = true;

        var stackLayout = new VerticalStackLayout
        {
           
            Spacing = 12,
            Padding = new Thickness(20)
        };

        // Title Header
        stackLayout.Children.Add(new Label
        {
            Text = $"Select Provider\n{serviceName}",
            FontFamily = "Oswald",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FFD700"),
            HorizontalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        });

        var scrollView = new ScrollView
        {
            MaximumHeightRequest = 250
        };

        var optionsList = new VerticalStackLayout { Spacing = 8 };

        foreach (var provider in providers)
        {
            bool isSelected = currentSelected?.Id == provider.Id;

            var itemBorder = new Border
            {
                Padding = new Thickness(14, 12),
                BackgroundColor = isSelected ? Color.FromArgb("#332E14") : Color.FromArgb("#2A2A2A"),
                Stroke = isSelected ? Color.FromArgb("#FFD700") : Color.FromArgb("#444444"),
                StrokeThickness = isSelected ? 2 : 1,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
                Content = new Label
                {
                    Text = provider.Name,
                    FontFamily = "Oswald",
                    FontSize = 14,
                    TextColor = isSelected ? Color.FromArgb("#FFD700") : Colors.White,
                    FontAttributes = isSelected ? FontAttributes.Bold : FontAttributes.None
                }
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                Close(provider); // إرجاع المزود المختار وإغلاق النافذة المنبثقة
            };
            itemBorder.GestureRecognizers.Add(tap);

            optionsList.Children.Add(itemBorder);
        }

        scrollView.Content = optionsList;
        stackLayout.Children.Add(scrollView);

        // Cancel Button
        var cancelBtn = new Button
        {
            Text = "Cancel",
            FontFamily = "Oswald",
            FontSize = 12,
            BackgroundColor = Color.FromArgb("#333333"),
            TextColor = Colors.White,
            CornerRadius = 8,
            HeightRequest = 40,
            Margin = new Thickness(0, 10, 0, 0)
        };
        cancelBtn.Clicked += (s, e) => Close(null);
        stackLayout.Children.Add(cancelBtn);

        Content = new Border
        {
            BackgroundColor = Color.FromArgb("#1E1E1E"),
            Stroke = Color.FromArgb("#444444"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            WidthRequest = 300,
            Padding = new Thickness(0),
            Content = stackLayout
        };
    }
}