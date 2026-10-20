using System.Globalization;
using loukupm.Langue;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace loukupm.Behaviors
{
    /// <summary>
    /// Grows the active pager dot into a yellow pill and shrinks the others into silver circles.
    /// </summary>
    public class PagerDotBehavior : Behavior<Border>
    {
        static readonly Color ActiveColor = Color.FromArgb("#F5B713");
        static readonly Color IdleColor = Color.FromArgb("#C5C8CC");
        const double ActiveWidth = 28;
        const double IdleWidth = 10;
        const string AnimationName = "pagerDot";

        public static readonly BindableProperty IsActiveProperty =
            BindableProperty.Create(
                nameof(IsActive),
                typeof(bool),
                typeof(PagerDotBehavior),
                false,
                propertyChanged: (bindable, _, newValue) =>
                    ((PagerDotBehavior)bindable).Update(newValue is true, animate: true));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        Border _dot;

        protected override void OnAttachedTo(Border bindable)
        {
            base.OnAttachedTo(bindable);
            _dot = bindable;
            Update(IsActive, animate: false);
            bindable.BindingContextChanged += OnDotBindingContextChanged;
            LocalizationResourcesManager.Instanse.LanguageChanged += OnLanguageChanged;
            BindingContext = bindable.BindingContext;
        }

        protected override void OnDetachingFrom(Border bindable)
        {
            bindable.BindingContextChanged -= OnDotBindingContextChanged;
            LocalizationResourcesManager.Instanse.LanguageChanged -= OnLanguageChanged;
            bindable.AbortAnimation(AnimationName);
            _dot = null;
            base.OnDetachingFrom(bindable);
        }

        void OnLanguageChanged(CultureInfo culture)
        {
            MainThread.BeginInvokeOnMainThread(() => Update(IsActive, animate: false));
        }

        void OnDotBindingContextChanged(object sender, EventArgs e)
        {
            if (_dot != null)
                BindingContext = _dot.BindingContext;
        }

        void Update(bool active, bool animate)
        {
            if (_dot == null)
                return;

            var targetWidth = active ? ActiveWidth : IdleWidth;
            var targetColor = active ? ActiveColor : IdleColor;

            _dot.AbortAnimation(AnimationName);

            if (!animate)
            {
                _dot.WidthRequest = targetWidth;
                _dot.BackgroundColor = targetColor;
                return;
            }

            var fromWidth = _dot.Width > 0 ? _dot.Width : _dot.WidthRequest;
            var fromColor = _dot.BackgroundColor ?? IdleColor;

            var animation = new Animation(progress =>
            {
                _dot.WidthRequest = fromWidth + (targetWidth - fromWidth) * progress;
                _dot.BackgroundColor = Color.FromRgba(
                    fromColor.Red + (targetColor.Red - fromColor.Red) * progress,
                    fromColor.Green + (targetColor.Green - fromColor.Green) * progress,
                    fromColor.Blue + (targetColor.Blue - fromColor.Blue) * progress,
                    1);
            });

            animation.Commit(_dot, AnimationName, 16, 220, Easing.CubicOut);
        }
    }
}
