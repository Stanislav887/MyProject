using Microsoft.Maui.Controls;

namespace MyProject
{
    [QueryProperty(nameof(Movie), "Movie")]
    public partial class MovieDetailPage : ContentPage
    {
        private Movie _movie;

        // Expose the shared ViewModel
        public MovieViewModel ViewModel { get; }

        public Movie Movie
        {
            get => _movie;
            set
            {
                _movie = value;
                BindingContext = _movie;
            }
        }

        public MovieDetailPage(MovieViewModel viewModel)
        {
            InitializeComponent();

            // Assign the shared ViewModel
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
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
