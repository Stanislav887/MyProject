using Microsoft.Maui.Controls;

namespace MyProject
{
    [QueryProperty(nameof(Movie), "Movie")]
    public partial class MovieDetailPage : ContentPage
    {
        private Movie _movie;

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
    }
}
