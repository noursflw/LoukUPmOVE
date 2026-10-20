using System.Globalization;
using loukupm.Langue;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace loukupm.Behaviors
{
    /// <summary>
    /// Puts the dark fade on the side where the slider text starts.
    /// Arabic reads from the right, other languages from the left.
    /// </summary>
    public class SliderShadeBehavior : Behavior<BoxView>
    {
        BoxView _shade;

        protected override void OnAttachedTo(BoxView bindable)
        {
            base.OnAttachedTo(bindable);
            _shade = bindable;
            LocalizationResourcesManager.Instanse.LanguageChanged += OnLanguageChanged;
            Apply();
        }

        protected override void OnDetachingFrom(BoxView bindable)
        {
            LocalizationResourcesManager.Instanse.LanguageChanged -= OnLanguageChanged;
            _shade = null;
            base.OnDetachingFrom(bindable);
        }

        void OnLanguageChanged(CultureInfo culture)
        {
            MainThread.BeginInvokeOnMainThread(Apply);
        }

        void Apply()
        {
            if (_shade == null)
                return;

            var rtl = LocalizationResourcesManager.Instanse.GetFlowDirection() == FlowDirection.RightToLeft;
            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(rtl ? 1 : 0, 0.5),
                EndPoint = new Point(rtl ? 0 : 1, 0.5)
            };
            brush.GradientStops.Add(new GradientStop(Color.FromArgb("#E6000000"), 0));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb("#CC000000"), 0.35f));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb("#99000000"), 0.62f));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb("#33000000"), 0.85f));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb("#00000000"), 1));
            _shade.Background = brush;
        }
    }
}
