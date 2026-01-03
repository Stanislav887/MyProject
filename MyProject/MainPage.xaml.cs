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

        private async void ShowHint(string text)
        {
            HintLabel.Text = text;       // Set the hint text
            HintFrame.Opacity = 0;       // Start fully transparent
            HintFrame.IsVisible = true;  // Show the frame + label

            await HintFrame.FadeTo(0.8, 250);  // Fade in to 80% opacity in 250ms

            await Task.Delay(1500);      // Keep it visible for 1.5 seconds

            await HintFrame.FadeTo(0, 250);    // Fade out to transparent in 250ms

            HintFrame.IsVisible = false; // Hide it
        }

        private void FavoriteButton_Pressed(object sender, EventArgs e)
        {
            ShowHint("Mark this movie as favorite");
        }

        private void SortOrderButton_Pressed(object sender, EventArgs e)
        {
            ShowHint("Toggle sort order ascending/descending");
        }

        private void HistoryButton_Pressed(object sender, EventArgs e)
        {
            ShowHint("View your movie history");
        }

        private void FavoriteButton_PointerEntered(object sender, PointerEventArgs e)
        {
            ShowHint("Mark this movie as favorite");
        }

        private void FavoriteButton_PointerExited(object sender, PointerEventArgs e)
        {
            HintFrame.IsVisible = false;
        }

        private void SortOrderButton_PointerEntered(object sender, PointerEventArgs e)
        {
            ShowHint("Toggle sort order ascending/descending");
        }

        private void HistoryButton_PointerEntered(object sender, PointerEventArgs e)
        {
            ShowHint("View your movie history");
        }

        private void PointerExited_HideHint(object sender, PointerEventArgs e)
        {
            HintFrame.IsVisible = false;
        }

        private async void StatisticsButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(StatisticsPage));
        }


        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue ?? "";

            // Apply search using ViewModel method
            viewModel.ApplySearch(searchText);

        }


        private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Movie selectedMovie)
            {
                var parameters = new Dictionary<string, object>
                {
                    { "Movie", selectedMovie }
                };

                await Shell.Current.GoToAsync(nameof(MovieDetailPage), parameters);

                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private void SortPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SortPicker.SelectedItem is string sortOption)
            {
                viewModel.SortMovies(sortOption);

            }
        }

        private void SortOrderButton_Clicked(object sender, EventArgs e)
        {
            // Toggle sort order in ViewModel
            viewModel.ToggleSortOrder();

        }

        private async void FavoriteButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.BindingContext is Movie movie)
            {
                await viewModel.ToggleFavorite(movie);

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

        private async void HistoryButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(HistoryPage));
        }


        private void DirectorFilterEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            string directorText = e.NewTextValue ?? "";
            string searchText = MovieSearchBar.Text ?? "";

            viewModel.ApplySearch(searchText, directorText);
        }

    }
}
