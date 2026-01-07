using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace MyProject
{
    public partial class MainPage : ContentPage
    {
        private MovieViewModel viewModel;

        public MainPage()
        {
            InitializeComponent();
            viewModel = MovieViewModel.Shared;
            BindingContext = viewModel;
        }

        // Navigate to the StatisticsPage when the Statistics button is clicked
        private async void StatisticsButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(StatisticsPage));
        }


        // Called when the SearchBar text changes
        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue ?? "";

            // Apply search using ViewModel method
            viewModel.ApplySearch(searchText);

        }

        // Called when a movie is selected in the main CollectionView
        private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected movie
            if (e.CurrentSelection.FirstOrDefault() is Movie selectedMovie)
            {
                // Prepare parameters to pass to the detail page
                var parameters = new Dictionary<string, object>
                {
                    { "Movie", selectedMovie }
                };

                // Navigate to MovieDetailPage with the selected movie
                await Shell.Current.GoToAsync(nameof(MovieDetailPage), parameters);

                // Deselect the item to allow re-selection later
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        // Called when the Sort Picker selection changes
        private void SortPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SortPicker.SelectedItem is string sortOption)
            {
                // Tell ViewModel to sort the movies
                viewModel.SortMovies(sortOption);

            }
        }

        // Called when the Sort Order button is clicked
        private void SortOrderButton_Clicked(object sender, EventArgs e)
        {
            // Toggle sort order in ViewModel
            viewModel.ToggleSortOrder();
        }

        // Called when the Favorite (star) button is clicked
        private async void FavoriteButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is Movie movie)
            {
                // Toggle favorite state in ViewModel
                await viewModel.ToggleFavorite(movie);

                // Check if animations are enabled in user preferences
                bool animationsEnabled = Preferences.Default.Get("AnimationsEnabled", true);

                if (animationsEnabled)
                {
                    // Animate: pop + rotate
                    await btn.ScaleTo(1.5, 100, Easing.CubicOut); // grow
                    await btn.RotateTo(20, 100, Easing.CubicIn);   // rotate slightly
                    await btn.RotateTo(-20, 100, Easing.CubicIn);
                    await btn.RotateTo(0, 100, Easing.CubicIn);    // reset rotation
                    await btn.ScaleTo(1.0, 100, Easing.CubicIn);   // shrink back


                    // Animate the emoji label
                    if (btn.Parent is Grid grid)
                    {
                        var emojiLabel = grid.Children
                                             .OfType<Label>()
                                             .FirstOrDefault(l => l.ClassId == "MovieEmojiLabel");
                        if (emojiLabel != null)
                        {
                            await emojiLabel.ScaleTo(2, 250, Easing.CubicIn);
                            await emojiLabel.ScaleTo(1, 250, Easing.CubicOut);
                        }
                    }
                }
            }
        }

        // Navigate to HistoryPage when the History button is clicked
        private async void HistoryButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(HistoryPage));
        }

        // Called when the Director filter Entry text changes
        private void DirectorFilterEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            string directorText = e.NewTextValue ?? "";
            string searchText = MovieSearchBar.Text ?? "";

            // Apply combined search and director filter in ViewModel
            viewModel.ApplySearch(searchText, directorText);
        }

        // Called when a movie frame is tapped (either in top 10 or main list)
        private async void MovieFrame_Tapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is Movie movie)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "Movie", movie }
                };
                // Navigate to MovieDetailPage with the tapped movie
                await Shell.Current.GoToAsync(nameof(MovieDetailPage), parameters);
            }
        }

    }
}
