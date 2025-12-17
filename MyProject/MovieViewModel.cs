using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.IO;
using Microsoft.Maui.Storage;

namespace MyProject
{
    internal class MovieViewModel : INotifyPropertyChanged
    {
        private string cacheFileName = "movies.json";
        private Movie _selectedMovie;
       
        private List<Movie> AllMovies = new();
        public ObservableCollection<Movie> FilteredMovies { get; set; } = new();

        public string CurrentSortOption { get; set; }
        public bool SortAscending { get; set; }

        public string SortOrderText
        {
            get => SortAscending ? "🔼 Ascending" : "🔽 Descending";
        }

        public Movie SelectedMovie
        {
            get => _selectedMovie;
            set
            {
                if (_selectedMovie != value)
                {
                    _selectedMovie = value;
                    OnPropertyChanged();
                }
            }
        }

        public MovieViewModel()
        {
            CurrentSortOption = Preferences.Default.Get("SortOption", "Rating");
            SortAscending = Preferences.Default.Get("SortAscending", false);

            LoadMoviesAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void LoadMoviesAsync()
        {
            string json = string.Empty;
            string localPath = Path.Combine(FileSystem.AppDataDirectory, cacheFileName);

            if (File.Exists(localPath))
            {
                json = await File.ReadAllTextAsync(localPath);
            }
            else
            {
                try
                {
                    using HttpClient client = new();
                    json = await client.GetStringAsync("https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/moviesemoji.json");
                    await File.WriteAllTextAsync(localPath, json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error downloading JSON: {ex.Message}");
                }
            }

            // Deserialize and populate list
            if (!string.IsNullOrEmpty(json))
            {
                var movies = JsonSerializer.Deserialize<List<Movie>>(json) ?? new List<Movie>();
                AllMovies = movies;
                FilteredMovies = new ObservableCollection<Movie>(AllMovies);

                // Apply persisted sort
                SortMovies(CurrentSortOption);

                OnPropertyChanged(nameof(FilteredMovies));
            }

        }

        public void ApplySearch(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                FilteredMovies = new ObservableCollection<Movie>(AllMovies);
            }
            else
            {
                FilteredMovies = new ObservableCollection<Movie>(
                    AllMovies.Where(movie =>
                        movie.title.ToLower().Contains(searchText.ToLower()) ||
                        movie.director.ToLower().Contains(searchText.ToLower()) ||
                        movie.year.ToString().Contains(searchText) ||
                        movie.genreString.ToLower().Contains(searchText.ToLower())
                    )
                );
            }
            SortMovies(CurrentSortOption);
            OnPropertyChanged(nameof(FilteredMovies));
        }

        public void SortMovies(string sortOption)
        {
            CurrentSortOption = sortOption;
            IEnumerable<Movie> sorted = FilteredMovies;

            switch (CurrentSortOption)
            {
                case "Rating":
                    sorted = SortAscending
                        ? sorted.OrderBy(m => m.rating)
                        : sorted.OrderByDescending(m => m.rating);
                    break;

                case "Year":
                    sorted = SortAscending
                        ? sorted.OrderBy(m => m.year)
                        : sorted.OrderByDescending(m => m.year);
                    break;

                case "Title":
                    sorted = SortAscending
                        ? sorted.OrderBy(m => m.title)
                        : sorted.OrderByDescending(m => m.title);
                    break;
            }
            
            Preferences.Default.Set("SortOption", CurrentSortOption);
            Preferences.Default.Set("SortAscending", SortAscending);

            FilteredMovies = new ObservableCollection<Movie>(sorted);
            OnPropertyChanged(nameof(FilteredMovies));
        }

        public void ToggleSortOrder()
        {
            SortAscending = !SortAscending;


            OnPropertyChanged(nameof(SortOrderText));

            SortMovies(CurrentSortOption);
        }

        public void ToggleFavorite(Movie movie)
        {
            movie.IsFavorite = !movie.IsFavorite;
            OnPropertyChanged(nameof(FilteredMovies));
        }




    }
}
