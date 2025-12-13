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
       
        public ObservableCollection<Movie> Movies { get; set; } = new();
        public ObservableCollection<Movie> AllMovies { get; set; } = new();
        public ObservableCollection<Movie> FilteredMovies { get; set; } = new();

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
            LoadMoviesAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
                var movies = JsonSerializer.Deserialize<List<Movie>>(json);

                AllMovies = movies != null
                    ? new ObservableCollection<Movie>(movies)
                    : new ObservableCollection<Movie>();

                FilteredMovies = new ObservableCollection<Movie>(AllMovies);

                OnPropertyChanged(nameof(AllMovies));
                OnPropertyChanged(nameof(FilteredMovies));
            }

        }

        public void SortMovies(string sortOption)
        {
            IEnumerable<Movie> sorted = FilteredMovies;

            switch (sortOption)
            {
                case "Rating":
                    sorted = sorted.OrderByDescending(m => m.rating);
                    break;

                case "Year":
                    sorted = sorted.OrderByDescending(m => m.year);
                    break;

                case "Title":
                    sorted = sorted.OrderBy(m => m.title);
                    break;
            }

            FilteredMovies = new ObservableCollection<Movie>(sorted);
            OnPropertyChanged(nameof(FilteredMovies));
        }

    }
}
