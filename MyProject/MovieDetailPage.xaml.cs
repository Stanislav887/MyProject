using Microsoft.Maui.Controls;

namespace MyProject
{
    [QueryProperty(nameof(Movie), "Movie")]
    public partial class MovieDetailPage : ContentPage
    {
        private Movie _movie;

        // Expose the shared ViewModel
        public MovieViewModel ViewModel { get; set; }

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

        private async void FavoriteButton_Clicked(object sender, EventArgs e)
        {
            if (Movie != null)
            {
                await ViewModel.ToggleFavorite(Movie);
            }
        }


    }
}
