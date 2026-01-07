using Microsoft.Maui.Controls;

namespace MyProject
{
    [QueryProperty(nameof(Movie), "Movie")]
    public partial class MovieDetailPage : ContentPage
    {
        private Movie _movie;

        // Expose the shared ViewModel
        public MovieViewModel ViewModel { get; set; }

        // Property bound to the MovieDetailPage
        // Setting this updates the BindingContext so the UI displays the correct movie
        public Movie Movie
        {
            get => _movie;
            set
            {
                _movie = value;
                BindingContext = _movie;

                if (_movie != null && ViewModel != null)
                {
                    _ = ViewModel.RecordViewedAsync(_movie);
                }
            }
        }

        public MovieDetailPage()
        {
            InitializeComponent();

            // Use the shared singleton instance
            ViewModel = MovieViewModel.Shared;
        }

        // Called when the Favorite (star) button is clicked
        private async void FavoriteButton_Clicked(object sender, EventArgs e)
        {
            if (Movie != null)
            {
                // Toggle favorite status using the ViewModel
                await ViewModel.ToggleFavorite(Movie);
            }
        }


    }
}
