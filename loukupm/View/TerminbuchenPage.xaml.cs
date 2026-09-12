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

namespace loukupm.View;

public partial class TerminbuchenPage : ContentPage
{
    private Dictionary<int, Picker> _serviceProviderPickers = new();

    public TerminbuchenPage()
    {
        InitializeComponent();
        this.InitializeLanguageTracking();

        this.BindingContext = AppViewModel.Instance;

        Loaded += async (s, e) =>
        {
            await RefreshProviderSelectorsAsync();
            await LoadReminderOptionsAsync();
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

            if (viewModel == null)
                return;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                ServiceProviderSelectorsContainer.Children.Clear();
                _serviceProviderPickers.Clear();
            });

            var services = viewModel.SelectedServices?.ToList();

            if (services == null || services.Count == 0)
                return;

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
            Console.WriteLine(
                $"[TerminbuchenPage] Error refreshing provider selectors: {ex}");
        }
    }

    // ============================================================
    // CREATE PROVIDER SELECTOR ROW
    // ============================================================

    private Border CreateProviderSelectorRow(Servies service)
    {
        var viewModel = this.BindingContext as AppViewModel;

        var verticalStackLayout = new VerticalStackLayout
        {
            Spacing = 6
        };

        var container = new Border
        {
            Margin = new Thickness(8, 8, 8, 8),
            Padding = new Thickness(12),
            BackgroundColor = Color.FromArgb("#222222"),
            Stroke = Color.FromArgb("#444444"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(8)
            },
            Content = verticalStackLayout
        };

        // ========================================================
        // SERVICE NAME
        // ========================================================

        var serviceName = new Label
        {
            Text = service.NameServies,
            FontFamily = "Oswald",
            TextColor = Colors.White,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold
        };

        verticalStackLayout.Children.Add(serviceName);

        // ========================================================
        // PROVIDER PICKER
        // ========================================================

        var picker = new Picker
        {
            Title = "Select provider",
            BackgroundColor = Color.FromArgb("#333333"),
            TextColor = Colors.White,
            FontFamily = "Oswald",
            FontSize = 13,
            HeightRequest = 40,
            ItemDisplayBinding = new Binding("Name")
        };

        var pickerBorder = new Border
        {
            Stroke = Color.FromArgb("#444444"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(8)
            },
            Padding = new Thickness(0),
            Content = picker
        };

        // ========================================================
        // DAYS CONTAINER
        // ========================================================

        var daysContainer = new HorizontalStackLayout
        {
            Spacing = 8,
            Margin = new Thickness(0, 8, 0, 0)
        };

        // ========================================================
        // SLOTS CONTAINER
        //
        // FlexLayout supports Wrap.
        // RowGap / ColumnGap are intentionally NOT used because
        // they are not available in the MAUI version being used.
        // ========================================================

        var slotsContainer = new FlexLayout
        {
            Direction = FlexDirection.Row,
            Wrap = FlexWrap.Wrap,
            JustifyContent = FlexJustify.Start,
            AlignItems = FlexAlignItems.Start,
            AlignContent = FlexAlignContent.Start,
            Margin = new Thickness(0, 8, 0, 0)
        };

        // ========================================================
        // GET PROVIDERS
        // ========================================================

        var providers =
            viewModel?.GetAlternativeProvidersForService(service.Id);

        if (providers != null && providers.Count > 0)
        {
            picker.ItemsSource = providers;

            // ====================================================
            // RESTORE PREVIOUS PROVIDER
            // ====================================================

            if (viewModel.CurrentBooking != null &&
                viewModel.CurrentBooking.ServiceProviderMap.TryGetValue(
                    service.Id,
                    out var providerId))
            {
                var existing =
                    providers.FirstOrDefault(
                        p => p.Id == providerId);

                if (existing != null)
                {
                    picker.SelectedItem = existing;
                }
            }
        }
        else
        {
            var noProvidersLabel = new Label
            {
                Text = "No provider available",
                TextColor = Color.FromArgb("#FF6666"),
                FontFamily = "Oswald",
                FontSize = 12,
                Margin = new Thickness(0, 4, 0, 4)
            };

            verticalStackLayout.Children.Add(
                noProvidersLabel);
        }

        // ========================================================
        // BUILD DAY BUTTONS
        // ========================================================

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

                btn.Clicked += async (s, e) =>
                {
                    // =================================================
                    // RESET DAY BUTTONS
                    // =================================================

                    foreach (
                        var c in daysContainer.Children.OfType<Button>())
                    {
                        c.BackgroundColor =
                            Color.FromArgb("#333333");

                        c.TextColor = Colors.White;
                    }

                    btn.BackgroundColor =
                        Color.FromArgb("#FFD700");

                    btn.TextColor = Colors.Black;

                    Console.WriteLine(
                        $"[TerminbuchenPage] Date selected for service " +
                        $"{service.Id}: {date:yyyy-MM-dd}");

                    // =================================================
                    // CHECK PROVIDER
                    // =================================================

                    if (picker.SelectedItem is WorkTeam selectedProvider)
                    {
                        await LoadSlotsAsync(
                            service,
                            selectedProvider,
                            date,
                            slotsContainer);
                    }
                    else
                    {
                        slotsContainer.Children.Clear();

                        slotsContainer.Children.Add(
                            new Label
                            {
                                Text = "Please select a provider first",
                                TextColor =
                                    Color.FromArgb("#FF6666"),
                                FontFamily = "Oswald",
                                FontSize = 12
                            });
                    }
                };

                daysContainer.Children.Add(btn);
            }
        }

        // ========================================================
        // PROVIDER SELECTION CHANGED
        // ========================================================

        picker.SelectedIndexChanged += async (s, e) =>
        {
            if (picker.SelectedItem is WorkTeam selectedProvider)
            {
                OnServiceProviderSelected(
                    service,
                    selectedProvider);

                BuildDayButtons();

                slotsContainer.Children.Clear();

                Console.WriteLine(
                    $"[TerminbuchenPage] Provider changed for " +
                    $"service {service.Id}: " +
                    $"{selectedProvider.Name}");

                // ====================================================
                // RESTORE PREVIOUS DATE
                // ====================================================

                if (viewModel?.CurrentBooking != null &&
                    viewModel.CurrentBooking.ServiceDateMap.TryGetValue(
                        service.Id,
                        out var existingDate))
                {
                    await LoadSlotsAsync(
                        service,
                        selectedProvider,
                        existingDate,
                        slotsContainer);
                }
            }
        };

        // ========================================================
        // STORE PICKER
        // ========================================================

        _serviceProviderPickers[service.Id] = picker;

        // ========================================================
        // ADD CONTROLS
        // ========================================================

        verticalStackLayout.Children.Add(
            pickerBorder);

        // ========================================================
        // WRAP DAYS CONTAINER IN SCROLL VIEW FOR HORIZONTAL SCROLLING
        // ========================================================

        var daysScrollView = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            Content = daysContainer,
            Margin = new Thickness(0, 8, 0, 8)
        };

        verticalStackLayout.Children.Add(
            daysScrollView);

        verticalStackLayout.Children.Add(
            slotsContainer);

        return container;
    }

    // ============================================================
    // LOAD SLOTS
    // ============================================================

    private async Task LoadSlotsAsync(
        Servies service,
        WorkTeam selectedProvider,
        DateTime date,
        FlexLayout slotsContainer)
    {
        try
        {
            slotsContainer.Children.Clear();

            Console.WriteLine(
                $"[TerminbuchenPage] Fetching availability " +
                $"for service {service.Id}, " +
                $"provider {selectedProvider.Id}, " +
                $"date {date:yyyy-MM-dd}");

            var api = new ApiServices();

            var avail =
                await api.GetProviderAvailabilityAsync(
                    service.Id,
                    selectedProvider.Id,
                    date);

            // ========================================================
            // CHECK SLOTS
            // ========================================================

            if (avail?.Data?.AvailableSlots != null &&
                avail.Data.AvailableSlots.Count > 0)
            {
                Console.WriteLine(
                    $"[TerminbuchenPage] Found " +
                    $"{avail.Data.AvailableSlots.Count} available slots");

                foreach (var slot in avail.Data.AvailableSlots)
                {
                    var slotButton = new Button
                    {
                        Text = slot.DisplayTime,
                        FontFamily = "Oswald",
                        FontSize = 12,
                        BackgroundColor =
                            Color.FromArgb("#333333"),
                        TextColor = Colors.White,
                        CornerRadius = 8,
                        Padding = new Thickness(8, 6),
                        Margin = new Thickness(0, 0, 8, 8),
                        HeightRequest = 40
                    };

                    // =================================================
                    // RESTORE SELECTED SLOT
                    // =================================================

                    if (BindingContext is AppViewModel vm &&
                        vm.CurrentBooking != null &&
                        vm.CurrentBooking.ServiceSlotMap.TryGetValue(
                            service.Id,
                            out var savedSlot))
                    {
                        var formattedSavedSlot =
                            Booking.FormatApiTime(
                                slot.StartTime)
                            ?? slot.StartTime;

                        if (string.Equals(
                            savedSlot,
                            formattedSavedSlot,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            slotButton.BackgroundColor =
                                Color.FromArgb("#FFD700");

                            slotButton.TextColor =
                                Colors.Black;
                        }
                    }

                    // =================================================
                    // SLOT CLICK
                    // =================================================

                    slotButton.Clicked += (ss, ee) =>
                    {
                        // =============================================
                        // RESET ALL SLOT BUTTONS
                        // =============================================

                        foreach (
                            var c in slotsContainer.Children
                                .OfType<Button>())
                        {
                            c.BackgroundColor =
                                Color.FromArgb("#333333");

                            c.TextColor =
                                Colors.White;
                        }

                        // =============================================
                        // SELECT CURRENT SLOT
                        // =============================================

                        slotButton.BackgroundColor =
                            Color.FromArgb("#FFD700");

                        slotButton.TextColor =
                            Colors.Black;

                        Console.WriteLine(
                            $"[TerminbuchenPage] " +
                            $"Slot selected for service {service.Id}: " +
                            $"{slot.DisplayTime} " +
                            $"(StartTime: {slot.StartTime})");

                        // =============================================
                        // SAVE TO BOOKING
                        // =============================================

                        if (BindingContext is AppViewModel viewModel &&
                            viewModel.CurrentBooking != null)
                        {
                            viewModel.CurrentBooking.EnsureMaps();

                            var formattedTime =
                                Booking.FormatApiTime(
                                    slot.StartTime)
                                ?? slot.StartTime;

                            // ==========================================
                            // SERVICE SLOT
                            // ==========================================

                            viewModel.CurrentBooking
                                .ServiceSlotMap[service.Id] =
                                formattedTime;

                            // ==========================================
                            // SERVICE DATE
                            // ==========================================

                            viewModel.CurrentBooking
                                .ServiceDateMap[service.Id] =
                                date.Date;

                            // ==========================================
                            // DEBUG
                            // ==========================================

                            Console.WriteLine(
                                $"[TerminbuchenPage] " +
                                $"Service {service.Id} slot stored: " +
                                $"{formattedTime} " +
                                $"on {date:yyyy-MM-dd}");

                            Console.WriteLine(
                                $"[TerminbuchenPage] " +
                                $"ServiceSlotMap now contains: " +
                                $"{viewModel.CurrentBooking.ServiceSlotMap.Count} entries");

                            Console.WriteLine(
                                $"[TerminbuchenPage] " +
                                $"ServiceDateMap now contains: " +
                                $"{viewModel.CurrentBooking.ServiceDateMap.Count} entries");

                            foreach (
                                var kvp in viewModel.CurrentBooking.ServiceDateMap)
                            {
                                Console.WriteLine(
                                    $"  [DATE_MAP] " +
                                    $"Service {kvp.Key} -> " +
                                    $"{kvp.Value:yyyy-MM-dd}");
                            }

                            foreach (
                                var kvp in viewModel.CurrentBooking.ServiceSlotMap)
                            {
                                Console.WriteLine(
                                    $"  [SLOT_MAP] " +
                                    $"Service {kvp.Key} -> " +
                                    $"{kvp.Value}");
                            }
                        }
                    };

                    // =================================================
                    // ADD SLOT BUTTON
                    // =================================================

                    slotsContainer.Children.Add(
                        slotButton);
                }
            }
            else
            {
                Console.WriteLine(
                    $"[TerminbuchenPage] " +
                    $"No available slots found for service " +
                    $"{service.Id}");

                slotsContainer.Children.Add(
                    new Label
                    {
                        Text = "No available slots",
                        TextColor =
                            Color.FromArgb("#FF6666"),
                        FontFamily = "Oswald",
                        FontSize = 12,
                        Margin = new Thickness(0, 5)
                    });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[TerminbuchenPage] " +
                $"Error loading slots: {ex}");

            slotsContainer.Children.Clear();

            slotsContainer.Children.Add(
                new Label
                {
                    Text = "Error loading slots",
                    TextColor =
                        Color.FromArgb("#FF6666"),
                    FontFamily = "Oswald",
                    FontSize = 12
                });
        }
    }

    // ============================================================
    // SERVICE PROVIDER SELECTED
    // ============================================================

    private void OnServiceProviderSelected(
        Servies service,
        WorkTeam provider)
    {
        try
        {
            var viewModel =
                this.BindingContext as AppViewModel;

            if (viewModel == null)
                return;

            viewModel.CurrentBooking ??=
                new Booking();

            viewModel.CurrentBooking.EnsureMaps();

            // ========================================================
            // REMOVE PREVIOUS SELECTION
            // ========================================================

            viewModel.CurrentBooking
                .RemoveServiceSelections(
                    service.Id);

            // ========================================================
            // SAVE PROVIDER
            // ========================================================

            viewModel.CurrentBooking
                .ServiceProviderMap[service.Id] =
                provider.Id;

            Console.WriteLine(
                $"[TerminbuchenPage] " +
                $"Service '{service.NameServies}' " +
                $"(ID: {service.Id}) assigned to provider " +
                $"'{provider.Name}' " +
                $"(ID: {provider.Id})");

            Console.WriteLine(
                $"[TerminbuchenPage] " +
                $"ServiceProviderMap now contains: " +
                $"{viewModel.CurrentBooking.ServiceProviderMap.Count} entries");

            foreach (
                var kvp in viewModel.CurrentBooking.ServiceProviderMap)
            {
                Console.WriteLine(
                    $"  [MAP] " +
                    $"Service {kvp.Key} -> Provider {kvp.Value}");
            }

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Toast.Make(
                    $"Provider selected: {provider.Name} for {service.NameServies}",
                    ToastDuration.Short)
                    .Show();
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[TerminbuchenPage] " +
                $"ERROR in OnServiceProviderSelected: {ex}");

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert(
                    "Error",
                    $"Error selecting provider: {ex.Message}",
                    "OK");
            });
        }
    }

    // ============================================================
    // BACK BUTTON
    // ============================================================

    protected override bool OnBackButtonPressed()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await NavigationService.HandleBackButton(
                NavigationService.ROUTE_TERM_BOOKING);
        });

        return true;
    }

    // ============================================================
    // HEADER BACK BUTTON
    // ============================================================

    private async void Button_Clicked(
        object sender,
        EventArgs e)
    {
        await NavigationService.HandleBackButton(
            NavigationService.ROUTE_TERM_BOOKING);
    }

    // ============================================================
    // ADD SERVICES
    // ============================================================

    private async void Button_Clicked_1(
        object sender,
        EventArgs e)
    {
    }

    // ============================================================
    // NEXT / CONTINUE
    // ============================================================

    private async void OnNext_Clicked(
        object sender,
        EventArgs e)
    {
        var vm = AppViewModel.Instance;

        vm.CurrentBooking ??=
            new Booking();

        vm.CurrentBooking.EnsureMaps();

        // ========================================================
        // PROVIDER
        // ========================================================

        if (vm.SelectedProvider != null)
        {
            vm.CurrentBooking.ProviderId =
                vm.SelectedProvider.Id.ToString();
        }

        // ========================================================
        // DATE
        // ========================================================

        if (!vm.CurrentBooking.HasPerServiceSelections ||
            vm.CurrentBooking.ServiceDateMap.Count == 0)
        {
            vm.CurrentBooking.Date =
                vm.SelectedDate;
        }

        // ========================================================
        // TIME
        // ========================================================

        if (vm.SelectedSlot != null)
        {
            vm.CurrentBooking.Time =
                vm.SelectedTime;
        }

        // ========================================================
        // DEBUG
        // ========================================================

        Console.WriteLine(
            "[TerminbuchenPage] ===== NEXT =====");

        Console.WriteLine(
            $"[TerminbuchenPage] ProviderId: " +
            $"{vm.CurrentBooking.ProviderId}");

        Console.WriteLine(
            $"[TerminbuchenPage] Date: " +
            $"{vm.CurrentBooking.Date}");

        Console.WriteLine(
            $"[TerminbuchenPage] Time: " +
            $"{vm.CurrentBooking.Time}");

        Console.WriteLine(
            $"[TerminbuchenPage] " +
            $"ServiceProviderMap: " +
            $"{vm.CurrentBooking.ServiceProviderMap.Count}");

        Console.WriteLine(
            $"[TerminbuchenPage] " +
            $"ServiceDateMap: " +
            $"{vm.CurrentBooking.ServiceDateMap.Count}");

        Console.WriteLine(
            $"[TerminbuchenPage] " +
            $"ServiceSlotMap: " +
            $"{vm.CurrentBooking.ServiceSlotMap.Count}");

        foreach (
            var item in vm.CurrentBooking.ServiceProviderMap)
        {
            Console.WriteLine(
                $"[PROVIDER_MAP] " +
                $"Service {item.Key} -> Provider {item.Value}");
        }

        foreach (
            var item in vm.CurrentBooking.ServiceDateMap)
        {
            Console.WriteLine(
                $"[DATE_MAP] " +
                $"Service {item.Key} -> " +
                $"{item.Value:yyyy-MM-dd}");
        }

        foreach (
            var item in vm.CurrentBooking.ServiceSlotMap)
        {
            Console.WriteLine(
                $"[SLOT_MAP] " +
                $"Service {item.Key} -> " +
                $"{item.Value}");
        }

        // ========================================================
        // NAVIGATE
        // ========================================================

        await NavigationService.NavigateToTabBarPage(
            NavigationService.ROUTE_BOOKING);
    }

    // ============================================================
    // POP PAGE
    // ============================================================

    private async void Button_Clicked_2(
        object sender,
        EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // ============================================================
    // REMOVE SERVICE
    // ============================================================

    private void OnRemoveService(
        object sender,
        EventArgs e)
    {
        try
        {
            if (sender is Button button &&
                button.BindingContext is Servies service)
            {
                var viewModel =
                    this.BindingContext as AppViewModel;

                if (viewModel != null)
                {
                    // =================================================
                    // REMOVE FROM SELECTED SERVICES
                    // =================================================

                    viewModel.RemoveSelectedService(
                        service);

                    // =================================================
                    // REMOVE FROM BOOKING MAPS
                    // =================================================

                    viewModel.CurrentBooking?
                        .RemoveServiceSelections(
                            service.Id);

                    Console.WriteLine(
                        $"[TerminbuchenPage] " +
                        $"Removed service {service.Id}");

                    // =================================================
                    // REFRESH PROVIDERS
                    // =================================================

                    _ = RefreshProviderSelectorsAsync();

                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Toast.Make(
                            $"Removed: {service.NameServies}",
                            ToastDuration.Short)
                            .Show();
                    });
                }
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert(
                    "Error",
                    ex.Message,
                    "OK");
            });
        }
    }

    // ============================================================
    // LOAD REMINDER OPTIONS FROM API
    // ============================================================

    private async Task LoadReminderOptionsAsync()
    {
        try
        {
            var viewModel = this.BindingContext as AppViewModel;
            if (viewModel == null)
                return;

            Console.WriteLine("[TerminbuchenPage] Loading reminder options from API...");

            var apiServices = new ApiServices();
            var response = await apiServices.GetReminderOptionsAsync();

            if (response == null || !response.Success || response.Data == null)
            {
                Console.WriteLine("[TerminbuchenPage] Failed to load reminder options");
                return;
            }

            Console.WriteLine($"[TerminbuchenPage] Loaded {response.Data.Options.Count} reminder options");

            // Update ViewModel with reminder options
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                viewModel.ReminderOptions.Clear();
                foreach (var option in response.Data.Options)
                {
                    viewModel.ReminderOptions.Add(option);
                }

                // Set default selected option
                if (response.Data.DefaultOffsetHours > 0)
                {
                    var defaultIndex = response.Data.Options.FindIndex(o => o.OffsetHours == response.Data.DefaultOffsetHours);
                    if (defaultIndex >= 0)
                    {
                        viewModel.SelectedReminderOptionIndex = defaultIndex;
                        Console.WriteLine($"[TerminbuchenPage] Set default reminder option index: {defaultIndex}");
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TerminbuchenPage] Exception loading reminder options: {ex.Message}");
        }
    }
}
