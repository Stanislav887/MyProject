using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject
{
    public class Movie
    {
        // The movie's title 
        public string Title { get; set; }

        // Release year of the movie 
        public int Year { get; set; }

        // Array of genres 
        public string[] Genres { get; set; }

        // Director's name 
        public string Director { get; set; }

        // IMDB rating as a number 
        public double IMDBRating { get; set; }

        // Emoji representing the primary genre 
        public string Emoji { get; set; }

        // Whether the user has marked this movie as a favourite
        public bool IsFavourite { get; set; }

        // When the movie was last viewed 
        public DateTime? ViewedAt { get; set; }
    }
}
