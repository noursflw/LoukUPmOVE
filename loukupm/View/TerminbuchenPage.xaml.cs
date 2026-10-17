using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using loukupm.Langue;
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
    private readonly Dictionary<int, ServiceSlotHost> _slotHosts = new();

    private sealed class ServiceSlotHost
    {
        public Servies Service { get; init; }
        public FlexLayout SlotsContainer { get; init; }
        public WorkTeam Provider { get; set; }
    }

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

            _slotHosts.Clear();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                ServiceProviderSelectorsContainer.Children.Clear();
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
        // PROVIDER SELECTOR
        // ========================================================

        WorkTeam? selectedProvider = null;

        var nameLabel = new Label
        {
            Text = AppResource.SelectProvider,
            TextColor = Color.FromArgb("#999999"),
            FontFamily = "Oswald",
            FontSize = 14,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        };

        var chevron = new Label
        {
            Text = "▾",
            TextColor = Color.FromArgb("#D4AF37"),
            FontSize = 16,
            VerticalOptions = LayoutOptions.Center
        };

        var selectorGrid = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };
        selectorGrid.Add(nameLabel, 0, 0);
        selectorGrid.Add(chevron, 1, 0);

        var selector = new Border
        {
            BackgroundColor = Color.FromArgb("#1A1813"),
            Stroke = Color.FromArgb("#D4AF37"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(10)
            },
            Padding = new Thickness(12, 10),
            Content = selectorGrid
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

        var slotHost = new ServiceSlotHost
        {
            Service = service,
            SlotsContainer = slotsContainer
        };
        _slotHosts[service.Id] = slotHost;

        // ========================================================
        // GET PROVIDERS
        // ========================================================

        var providers =
            viewModel?.GetAlternativeProvidersForService(service.Id)?.ToList();

        if (providers != null && providers.Count > 0)
        {
            // ====================================================
            // RESTORE PREVIOUS PROVIDER
            // ====================================================

            if (viewModel?.CurrentBooking != null &&
                viewModel.CurrentBooking.ServiceProviderMap.TryGetValue(
                    service.Id,
                    out var providerId))
            {
                var existing =
                    providers.FirstOrDefault(
                        p => p.Id == providerId);

                if (existing != null)
                {
                    selectedProvider = existing;
                    slotHost.Provider = existing;
                    nameLabel.Text = existing.Name;
                    nameLabel.TextColor = Color.FromArgb("#E8C966");
                }
            }
        }
        else
        {
            var noProvidersLabel = new Label
            {
                Text = AppResource.NoProviderAvailable,
                TextColor = Color.FromArgb("#FF6666"),
                FontFamily = "Oswald",
                FontSize = 12,
                Margin = new Thickness(0, 4, 0, 4)
            };

            verticalStackLayout.Children.Add(
                noProvidersLabel);
        }

        // ========================================================
        // BUILD DAY SHORTCUTS (day abbreviation + date number)
        // ========================================================

        void ApplyDayCellStyle(Border cell, bool selected)
        {
            cell.BackgroundColor = selected
                ? Color.FromArgb("#FFD700")
                : Color.FromArgb("#333333");

            cell.Stroke = selected
                ? Color.FromArgb("#FFD700")
                : Color.FromArgb("#444444");

            if (cell.Content is not Grid grid)
                return;

            foreach (var child in grid.Children.OfType<Label>())
            {
                var row = Grid.GetRow(child);
                if (row == 0)
                {
                    child.TextColor = selected
                        ? Colors.Black
                        : Color.FromArgb("#999999");
                }
                else
                {
                    child.TextColor = selected
                        ? Colors.Black
                        : Colors.White;
                }
            }
        }

        Border CreateDayCell(DateTime date, bool selected)
        {
            var dayLabel = new Label
            {
                Text = GetDayAbbreviation(date.DayOfWeek),
                FontFamily = "Oswald",
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.NoWrap,
                MaxLines = 1,
                HeightRequest = 18
            };

            var dateLabel = new Label
            {
                Text = date.ToString("dd.MM"),
                FontFamily = "Oswald",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.NoWrap,
                MaxLines = 1,
                HeightRequest = 24
            };

            Grid.SetRow(dayLabel, 0);
            Grid.SetRow(dateLabel, 1);

            var content = new Grid
            {
                RowSpacing = 0,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(18) },
                    new RowDefinition { Height = new GridLength(24) }
                },
                Children = { dayLabel, dateLabel }
            };

            var cell = new Border
            {
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(10)
                },
                Padding = new Thickness(4, 6),
                WidthRequest = 58,
                HeightRequest = 72,
                Content = content
            };

            ApplyDayCellStyle(cell, selected);
            return cell;
        }

        void BuildDayButtons()
        {
            daysContainer.Children.Clear();

            var today = DateTime.Today;
            DateTime? highlightedDate = null;

            if (viewModel?.CurrentBooking != null &&
                viewModel.CurrentBooking.ServiceDateMap.TryGetValue(
                    service.Id,
                    out var mappedDate))
            {
                highlightedDate = mappedDate.Date;
            }

            for (int i = 0; i < 7; i++)
            {
                var date = today.AddDays(i);
                var isSelected =
                    highlightedDate.HasValue &&
                    Booking.ToCalendarDate(highlightedDate.Value) == Booking.ToCalendarDate(date);

                var cell = CreateDayCell(date, isSelected);

                var tap = new TapGestureRecognizer();
                tap.Tapped += async (s, e) =>
                {
                    foreach (var c in daysContainer.Children.OfType<Border>())
                        ApplyDayCellStyle(c, selected: false);

                    ApplyDayCellStyle(cell, selected: true);

                    Console.WriteLine(
                        $"[TerminbuchenPage] Date selected for service " +
                        $"{service.Id}: {date:yyyy-MM-dd}");

                    var dateChanged = RememberServiceDate(service, date);

                    if (selectedProvider != null)
                    {
                        await LoadSlotsAsync(
                            service,
                            selectedProvider,
                            date,
                            slotsContainer);

                        if (dateChanged)
                            await RefreshFollowingServiceSlotsAsync(service.Id);
                    }
                    else if (dateChanged)
                    {
                        await RefreshFollowingServiceSlotsAsync(service.Id);
                    }

                    if (selectedProvider == null)
                    {
                        slotsContainer.Children.Clear();

                        slotsContainer.Children.Add(
                            new Label
                            {
                                Text = AppResource.PleaseSelectProviderFirst,
                                TextColor =
                                    Color.FromArgb("#FF6666"),
                                FontFamily = "Oswald",
                                FontSize = 12
                            });
                    }
                };

                cell.GestureRecognizers.Add(tap);
                daysContainer.Children.Add(cell);
            }
        }

        // ========================================================
        // PROVIDER SELECTION
        // ========================================================

        if (providers != null && providers.Count > 0)
        {
            var openProviders = new TapGestureRecognizer();
            openProviders.Tapped += async (s, e) =>
            {
                var popup = new ProviderSelectionPopup(
                    AppResource.SelectProvider,
                    providers,
                    selectedProvider);

                if (await this.ShowPopupAsync(popup) is not WorkTeam chosen)
                    return;

                selectedProvider = chosen;
                slotHost.Provider = chosen;
                nameLabel.Text = chosen.Name;
                nameLabel.TextColor = Color.FromArgb("#E8C966");

                var providerChanged = OnServiceProviderSelected(service, chosen);
                BuildDayButtons();
                slotsContainer.Children.Clear();

                Console.WriteLine(
                    $"[TerminbuchenPage] Provider changed for " +
                    $"service {service.Id}: " +
                    $"{chosen.Name}");

                if (viewModel?.CurrentBooking != null &&
                    viewModel.CurrentBooking.ServiceDateMap.TryGetValue(
                        service.Id,
                        out var existingDate))
                {
                    await LoadSlotsAsync(
                        service,
                        chosen,
                        existingDate,
                        slotsContainer);

                    if (providerChanged)
                        await RefreshFollowingServiceSlotsAsync(service.Id);
                }
                else if (providerChanged)
                {
                    await RefreshFollowingServiceSlotsAsync(service.Id);
                }
            };

            selector.GestureRecognizers.Add(openProviders);

            verticalStackLayout.Children.Add(selector);
        }

        var daysScrollView = new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
            Content = daysContainer,
            Margin = new Thickness(0, 8, 0, 8)
        };

        verticalStackLayout.Children.Add(
            daysScrollView);

        verticalStackLayout.Children.Add(
            slotsContainer);

        if (selectedProvider != null)
        {
            BuildDayButtons();

            if (viewModel?.CurrentBooking != null &&
                viewModel.CurrentBooking.ServiceDateMap.TryGetValue(service.Id, out var savedDate))
            {
                _ = LoadSlotsAsync(service, selectedProvider, savedDate, slotsContainer);
            }
        }

        return container;
    }

    // ============================================================
    // DAY ABBREVIATIONS (ar / de)
    // ============================================================

    private static string GetDayAbbreviation(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Sunday => AppResource.DayAbbrevSunday,
            DayOfWeek.Monday => AppResource.DayAbbrevMonday,
            DayOfWeek.Tuesday => AppResource.DayAbbrevTuesday,
            DayOfWeek.Wednesday => AppResource.DayAbbrevWednesday,
            DayOfWeek.Thursday => AppResource.DayAbbrevThursday,
            DayOfWeek.Friday => AppResource.DayAbbrevFriday,
            DayOfWeek.Saturday => AppResource.DayAbbrevSaturday,
            _ => string.Empty
        };
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
            // Each later service only keeps times that start after
            // every earlier selected service has finished.
            // ========================================================

            var slots = (avail?.Data?.AvailableSlots ?? new List<AvailableSlot>())
                .Where(slot => IsSlotAfterEarlierServices(service, date, slot))
                .ToList();

            RemoveSavedSlotIfTooEarly(service);

            if (slots.Count > 0)
            {
                Console.WriteLine(
                    $"[TerminbuchenPage] Found " +
                    $"{slots.Count} available slots");

                foreach (var slot in slots)
                {
                    var isSavedSlot = false;

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

                        isSavedSlot = string.Equals(
                            savedSlot,
                            formattedSavedSlot,
                            StringComparison.OrdinalIgnoreCase);
                    }

                    var slotChip = CreateSlotChip(slot, isSavedSlot);

                    var tap = new TapGestureRecognizer();
                    tap.Tapped += async (ss, ee) =>
                    {
                        foreach (var c in slotsContainer.Children.OfType<Border>())
                        {
                            if (c.ClassId == "slot")
                                ApplySlotChipStyle(c, selected: false);
                        }

                        ApplySlotChipStyle(slotChip, selected: true);

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

                            var formattedEnd = ResolveSlotEnd(
                                service,
                                slot,
                                formattedTime);

                            // ==========================================
                            // SERVICE SLOT
                            // ==========================================

                            viewModel.CurrentBooking.SetServiceSlot(
                                service.Id,
                                formattedTime,
                                formattedEnd);

                            // ==========================================
                            // SERVICE DATE
                            // ==========================================

                            var day = Booking.ToCalendarDate(date);
                            viewModel.CurrentBooking.SetServiceDate(service.Id, day);
                            viewModel.SelectedDate = day;

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

                            await RefreshFollowingServiceSlotsAsync(service.Id);
                        }
                    };

                    slotChip.GestureRecognizers.Add(tap);
                    slotsContainer.Children.Add(slotChip);
                }
            }
            else
            {
                Console.WriteLine(
                    $"[TerminbuchenPage] " +
                    $"No available slots found for service " +
                    $"{service.Id}");

                var emptyState = new VerticalStackLayout
                {
                    Spacing = 4,
                    Margin = new Thickness(0, 5)
                };

                FlexLayout.SetBasis(emptyState, new FlexBasis(1, true));

                emptyState.Children.Add(
                    new Label
                    {
                        Text = AppResource.NoAppointmentsAvailable,
                        TextColor = Color.FromArgb("#FF6666"),
                        FontFamily = "georgia-bold",
                        FontSize = 16,
                        LineBreakMode = LineBreakMode.WordWrap
                    });

                emptyState.Children.Add(
                    new Label
                    {
                        Text = AppResource.NoAppointmentsAvailableForDay,
                        TextColor = Color.FromArgb("#888888"),
                        FontFamily = "Oswald",
                        FontSize = 13,
                        LineBreakMode = LineBreakMode.WordWrap
                    });

                slotsContainer.Children.Add(emptyState);
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
                    Text = AppResource.ErrorLoadingSlots,
                    TextColor =
                        Color.FromArgb("#FF6666"),
                    FontFamily = "Oswald",
                    FontSize = 12
                });
        }
    }

    private static Border CreateSlotChip(AvailableSlot slot, bool selected)
    {
        var startText = FormatSlotClock(slot.StartTime);
        if (string.IsNullOrWhiteSpace(startText))
            startText = slot.DisplayTime;

        var endText = FormatSlotClock(slot.EndTime);

        var stack = new VerticalStackLayout
        {
            Spacing = 0,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        stack.Children.Add(new Label
        {
            Text = startText,
            FontFamily = "Oswald",
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            HorizontalTextAlignment = TextAlignment.Center,
            HorizontalOptions = LayoutOptions.Center
        });

        if (!string.IsNullOrWhiteSpace(endText) &&
            !string.Equals(endText, startText, StringComparison.Ordinal))
        {
            stack.Children.Add(new Label
            {
                Text = endText,
                FontFamily = "Oswald",
                FontSize = 11,
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Center
            });
        }

        var chip = new Border
        {
            ClassId = "slot",
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(12)
            },
            Padding = new Thickness(4, 8),
            Margin = new Thickness(3),
            HeightRequest = 58,
            HorizontalOptions = LayoutOptions.Fill,
            Content = stack
        };

        FlexLayout.SetBasis(chip, new FlexBasis(0.30f, true));
        ApplySlotChipStyle(chip, selected);
        return chip;
    }

    private static void ApplySlotChipStyle(Border chip, bool selected)
    {
        chip.BackgroundColor = selected
            ? Color.FromArgb("#FFD700")
            : Color.FromArgb("#1A1813");

        chip.Stroke = selected
            ? Color.FromArgb("#F3D97A")
            : Color.FromArgb("#3A3A3A");

        if (chip.Content is not VerticalStackLayout stack)
            return;

        var labels = stack.Children.OfType<Label>().ToList();
        if (labels.Count > 0)
            labels[0].TextColor = selected ? Colors.Black : Colors.White;

        if (labels.Count > 1)
        {
            labels[1].TextColor = selected
                ? Color.FromArgb("#3A2E00")
                : Color.FromArgb("#8A8A8A");
        }
    }

    private static string FormatSlotClock(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        if (TimeSpan.TryParse(raw, out var time))
            return $"{time.Hours:00}:{time.Minutes:00}";

        if (DateTime.TryParse(raw, out var dateTime))
            return dateTime.ToString("HH:mm");

        return raw.Trim();
    }

    private bool IsSlotAfterEarlierServices(Servies service, DateTime slotDate, AvailableSlot slot)
    {
        if (!TryGetPreviousServicesEnd(service, out var earlierEnd))
            return true;

        var day = Booking.ToCalendarDate(slotDate);
        var endDay = Booking.ToCalendarDate(earlierEnd);

        if (day > endDay)
            return true;

        if (day < endDay)
            return false;

        if (!TryParseClock(slot.StartTime, out var start))
            return false;

        return start >= earlierEnd.TimeOfDay;
    }

    private bool TryGetPreviousServicesEnd(Servies service, out DateTime earlierEnd)
    {
        earlierEnd = default;

        if (BindingContext is not AppViewModel viewModel)
            return false;

        var services = viewModel.SelectedServices?.ToList();
        if (services == null || services.Count < 2)
            return false;

        var index = services.FindIndex(s => s.Id == service.Id);
        if (index <= 0)
            return false;

        DateTime? latestEnd = null;
        for (var i = 0; i < index; i++)
        {
            if (!TryGetScheduledEnd(viewModel, services[i], out var endAt))
                continue;

            if (latestEnd == null || endAt > latestEnd)
                latestEnd = endAt;
        }

        if (latestEnd == null)
            return false;

        earlierEnd = latestEnd.Value;
        return true;
    }

    private static bool TryGetScheduledEnd(AppViewModel viewModel, Servies service, out DateTime endAt)
    {
        endAt = default;
        var booking = viewModel.CurrentBooking;
        if (booking == null || service == null)
            return false;

        if (!booking.ServiceDateMap.TryGetValue(service.Id, out var date))
            return false;

        if (!booking.ServiceSlotMap.TryGetValue(service.Id, out var startRaw))
            return false;

        if (!TryParseClock(startRaw, out var start))
            return false;

        var day = Booking.ToCalendarDate(date);
        var startAt = day.Add(start);

        if (booking.ServiceSlotEndMap.TryGetValue(service.Id, out var endRaw) &&
            TryParseClock(endRaw, out var endClock) &&
            endClock > start)
        {
            endAt = day.Add(endClock);
            return true;
        }

        var minutes = service.TimeServies > 0
            ? service.TimeServies
            : 30;

        endAt = startAt.Add(TimeSpan.FromMinutes(minutes));
        return true;
    }

    private void RemoveSavedSlotIfTooEarly(Servies service)
    {
        if (BindingContext is not AppViewModel viewModel || viewModel.CurrentBooking == null)
            return;

        if (!TryGetPreviousServicesEnd(service, out var earlierEnd))
            return;

        var booking = viewModel.CurrentBooking;
        if (!booking.ServiceSlotMap.TryGetValue(service.Id, out var saved))
            return;

        if (!booking.ServiceDateMap.TryGetValue(service.Id, out var savedDate))
            return;

        if (!TryParseClock(saved, out var start))
            return;

        var day = Booking.ToCalendarDate(savedDate);
        var endDay = Booking.ToCalendarDate(earlierEnd);
        var tooEarly = day < endDay || (day == endDay && start < earlierEnd.TimeOfDay);

        if (tooEarly)
            booking.ClearServiceSlot(service.Id);
    }

    private static string ResolveSlotEnd(Servies service, AvailableSlot slot, string formattedStart)
    {
        var formattedEnd = Booking.FormatApiTime(slot.EndTime);
        if (!string.IsNullOrWhiteSpace(formattedEnd) &&
            TryParseClock(formattedStart, out var start) &&
            TryParseClock(formattedEnd, out var end) &&
            end > start)
        {
            return formattedEnd;
        }

        if (!TryParseClock(formattedStart, out var startClock))
            return formattedEnd;

        var minutes = slot.DurationMinutes > 0
            ? slot.DurationMinutes
            : service.TimeServies > 0
                ? service.TimeServies
                : 30;

        var computedEnd = startClock.Add(TimeSpan.FromMinutes(minutes));
        return $"{computedEnd.Hours:D2}:{computedEnd.Minutes:D2}";
    }

    private static bool TryParseClock(string raw, out TimeSpan time)
    {
        time = default;
        var formatted = Booking.FormatApiTime(raw);
        if (string.IsNullOrWhiteSpace(formatted))
            return false;

        return TimeSpan.TryParse(formatted, System.Globalization.CultureInfo.InvariantCulture, out time);
    }

    private async Task RefreshFollowingServiceSlotsAsync(int changedServiceId)
    {
        if (BindingContext is not AppViewModel viewModel)
            return;

        var services = viewModel.SelectedServices?.ToList();
        if (services == null || services.Count < 2)
            return;

        var index = services.FindIndex(s => s.Id == changedServiceId);
        if (index < 0 || index >= services.Count - 1)
            return;

        for (var i = index + 1; i < services.Count; i++)
        {
            var following = services[i];
            if (!_slotHosts.TryGetValue(following.Id, out var host) || host.Provider == null)
                continue;

            if (viewModel.CurrentBooking == null ||
                !viewModel.CurrentBooking.ServiceDateMap.TryGetValue(following.Id, out var date))
            {
                continue;
            }

            await LoadSlotsAsync(following, host.Provider, date, host.SlotsContainer);
        }
    }

    private bool RememberServiceDate(Servies service, DateTime date)
    {
        if (BindingContext is not AppViewModel viewModel)
            return false;

        viewModel.CurrentBooking ??= new Booking();
        viewModel.CurrentBooking.EnsureMaps();

        var day = Booking.ToCalendarDate(date);
        var dateChanged = !viewModel.CurrentBooking.ServiceDateMap.TryGetValue(service.Id, out var existing)
            || Booking.ToCalendarDate(existing) != day;

        viewModel.CurrentBooking.SetServiceDate(service.Id, day);
        viewModel.SelectedDate = day;

        if (dateChanged)
            viewModel.CurrentBooking.ClearServiceSlot(service.Id);

        return dateChanged;
    }

    // ============================================================
    // SERVICE PROVIDER SELECTED
    // ============================================================

    private bool OnServiceProviderSelected(
        Servies service,
        WorkTeam provider)
    {
        try
        {
            var viewModel =
                this.BindingContext as AppViewModel;

            if (viewModel == null)
                return false;

            viewModel.CurrentBooking ??=
                new Booking();

            viewModel.CurrentBooking.EnsureMaps();

            var providerChanged =
                !viewModel.CurrentBooking.ServiceProviderMap.TryGetValue(service.Id, out var currentProviderId)
                || currentProviderId != provider.Id;

            viewModel.CurrentBooking.ServiceProviderMap[service.Id] = provider.Id;

            // A new provider has different times. Keep the chosen date.
            if (providerChanged)
                viewModel.CurrentBooking.ClearServiceSlot(service.Id);

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
                //await Toast.Make(
                //    $"Provider selected: {provider.Name} for {service.NameServies}",
                //    ToastDuration.Short)
                //    .Show();
            });

            return providerChanged;
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

            return false;
        }
    }

    // ============================================================
    // BACK BUTTON
    // ============================================================

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Re-fetch so labels follow the active UI language.
        await LoadReminderOptionsAsync();
    }

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
            if (this.BindingContext is AppViewModel viewModel)
                await viewModel.RefreshReminderOptionsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TerminbuchenPage] Exception loading reminder options: {ex.Message}");
        }
    }


    private async void OnReminderTimeTapped(object sender, TappedEventArgs e)
    {
        if (BindingContext is not AppViewModel viewModel)
            return;

        if (viewModel.ReminderOptions.Count == 0)
            await viewModel.RefreshReminderOptionsAsync();

        if (viewModel.ReminderOptions.Count == 0)
            return;

        var popup = new ReminderTimePopup(
            viewModel.ReminderQuestion,
            viewModel.ReminderOptions,
            viewModel.SelectedReminderOption);

        if (await this.ShowPopupAsync(popup) is ReminderTimePopupResult result && result.Option != null)
            viewModel.SelectedReminderOption = result.Option;
    }

    private async void OnReminderSwitchToggled(object sender, ToggledEventArgs e)
    {
        bool isEnabled = e.Value;

        if (isEnabled)
        {
            // 1. إظهار العنصر وجعله شفافاً للبدء في تأثير الظهور
            ReminderDetailsLayout.IsVisible = true;
            ReminderDetailsLayout.Opacity = 0;

            // 2. تطبيق انميشن تدريجي للظهور والتوسع اللطيف
            await Task.WhenAll(
                ReminderDetailsLayout.FadeTo(1, 250, Easing.CubicIn),
                ReminderDetailsLayout.TranslateTo(0, 0, 250, Easing.CubicOut)
            );
        }
        else
        {
            // 1. انميشن الاختفاء التدريجي
            await Task.WhenAll(
                ReminderDetailsLayout.FadeTo(0, 200, Easing.CubicOut),
                ReminderDetailsLayout.TranslateTo(0, -10, 200, Easing.CubicIn)
            );

            // 2. إخفاؤه تماماً بعد انتهاء الحركة لكي لا يتفاعل مع الـ Layout
            ReminderDetailsLayout.IsVisible = false;
        }
    }
}
