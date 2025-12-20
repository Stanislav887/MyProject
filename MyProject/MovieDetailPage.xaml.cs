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
            }
        }

        public MovieDetailPage()
        {
            InitializeComponent();
        }

        public MovieDetailPage(MovieViewModel viewModel)
        {
            InitializeComponent();

            // Assign the shared ViewModel
            ViewModel = viewModel;
        }

    }
}
