using Microsoft.Maui.Controls;

namespace MyProject;

public partial class SettingsPage : ContentPage
{
    // Expose the current username for binding in XAML
    public string UserName =>
        Preferences.Default.Get("UserName", "Not set");

    public SettingsPage()
	{
		InitializeComponent();

        // Set the page's BindingContext to the shared MovieViewModel
        BindingContext = MovieViewModel.Shared;

        // Load saved theme from Preferences
        string theme = Preferences.Default.Get("AppTheme", "System Default");
        ThemePicker.SelectedIndex = ThemePicker.Items.IndexOf(theme);

        // Set AnimationsSwitch to saved preference
        AnimationsSwitch.IsToggled = Preferences.Default.Get("AnimationsEnabled", true);
    }

    // Event handler for the animations toggle switch
    private void AnimationsSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        bool enabled = e.Value;

        // Save user preference for animations
        Preferences.Default.Set("AnimationsEnabled", enabled);
    }

    // Event handler when user changes the app theme from Picker
    private void ThemePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        string selected = ThemePicker.SelectedItem.ToString();

        // Save the selected theme to Preferences
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

    // Clears the user's movie history
    private async void ClearHistory_Clicked(object sender, EventArgs e)
    {
        // Confirm with user before clearing
        bool confirm = await DisplayAlert(
            "Clear History",
            "Are you sure you want to clear your movie history?",
            "Yes",
            "Cancel");

        if (!confirm)
            return;

        // Call ViewModel to clear history
        await MovieViewModel.Shared.ClearHistoryAsync();

        // Notify user that history is cleared
        await DisplayAlert(
            "History Cleared",
            "Your movie history has been cleared.",
            "OK");
    }

    // Resets the username by removing it from Preferences
    private async void ResetUsername_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Reset Username",
            "Are you sure you want to reset your username?",
            "Yes",
            "Cancel");

        if (!confirm)
            return;

        // Remove saved username
        Preferences.Default.Remove("UserName");

        await DisplayAlert(
            "Username Reset",
            "Username has been cleared. Restart the app to set a new one.",
            "OK");
    }

    // Allows user to change their username with validation
    private async void ChangeUsername_Clicked(object sender, EventArgs e)
    {
        // Prompt user to enter new username
        string result = await DisplayPromptAsync(
            "Change Username",
            "Enter new username:");

        // Cannot be empty
        if (string.IsNullOrWhiteSpace(result))
        {
            await DisplayAlert("Invalid Username", "Username cannot be empty.", "OK");
            return;
        }

        //Maximum Length 
        if (result.Length > 20)
        {
            await DisplayAlert("Invalid Username", "Username cannot be longer than 20 characters.", "OK");
            return;
        }

        // Invalid characters (allow only letters, numbers, underscores)
        if (!System.Text.RegularExpressions.Regex.IsMatch(result, @"^[a-zA-Z0-9_]+$"))
        {
            await DisplayAlert("Invalid Username", "Username can only contain letters, numbers, and underscores.", "OK");
            return;
        }

        // Save valid username
        Preferences.Default.Set("UserName", result.Trim());

        // Notify ViewModel so UI updates (including GreetingMessage)
        MovieViewModel.Shared.NotifyUsernameChanged();

        // Update local binding (SettingsPage)
        OnPropertyChanged(nameof(UserName));

    }

    // Updates the user's avatar when a new one is selected from Picker
    private void AvatarPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Ignore if nothing is selected
        if (AvatarPicker.SelectedIndex == -1)
            return;

        // Get the selected emoji
        string selectedEmoji = AvatarPicker.Items[AvatarPicker.SelectedIndex];
        
        // Update ViewModel 
        MovieViewModel.Shared.UserEmoji = selectedEmoji;
    }

}