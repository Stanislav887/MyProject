using Microsoft.Maui.Controls;

namespace MyProject;

public partial class UserNamePage : ContentPage
{
	public UserNamePage()
	{
		InitializeComponent();
	}

    private async void ContinueButton_Clicked(object sender, EventArgs e)
    {
        string name = NameEntry.Text?.Trim();

        if (!string.IsNullOrEmpty(name))
        {
            // Save the name to Preferences for later use
            Preferences.Default.Set("UserName", name);

            // Close the top page
            await Navigation.PopModalAsync();
        }
        else
        {
            await DisplayAlert("Error", "Please enter your name.", "OK");
        }
    }

}