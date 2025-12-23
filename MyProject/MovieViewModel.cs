using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.IO;
using Microsoft.Maui.Storage;

namespace MyProject
{
    public class MovieViewModel : INotifyPropertyChanged
    {
        private string cacheFileName = "movies.json";
        private Movie _selectedMovie;
        private bool _showFavoritesOnly;
        private string favoritesFileName = "favorites.json";
        private string historyFileName = "history.json";

        private List<Movie> AllMovies = new();

        private List<MovieHistoryEntry> History = new();
        public ObservableCollection<Movie> FilteredMovies { get; set; } = new();
        public static MovieViewModel Shared { get; } = new MovieViewModel();
        public ObservableCollection<MovieHistoryEntry> HistoryObservable { get; private set; } = new ObservableCollection<MovieHistoryEntry>();
        public ObservableCollection<HistoryGroup> GroupedHistory { get; private set; }= new ObservableCollection<HistoryGroup>();
        public string CurrentSortOption { get; set; }
        public bool SortAscending { get; set; }
        public string CurrentUser { get; set; }

        public string SortOrderText
        {
            get => SortAscending ? "🔼 Ascending" : "🔽 Descending";
        }

        public bool ShowFavoritesOnly
        {
            get => _showFavoritesOnly;
            set
            {
                if (_showFavoritesOnly != value)
                {
                    _showFavoritesOnly = value;
                    ApplySearch(string.Empty); // reapply filtering
                    OnPropertyChanged();
                }
            }
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
            _ = LoadHistoryAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
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

                await LoadFavoritesAsync();

                FilteredMovies = new ObservableCollection<Movie>(AllMovies);

                // Apply persisted sort
                SortMovies(CurrentSortOption);

                OnPropertyChanged(nameof(FilteredMovies));
            }

        }

        private async Task LoadFavoritesAsync()
        {
            string path = Path.Combine(FileSystem.AppDataDirectory, favoritesFileName);
            if (!File.Exists(path)) return;

            try
            {
                string json = await File.ReadAllTextAsync(path);
                var favorites = JsonSerializer.Deserialize<List<Movie>>(json) ?? new List<Movie>();

                var favoriteSet = new HashSet<(string title, int year)>(favorites.Select(f => (f.title, f.year)));
                AllMovies.ForEach(movie => movie.IsFavorite = favoriteSet.Contains((movie.title, movie.year)));

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading favorites: {ex.Message}");
            }
        }

        private async Task LoadHistoryAsync()
        {
            string path = Path.Combine(FileSystem.AppDataDirectory, historyFileName);
            if (!File.Exists(path)) return;

            try
            {
                string json = await File.ReadAllTextAsync(path);
                History = JsonSerializer.Deserialize<List<MovieHistoryEntry>>(json)
                          ?? new List<MovieHistoryEntry>();

                HistoryObservable = new ObservableCollection<MovieHistoryEntry>(History);
                BuildGroupedHistory();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading history: {ex.Message}");
            }
        }

        private async Task SaveHistoryAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(History);
                string path = Path.Combine(FileSystem.AppDataDirectory, historyFileName);
                await File.WriteAllTextAsync(path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving history: {ex.Message}");
            }
        }

        private async Task AddHistoryEntryAsync(Movie movie, string action)
        {
            var entry = new MovieHistoryEntry
            {
                Title = movie.title,
                Year = movie.year,
                Genre = movie.genreString,
                Emoji = movie.emoji,
                Timestamp = DateTime.Now,
                Action = action
            };

            History.Add(entry);                    // Keep for saving to file
            HistoryObservable.Add(entry);          // For UI updates

            BuildGroupedHistory();

            await SaveHistoryAsync();
        }

        public void ApplySearch(string searchText)
        {
            IEnumerable<Movie> query = AllMovies;


            // Apply favorites filter
            if (ShowFavoritesOnly)
            {
                query = query.Where(movie => movie.IsFavorite);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                searchText = searchText.ToLower();

                query = query.Where(movie =>
                    movie.title.ToLower().Contains(searchText) ||
                    movie.director.ToLower().Contains(searchText) ||
                    movie.year.ToString().Contains(searchText) ||
                    movie.genreString.ToLower().Contains(searchText)
                );
            }

            FilteredMovies = new ObservableCollection<Movie>(query);

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

        public async Task ToggleFavorite(Movie movie)
        {
            movie.IsFavorite = !movie.IsFavorite;

            // Save the updated favorites to file
            await SaveFavoritesAsync();

            // Record the history
            if (movie.IsFavorite)
                await RecordFavoritedAsync(movie);
            else
                await RecordUnfavoritedAsync(movie);
        }

        // Public method to record a viewed movie
        public async Task RecordViewedAsync(Movie movie)
        {
            await AddHistoryEntryAsync(movie, "Viewed");
        }

        // Public method to record favorited movie
        public async Task RecordFavoritedAsync(Movie movie)
        {
            await AddHistoryEntryAsync(movie, "Favorited");
        }

        // Public method to record unfavorited movie
        public async Task RecordUnfavoritedAsync(Movie movie)
        {
            await AddHistoryEntryAsync(movie, "Unfavorited");
        }

        public async Task ClearHistoryAsync()
        {
            History.Clear();
            HistoryObservable.Clear();
            GroupedHistory.Clear();
            await SaveHistoryAsync();
        }

        private async Task SaveFavoritesAsync()
        {
            try
            {
                // Only save movies that are currently favorites
                var favorites = AllMovies.Where(m => m.IsFavorite).ToList();
                string json = JsonSerializer.Serialize(favorites);

                string path = Path.Combine(FileSystem.AppDataDirectory, favoritesFileName);
                await File.WriteAllTextAsync(path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving favorites: {ex.Message}");
            }
        }

        private void BuildGroupedHistory()
        {
            GroupedHistory.Clear();

            var grouped = History
                .OrderByDescending(h => h.Timestamp)
                .GroupBy(h => h.Timestamp.Date);

            foreach (var group in grouped)
            {
                string dateLabel =
                    group.Key == DateTime.Today ? "Today" :
                    group.Key == DateTime.Today.AddDays(-1) ? "Yesterday" :
                    group.Key.ToString("yyyy-MM-dd");

                GroupedHistory.Add(
                    new HistoryGroup(
                        dateLabel,
                        new ObservableCollection<MovieHistoryEntry>(group)
                    )
                );
            }
        }


    }
}
