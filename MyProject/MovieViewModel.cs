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
        // File names for caching movies, favorites, and history
        private string cacheFileName = "movies.json";
        private string favoritesFileName = "favorites.json";
        private string historyFileName = "history.json";

        // Per-user file paths (appends username)
        private string FavoritesFileName => $"{CurrentUser}_favorites.json";
        private string HistoryFileName => $"{CurrentUser}_history.json";

        // Backing field for the currently selected movie
        private Movie _selectedMovie;

        // Backing field for the "favorites only" toggle
        private bool _showFavoritesOnly;

        // All movies loaded from JSON 
        private List<Movie> AllMovies = new();

        // Raw history list
        private List<MovieHistoryEntry> History = new();
        
        // Filtered list of movies (bound to CollectionView)
        public ObservableCollection<Movie> FilteredMovies { get; set; } = new();

        // Singleton instance for easy shared access across pages
        public static MovieViewModel Shared { get; } = new MovieViewModel();

        // Stats collections for UI
        public ObservableCollection<GenreStat> GenreStats { get; } = new();
        public ObservableCollection<MovieHistoryEntry> HistoryObservable { get; private set; } = new ObservableCollection<MovieHistoryEntry>();
        public ObservableCollection<HistoryGroup> GroupedHistory { get; private set; }= new ObservableCollection<HistoryGroup>();

        // Top 10 movies collection
        private ObservableCollection<Movie> topMovies = new();
        public ObservableCollection<Movie> TopMovies
        {
            get => topMovies;
            set
            {
                if (topMovies != value)
                {
                    topMovies = value;
                    OnPropertyChanged();
                }
            }
        }

        // Current sort option ("Rating", "Year", "Title")
        public string CurrentSortOption { get; set; }

        // Whether sorting is ascending or descending
        public bool SortAscending { get; set; }

        // Currently logged-in user
        public string CurrentUser { get; set; }

        // Simple UI helpers
        public bool HasMovies => FilteredMovies?.Any() == true;
        public bool HasFavorites => FilteredMovies?.Any(m => m.IsFavorite) == true;
        public bool HasHistory => HistoryObservable?.Any() == true;

        // Aggregated stats
        public int TotalFavorites => FilteredMoviesByTimeRange().Count(m => m.IsFavorite);

        public string MostWatchedGenre =>
            FilteredMoviesByTimeRange()
                .GroupBy(m => m.genreString)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

        public double AverageRating => FilteredMoviesByTimeRange().Any()
            ? FilteredMoviesByTimeRange().Average(m => m.rating)
            : 0;

        public string TopDirector =>
            FilteredMoviesByTimeRange()
                .GroupBy(m => m.director)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "N/A";

        // Pull-to-refresh command for UI
        public Command RefreshMoviesCommand { get; }

        // UI greeting message
        public string UserTitle => string.IsNullOrWhiteSpace(CurrentUser)
                           ? "Movies"
                           : $"{CurrentUser}'s Movies";

        private string selectedTimeRange = "All Time";
        public string SelectedTimeRange
        {
            get => selectedTimeRange;
            set
            {
                if (selectedTimeRange != value)
                {
                    selectedTimeRange = value;
                    OnPropertyChanged();

                    // Update dependent stats
                    OnPropertyChanged(nameof(TotalFavorites));
                    OnPropertyChanged(nameof(MostWatchedGenre));
                    OnPropertyChanged(nameof(AverageRating));
                    OnPropertyChanged(nameof(TopDirector));
                }
            }
        }

        // Filter movies according to selected time range
        private IEnumerable<Movie> FilteredMoviesByTimeRange()
        {
            DateTime now = DateTime.Now;

            return selectedTimeRange switch
            {
                "Last Month" => AllMovies.Where(m => m.DateAdded >= now.AddMonths(-1)),
                "Last Year" => AllMovies.Where(m => m.DateAdded >= now.AddYears(-1)),
                _ => AllMovies
            };
        }

        // Pull-to-refresh state
        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        // Emoji shown for the current user
        public string UserEmoji
        {
            get => Preferences.Default.Get("UserEmoji", "🎬");
            set
            {
                Preferences.Default.Set("UserEmoji", value);
                OnPropertyChanged();
            }
        }

        // Greeting message for the user
        public string GreetingMessage => $"Hello, {Preferences.Default.Get("UserName", CurrentUser)}!";

        // Sort order button text
        public string SortOrderText
        {
            get => SortAscending ? "🔼 Ascending" : "🔽 Descending";
        }

        // Favorites-only toggle
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

        // Currently selected movie
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
            // Load current user and preferences
            CurrentUser = Preferences.Default.Get("UserName", "defaultUser");
            CurrentSortOption = Preferences.Default.Get("SortOption", "Rating");
            SortAscending = Preferences.Default.Get("SortAscending", false);

            // Pull-to-refresh command
            RefreshMoviesCommand = new Command(async () =>
            {
                IsRefreshing = true;

                await LoadMoviesAsync();
                await LoadHistoryAsync();

                // Small delay so spinner is visible
                await Task.Delay(500);

                IsRefreshing = false;
            });

            // Load movies and history on startup (fire-and-forget)
            _ = LoadMoviesAsync();
            _ = LoadHistoryAsync();
        }

        // Notify UI that username has changed
        public void NotifyUsernameChanged()
        {
            CurrentUser = Preferences.Default.Get("UserName", "Guest");
            OnPropertyChanged(nameof(CurrentUser));
            OnPropertyChanged(nameof(UserTitle));
            OnPropertyChanged(nameof(GreetingMessage));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Load movies from local cache or online JSON
        private async Task LoadMoviesAsync()
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

                // Ensure every movie has a DateAdded value
                foreach (var movie in movies)
                {
                    if (movie.DateAdded == default)
                        movie.DateAdded = DateTime.Now;
                }

                AllMovies = movies;

                await LoadFavoritesAsync();

                FilteredMovies = new ObservableCollection<Movie>(AllMovies);

                // Update top movies
                UpdateTopMovies();

                // Apply persisted sort
                SortMovies(CurrentSortOption);

                OnPropertyChanged(nameof(FilteredMovies));
                OnPropertyChanged(nameof(HasMovies));
                OnPropertyChanged(nameof(HasFavorites));
                
                OnPropertyChanged(nameof(TotalFavorites));
                OnPropertyChanged(nameof(MostWatchedGenre));
                OnPropertyChanged(nameof(AverageRating));
                OnPropertyChanged(nameof(TopDirector));
            }

        }

        // Load favorites from file
        private async Task LoadFavoritesAsync()
        {
            string path = Path.Combine(FileSystem.AppDataDirectory, FavoritesFileName);
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

        // Load history from file
        private async Task LoadHistoryAsync()
        {
            string path = Path.Combine(FileSystem.AppDataDirectory, HistoryFileName);
            if (!File.Exists(path)) return;

            try
            {
                string json = await File.ReadAllTextAsync(path);
                History = JsonSerializer.Deserialize<List<MovieHistoryEntry>>(json)
                          ?? new List<MovieHistoryEntry>();

                HistoryObservable = new ObservableCollection<MovieHistoryEntry>(History);
                BuildGroupedHistory();
                BuildGenreStats();
                OnPropertyChanged(nameof(HistoryObservable));
                OnPropertyChanged(nameof(HasHistory));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading history: {ex.Message}");
            }
        }

        // Save history to file
        private async Task SaveHistoryAsync()
        {
            try
            {
                string json = JsonSerializer.Serialize(History);
                string path = Path.Combine(FileSystem.AppDataDirectory, HistoryFileName);
                await File.WriteAllTextAsync(path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving history: {ex.Message}");
            }
        }

        // Add a new history entry
        private async Task AddHistoryEntryAsync(Movie movie, string action)
        {
            var entry = new MovieHistoryEntry
            {
                Title = movie.title,
                Year = movie.year,
                Genre = movie.genre,
                Emoji = movie.emoji,
                Timestamp = DateTime.Now,
                Action = action
            };

            History.Add(entry);                    // Keep for saving to file
            HistoryObservable.Add(entry);          // For UI updates

            BuildGroupedHistory();
            BuildGenreStats();

            OnPropertyChanged(nameof(HasHistory));

            await SaveHistoryAsync();
        }

        // Filter movies by search text and director
        public void ApplySearch(string searchText, string directorFilter = "")
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

            // Apply director-specific filter
            if (!string.IsNullOrWhiteSpace(directorFilter))
            {
                string lowerDirector = directorFilter.ToLower();
                query = query.Where(movie => movie.director.ToLower().Contains(lowerDirector));
            }

            FilteredMovies = new ObservableCollection<Movie>(query);

            SortMovies(CurrentSortOption);
            OnPropertyChanged(nameof(FilteredMovies));
            OnPropertyChanged(nameof(HasMovies));
            OnPropertyChanged(nameof(HasFavorites));
        }

        // Sort movies
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
            OnPropertyChanged(nameof(HasMovies));
            OnPropertyChanged(nameof(HasFavorites));
        }

        // Toggle ascending/descending
        public void ToggleSortOrder()
        {
            SortAscending = !SortAscending;


            OnPropertyChanged(nameof(SortOrderText));

            SortMovies(CurrentSortOption);
        }

        // Toggle favorite for a movie
        public async Task ToggleFavorite(Movie movie)
        {
            movie.IsFavorite = !movie.IsFavorite;
            OnPropertyChanged(nameof(HasFavorites));

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

        // Clear history
        public async Task ClearHistoryAsync()
        {
            History.Clear();
            HistoryObservable.Clear();
            GroupedHistory.Clear();
            GenreStats.Clear();   // Clear emoji chart data
            await SaveHistoryAsync();
            OnPropertyChanged(nameof(HasHistory));
        }

        // Save favorite movies to file
        private async Task SaveFavoritesAsync()
        {
            try
            {
                // Only save movies that are currently favorites
                var favorites = AllMovies.Where(m => m.IsFavorite).ToList();
                string json = JsonSerializer.Serialize(favorites);

                string path = Path.Combine(FileSystem.AppDataDirectory, FavoritesFileName);
                await File.WriteAllTextAsync(path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving favorites: {ex.Message}");
            }
        }

        // Group history by day for UI
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

        // Build genre stats for emoji chart
        private void BuildGenreStats()
        {
            GenreStats.Clear();

            // Flatten history: one entry per genre
            var genreGroups = History
                .SelectMany(h => h.Genre, (h, genre) => new { genre, emoji = h.Emoji }) // Emoji is string
                .GroupBy(x => x.genre)
                .OrderByDescending(g => g.Count());

            foreach (var group in genreGroups)
            {
                // Concatenate each movie's emoji safely as string
                string emojiBar = string.Join("", group.Select(x => x.emoji));

                GenreStats.Add(new GenreStat
                {
                    Genre = group.Key,
                    EmojiBar = emojiBar
                });
            }
        }

        // Update top 10 movies by rating
        public void UpdateTopMovies()
        {
            if (AllMovies == null || !AllMovies.Any())
            {
                TopMovies = new ObservableCollection<Movie>();
                return;
            }

            // Take top 10 by rating
            var top = AllMovies
                .OrderByDescending(m => m.rating) // assuming rating is double
                .Take(10)
                .ToList();

            TopMovies = new ObservableCollection<Movie>(top);
        }


    }
}
