using CommunityToolkit.Maui.Views;
using loukupm.Model;

namespace loukupm.View;

public partial class ProviderSelectionPopup : Popup
{
    public ProviderSelectionPopup(string title, IEnumerable<WorkTeam> providers, WorkTeam? selected = null)
    {
        InitializeComponent();

        TitleLabel.Text = title;
        CancelButton.Text = ReminderChannelHelper.DismissText();

        foreach (var provider in providers)
        {
            bool isSelected = selected != null && provider.Id == selected.Id;
            OptionsLayout.Children.Add(ReminderTimePopup.CreateOptionRow(
                provider.Name,
                isSelected,
                () => Close(provider)));
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close(null);
    }
}
