using CommunityToolkit.Maui.Views;
using loukupm.Langue;
using loukupm.Model;
using loukupm.ViewModel;
using Microsoft.Maui.Controls.Shapes;

namespace loukupm.View;

public partial class LanguageSelectionPopup : Popup
{
    public LanguageSelectionPopup(
        string title,
        IEnumerable<SettingsViewModel.LanguageOption> languages,
        SettingsViewModel.LanguageOption? selected)
    {
        InitializeComponent();

        DialogRoot.FlowDirection = LocalizationResourcesManager.Instanse.GetFlowDirection();
        TitleLabel.Text = title;
        CancelButton.Text = ReminderChannelHelper.DismissText();

        foreach (var language in languages)
        {
            bool isSelected = selected != null
                && string.Equals(language.CultureName, selected.CultureName, StringComparison.OrdinalIgnoreCase);

            OptionsLayout.Children.Add(CreateLanguageRow(
                language.Name,
                language.Icon,
                isSelected,
                () => Close(language)));
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }

    private static Border CreateLanguageRow(string label, string icon, bool isSelected, Action onClick)
    {
        Color background = isSelected ? Color.FromArgb("#1A1813") : Color.FromArgb("#141414");
        Color text = isSelected ? Color.FromArgb("#E8C966") : Colors.White;
        Color border = isSelected ? Color.FromArgb("#D4AF37") : Color.FromArgb("#333333");

        var content = new Grid
        {
            ColumnSpacing = 12,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(new GridLength(18))
            }
        };

        content.Add(new Image
        {
            Source = icon,
            WidthRequest = 28,
            HeightRequest = 18,
            Aspect = Aspect.AspectFill,
            VerticalOptions = LayoutOptions.Center
        }, 0, 0);

        content.Add(new Label
        {
            Text = label,
            FontFamily = "Oswald",
            FontSize = 14,
            TextColor = text,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalOptions = LayoutOptions.Center
        }, 1, 0);

        content.Add(new Image
        {
            Source = "checkeseves",
            WidthRequest = 18,
            HeightRequest = 18,
            Opacity = isSelected ? 1 : 0,
            VerticalOptions = LayoutOptions.Center
        }, 2, 0);

        var row = new Border
        {
            BackgroundColor = background,
            Stroke = border,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Padding = new Thickness(14, 12),
            Content = content
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => onClick();
        row.GestureRecognizers.Add(tap);
        return row;
    }
}
