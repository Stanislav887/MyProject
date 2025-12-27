using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject
{
    public class MovieHistoryEntry
    {
        public string Title { get; set; }
        public int Year { get; set; }
        public List<string> Genre { get; set; } = new();
        public string Emoji { get; set; }

        // "Viewed", "Favorited", "Unfavorited"
        public string Action { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
