using Microsoft.Maui.Controls;

namespace MyProject;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();

        // Load saved theme from Preferences
        string theme = Preferences.Default.Get("AppTheme", "System Default");
        ThemePicker.SelectedIndex = ThemePicker.Items.IndexOf(theme);
    }

    private void ThemePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        string selected = ThemePicker.SelectedItem.ToString();
        Preferences.Default.Set("AppTheme", selected);

        // Apply theme immediately
        switch (selected)
        {
            case "Light":
                Application.Current.UserAppTheme = AppTheme.Light;
                break;
            case "Dark":
                Application.Current.UserAppTheme = AppTheme.Dark;
                break;
            default:
                Application.Current.UserAppTheme = AppTheme.Unspecified;
                break;
        }
    }

    private async void ClearHistory_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Clear History",
            "Are you sure you want to clear your movie history?",
            "Yes",
            "Cancel");

        if (!confirm)
            return;

        await MovieViewModel.Shared.ClearHistoryAsync();

        await DisplayAlert(
            "History Cleared",
            "Your movie history has been cleared.",
            "OK");
    }


}