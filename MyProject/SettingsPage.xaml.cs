using Microsoft.Maui.Controls;

namespace MyProject;

public partial class SettingsPage : ContentPage
{
    // Existing properties
    public string UserName =>
        Preferences.Default.Get("UserName", "Not set");

    // Add this property for the emoji avatar
    public string UserEmoji =>
        Preferences.Default.Get("UserEmoji", "🎬"); // default movie emoji
    public SettingsPage()
	{
		InitializeComponent();

        BindingContext = this;

        // Load saved theme from Preferences
        string theme = Preferences.Default.Get("AppTheme", "System Default");
        ThemePicker.SelectedIndex = ThemePicker.Items.IndexOf(theme);

        // Set AnimationsSwitch to saved preference
        AnimationsSwitch.IsToggled = Preferences.Default.Get("AnimationsEnabled", true);
    }

    private void AnimationsSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        bool enabled = e.Value;
        Preferences.Default.Set("AnimationsEnabled", enabled);
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

    private async void ResetUsername_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Reset Username",
            "Are you sure you want to reset your username?",
            "Yes",
            "Cancel");

        if (!confirm)
            return;

        Preferences.Default.Remove("UserName");

        await DisplayAlert(
            "Username Reset",
            "Username has been cleared. Restart the app to set a new one.",
            "OK");
    }


    private async void ChangeUsername_Clicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync(
            "Change Username",
            "Enter new username:");

        if (!string.IsNullOrWhiteSpace(result))
        {
            Preferences.Default.Set("UserName", result.Trim());
            OnPropertyChanged(nameof(UserName));
        }
    }

}