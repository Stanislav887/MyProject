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
    }
}
