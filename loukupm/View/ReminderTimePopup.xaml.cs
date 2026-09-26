using CommunityToolkit.Maui.Views;
using loukupm.Model;
using Microsoft.Maui.Controls.Shapes;

namespace loukupm.View;

public sealed class ReminderTimePopupResult
{
    public bool TurnOff { get; init; }

    public ReminderOption? Option { get; init; }
}

public partial class ReminderTimePopup : Popup
{
    public ReminderTimePopup(
        string title,
        IEnumerable<ReminderOption> options,
        ReminderOption? selected = null,
        bool includeTurnOff = false)
    {
        InitializeComponent();

        TitleLabel.Text = title;
        CancelButton.Text = ReminderChannelHelper.DismissText();

        foreach (var option in options)
        {
            bool isSelected = selected != null && option.OffsetHours == selected.OffsetHours;
            OptionsLayout.Children.Add(CreateOptionRow(
                option.Label,
                isSelected,
                () => Close(new ReminderTimePopupResult { Option = option })));
        }

        if (includeTurnOff)
        {
            OptionsLayout.Children.Add(CreateOptionRow(
                ReminderChannelHelper.TurnOffText(),
                false,
                () => Close(new ReminderTimePopupResult { TurnOff = true }),
                isDestructive: true));
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }

    internal static Border CreateOptionRow(string label, bool isSelected, Action onClick, bool isDestructive = false)
    {
        Color background;
        Color text;
        Color border;

        if (isDestructive)
        {
            background = Color.FromArgb("#1A0D0D");
            text = Color.FromArgb("#FF6B6B");
            border = Color.FromArgb("#3A2222");
        }
        else if (isSelected)
        {
            background = Color.FromArgb("#1A1813");
            text = Color.FromArgb("#E8C966");
            border = Color.FromArgb("#D4AF37");
        }
        else
        {
            background = Color.FromArgb("#141414");
            text = Colors.White;
            border = Color.FromArgb("#333333");
        }

        var row = new Border
        {
            BackgroundColor = background,
            Stroke = border,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = new Thickness(14, 12),
            Content = new Label
            {
                Text = label,
                FontFamily = "Oswald",
                FontSize = 14,
                TextColor = text,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => onClick();
        row.GestureRecognizers.Add(tap);
        return row;
    }
}
