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
}