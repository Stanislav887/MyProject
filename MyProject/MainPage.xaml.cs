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

    }
}
