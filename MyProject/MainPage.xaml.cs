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
            }
        }

        private async void HistoryButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(HistoryPage));
        }

    }
}
