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
            viewModel = new MovieViewModel();
            BindingContext = viewModel;
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Show all movies if search text is empty
                MoviesCollectionView.ItemsSource = viewModel.Movies;
                return;
            }

            // Filter movies by title, director, year, or genreString
            var filtered = viewModel.Movies.Where(movie =>
                movie.title.ToLower().Contains(searchText) ||
                movie.director.ToLower().Contains(searchText) ||
                movie.year.ToString().Contains(searchText) ||
                movie.genreString.ToLower().Contains(searchText)
            ).ToList();

            MoviesCollectionView.ItemsSource = filtered;
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

    }
}
