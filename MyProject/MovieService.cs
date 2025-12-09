using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace MyProject
{
    public static class MovieService
    {
        private static string filePath = Path.Combine(FileSystem.AppDataDirectory, "movies.json");

        private static string url = "https://raw.githubusercontent.com/DonH-ITS/jsonfiles/main/moviesemoji.json";

        // Load movies (download if needed)
        public static async Task<List<Movie>> LoadMoviesAsync()
        {
            // If local file doesn't exist, download it
            if (!File.Exists(filePath))
            {
                var json = await new HttpClient().GetStringAsync(url);
                await File.WriteAllTextAsync(filePath, json);
            }

            // Read local JSON and convert to list of Movie objects
            var localJson = await File.ReadAllTextAsync(filePath);

            var movies = JsonSerializer.Deserialize<List<Movie>>(localJson);
            return movies ?? new List<Movie>();

        }
    }
}
