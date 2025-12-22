namespace MyProject;

public partial class HistoryPage : ContentPage
{
	public HistoryPage()
	{
		InitializeComponent();
        BindingContext = MovieViewModel.Shared; // Use the shared ViewModel

        HistoryCollectionView.ItemsSource = MovieViewModel.Shared.HistoryObservable;
    }

    private async void ClearHistoryButton_Clicked(object sender, EventArgs e)
    {
        await MovieViewModel.Shared.ClearHistoryAsync();
    }

}