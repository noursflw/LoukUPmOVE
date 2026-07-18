using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using loukupm.Langue;
using loukupm.Model;
using loukupm.Services;
using loukupm.ViewModel;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace loukupm.View;

public partial class ServicesPage : ContentPage
{
    // ✅ نسخة محلية منفصلة من الخدمات المرشحة - لا تؤثر على HomePage
    private ObservableCollection<Servies> _localFilteredServices;
    private string _localSearchServiceTerm = string.Empty;

    // ✅ Public property لـ XAML Binding
    public ObservableCollection<Servies> LocalFilteredServices
    {
        get => _localFilteredServices;
        private set => _localFilteredServices = value;
    }

    public ServicesPage()
    {
        InitializeComponent();
        this.InitializeLanguageTracking();
        // ✅ استخدم Instance
        var vm = AppViewModel.Instance;

        // ✅ أنشئ نسخة محلية
        _localFilteredServices = new ObservableCollection<Servies>(vm.Services);

        // ✅ اضبط BindingContext على الصفحة نفسها لاستخدام LocalFilteredServices
        // لكن احتفظ بـ reference إلى VM للبيانات الأخرى
        this.BindingContext = vm;

        // ✅ ربط البيانات الأخرى من ViewModel
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // لا تُعدّل BindingContext، فقط استخدم vm مباشرة في الكود
        });

        Console.WriteLine($"📱 [ServicesPage] Initialized with local filtered services copy. Count: {_localFilteredServices.Count}");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // ✅ عند فتح الصفحة، أعد تعيين البيانات المحلية من الأصلية
        if (BindingContext is AppViewModel vm)
        {
            _localFilteredServices.Clear();
            foreach (var service in vm.Services)
            {
                _localFilteredServices.Add(service);
            }
            _localSearchServiceTerm = string.Empty;

            // ⚠️ امسح الفلترة والبحث المشترك في ViewModel
            vm.SelectedCategory = null;
            vm.SearchServiceTerm = string.Empty;

            // ⚠️ امسح FilteredServices المشترك أيضاً
            vm.FilteredServices.Clear();
            foreach (var service in vm.Services)
            {
                vm.FilteredServices.Add(service);
            }

            OnPropertyChanged(nameof(LocalFilteredServices));

            Console.WriteLine($"✅ [ServicesPage] OnAppearing - Reset to show all {_localFilteredServices.Count} services");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // ✅ عند الخروج من الصفحة، امسح البيانات المحلية والمشتركة
        if (BindingContext is AppViewModel vm)
        {
            _localFilteredServices?.Clear();
            _localSearchServiceTerm = string.Empty;

            // ⚠️ امسح البحث من ViewModel أيضاً
            vm.SearchServiceTerm = string.Empty;
        }

        Console.WriteLine($"🚪 [ServicesPage] OnDisappearing - Cleared local filtered services");
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        var service = (sender as Button)?.BindingContext as Servies;
        if (service == null) return;

        var vm = BindingContext as AppViewModel;
        if (vm?.SelectServiceButtonCommand is ICommand command && command.CanExecute(service))
        {
            command.Execute(service);
        }
    }


    private Frame _lastSelectedFrame;

    private readonly List<Frame> _categoryFrames = new();

    private async void OnCategoryTapped(object sender, TappedEventArgs e)
    {
        if (sender is Frame tappedFrame && tappedFrame.BindingContext is Category selectedCategory)
        {
            // ✅ استخدم الفلترة المحلية فقط - لا تؤثر على البيانات المشتركة
            FilterServicesLocally(selectedCategory);


            if (!_categoryFrames.Contains(tappedFrame))
                _categoryFrames.Add(tappedFrame);


            if (_lastSelectedFrame != null)
            {
                _lastSelectedFrame.BorderColor = Color.FromArgb("#444444");
                _lastSelectedFrame.BackgroundColor = Color.FromArgb("#444444");

                if (_lastSelectedFrame.Content is Label oldLabel)
                    oldLabel.TextColor = Color.FromArgb("#999999");
            }


            tappedFrame.BorderColor = Color.FromArgb("#C9A24A");
            tappedFrame.BackgroundColor = Color.FromArgb("#C9A24A");


            if (tappedFrame.Content is Label label)
            {
                label.TextColor = Color.FromArgb("#000000");
            }

            _lastSelectedFrame = tappedFrame;

            tappedFrame.AnchorX = 0.5;
            tappedFrame.AnchorY = 0.5;

            await tappedFrame.ScaleTo(1.05, 100, Easing.CubicOut);
            await tappedFrame.ScaleTo(1, 100, Easing.CubicIn);
        }
    }

    // ✅ فلترة محلية منفصلة - لا تؤثر على HomePage
    private void FilterServicesLocally(Category category)
    {
        if (BindingContext is not AppViewModel vm)
            return;

        // ⚠️ امسح البحث السابق عند تغيير الفئة
        _localSearchServiceTerm = string.Empty;

        IEnumerable<Servies> result = vm.Services;

        if (category != null &&
            !string.IsNullOrWhiteSpace(category.Name) &&
            category.Name != "الكل")
        {
            result = result.Where(s => s.Category?.Name == category.Name);
        }

        _localFilteredServices.Clear();

        foreach (var item in result)
            _localFilteredServices.Add(item);

        Console.WriteLine($"🔍 [ServicesPage] Filtered locally to: {_localFilteredServices.Count} services (Category: {category?.Name ?? "All"})");
    }

    // ✅ بحث محلي منفصل - لا يؤثر على HomePage
    public void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (BindingContext is not AppViewModel vm)
            return;

        _localSearchServiceTerm = e.NewTextValue ?? string.Empty;

        // ⚠️ تحديث البحث المحلي فقط
        PerformLocalSearch();
    }

    private void PerformLocalSearch()
    {
        if (BindingContext is not AppViewModel vm)
            return;

        IEnumerable<Servies> result = vm.Services;

        // ✅ تطبيق الفئة أولاً
        var currentCategory = vm.SelectedCategory;
        if (currentCategory != null &&
            !string.IsNullOrWhiteSpace(currentCategory.Name) &&
            currentCategory.Name != "الكل")
        {
            result = result.Where(s => s.Category?.Name == currentCategory.Name);
        }

        // ✅ ثم تطبيق البحث
        if (!string.IsNullOrWhiteSpace(_localSearchServiceTerm))
        {
            result = result.Where(s =>
                s.NameServies?.Contains(_localSearchServiceTerm, StringComparison.OrdinalIgnoreCase) == true
            );
        }

        _localFilteredServices.Clear();

        foreach (var item in result)
            _localFilteredServices.Add(item);

        Console.WriteLine($"🔎 [ServicesPage] Local search completed. Found: {_localFilteredServices.Count} services");
    }

    private void ResetCategoriesUI()
    {
        _lastSelectedFrame = null;

        foreach (var frame in _categoryFrames)
        {
            frame.BorderColor = Color.FromArgb("#444444");
            frame.BackgroundColor = Color.FromArgb("#444444");

            if (frame.Content is Label label)
                label.TextColor = Color.FromArgb("#999999");
        }
    }



    private async void Button_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }


    private async void Button_Clicked_2(object sender, EventArgs e)
    {
        var selected = AppViewModel.Instance.CurrentBooking.SelectedServices;

        if (selected == null || selected.Count == 0)
        {
            await Toast.Make(AppResource.pleaseselectoneservice).Show();
            return;
        }

        await Navigation.PushAsync(new TerminbuchenPage());
    }


    private void Button_Clicked_3(object sender, EventArgs e)
    {
        // ✅ عند النقر على "ALL"، امسح الفلترة والبحث المحلي
        ResetCategoriesUI();

        // ✅ امسح البحث المحلي والفئة
        _localSearchServiceTerm = string.Empty;

        if (BindingContext is AppViewModel vm)
        {
            // ✅ أعد تحميل جميع الخدمات الأصلية بدون فلترة
            _localFilteredServices.Clear();
            foreach (var service in vm.Services)
            {
                _localFilteredServices.Add(service);
            }

            // ✅ امسح الفلترة والبحث المشترك
            vm.SelectedCategory = null;
            vm.SearchServiceTerm = string.Empty;

            Console.WriteLine($"✅ [ServicesPage] ALL button clicked - Reset to show all {_localFilteredServices.Count} services");
        }
    }
}
