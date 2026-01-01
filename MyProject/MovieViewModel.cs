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
        
        // New per-user filenames
        private string FavoritesFileName => $"{CurrentUser}_favorites.json";
        private string HistoryFileName => $"{CurrentUser}_history.json";

        private List<Movie> AllMovies = new();

        private List<MovieHistoryEntry> History = new();
        public ObservableCollection<Movie> FilteredMovies { get; set; } = new();
        public static MovieViewModel Shared { get; } = new MovieViewModel();
        public ObservableCollection<GenreStat> GenreStats { get; } = new();
        public ObservableCollection<MovieHistoryEntry> HistoryObservable { get; private set; } = new ObservableCollection<MovieHistoryEntry>();
        public ObservableCollection<HistoryGroup> GroupedHistory { get; private set; }= new ObservableCollection<HistoryGroup>();
        public string CurrentSortOption { get; set; }
        public bool SortAscending { get; set; }
        public string CurrentUser { get; set; }

        public bool HasMovies => FilteredMovies?.Any() == true;

        public bool HasFavorites =>
            FilteredMovies?.Any(m => m.IsFavorite) == true;

        public bool HasHistory =>
            HistoryObservable?.Any() == true;

        public Command RefreshMoviesCommand { get; }
        public string UserTitle => string.IsNullOrWhiteSpace(CurrentUser)
                           ? "Movies"
                           : $"{CurrentUser}'s Movies";

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

        public string UserEmoji
        {
            get => Preferences.Default.Get("UserEmoji", "🎬");
            set
            {
                Preferences.Default.Set("UserEmoji", value);
                OnPropertyChanged();
            }
        }

        // GreetingMessage here
        public string GreetingMessage =>
            $"Hello, {Preferences.Default.Get("UserName", CurrentUser)}!";


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
                string path = Path.Combine(FileSystem.AppDataDirectory, HistoryFileName);
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
                Genre = movie.genre,
                Emoji = movie.emoji,
                Timestamp = DateTime.Now,
                Action = action
            };

            History.Add(entry);                    // Keep for saving to file
            HistoryObservable.Add(entry);          // For UI updates

            BuildGroupedHistory();
            BuildGenreStats();

            await SaveHistoryAsync();
        }

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
            GenreStats.Clear();   // Clear emoji chart data
            await SaveHistoryAsync();
        }

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


    }
}
