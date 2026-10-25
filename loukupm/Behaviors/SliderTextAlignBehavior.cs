using System.Globalization;
using loukupm.Langue;

namespace loukupm.Behaviors
{
    /// <summary>
    /// Keeps slider text on the reading side. The carousel itself stays left-to-right
    /// because Android closes the app when that control becomes right-to-left.
    /// </summary>
    public class SliderTextAlignBehavior : Behavior<VerticalStackLayout>
    {
        readonly List<Label> _labels = new();
        VerticalStackLayout _layout;

        protected override void OnAttachedTo(VerticalStackLayout bindable)
        {
            base.OnAttachedTo(bindable);
            _layout = bindable;
            bindable.ChildAdded += OnChildAdded;
            bindable.Loaded += OnLoaded;
            LocalizationResourcesManager.Instanse.LanguageChanged += OnLanguageChanged;

            foreach (var child in bindable.Children)
                Watch(child);

            Apply();
        }

        protected override void OnDetachingFrom(VerticalStackLayout bindable)
        {
            bindable.ChildAdded -= OnChildAdded;
            bindable.Loaded -= OnLoaded;
            LocalizationResourcesManager.Instanse.LanguageChanged -= OnLanguageChanged;

            foreach (var label in _labels)
                label.HandlerChanged -= OnLabelHandlerChanged;

            _labels.Clear();
            _layout = null;
            base.OnDetachingFrom(bindable);
        }

        void OnLoaded(object sender, EventArgs e) => Apply();

        void OnChildAdded(object sender, ElementEventArgs e)
        {
            if (e.Element is IView view)
                Watch(view);

            Apply();
        }

        void OnLanguageChanged(CultureInfo culture)
        {
            MainThread.BeginInvokeOnMainThread(Apply);
        }

        void OnLabelHandlerChanged(object sender, EventArgs e)
        {
            Apply();
            _layout?.Dispatcher.Dispatch(Apply);
        }

        void Watch(IView child)
        {
            if (child is not Label label || _labels.Contains(label))
                return;

            _labels.Add(label);
            label.HandlerChanged += OnLabelHandlerChanged;
        }

        void Apply()
        {
            if (_layout == null)
                return;

            var rtl = LocalizationResourcesManager.Instanse.GetFlowDirection() == FlowDirection.RightToLeft;
            _layout.FlowDirection = FlowDirection.LeftToRight;
            _layout.HorizontalOptions = LayoutOptions.Fill;

            foreach (var child in _layout.Children)
                Watch(child);

            foreach (var label in _labels)
                ApplyLabel(label, rtl);
        }

        static void ApplyLabel(Label label, bool rtl)
        {
            label.FlowDirection = FlowDirection.LeftToRight;
            label.HorizontalOptions = LayoutOptions.Fill;
            label.HorizontalTextAlignment = rtl ? TextAlignment.End : TextAlignment.Start;

#if ANDROID
            if (label.Handler?.PlatformView is Android.Widget.TextView textView)
            {
                textView.LayoutDirection = Android.Views.LayoutDirection.Ltr;
                textView.TextDirection = rtl
                    ? Android.Views.TextDirection.Rtl
                    : Android.Views.TextDirection.Ltr;
                textView.Gravity = (rtl
                        ? Android.Views.GravityFlags.Right
                        : Android.Views.GravityFlags.Left)
                    | Android.Views.GravityFlags.CenterVertical;
            }
#endif
        }
    }
}
