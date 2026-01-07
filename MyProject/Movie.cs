using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyProject
{
    public class Movie : INotifyPropertyChanged
    {
        public string title { get; set; }
        public int year { get; set; }
        public List<string> genre { get; set; }
        public string director { get; set; }
        public double rating { get; set; }
        public string emoji { get; set; }

        // Date when the movie was added to the list
        public DateTime DateAdded { get; set; } = DateTime.Now;

        // Read-only property to convert genre list into a comma-separated string
        public string genreString => genre != null ? string.Join(", ", genre) : "";

        // Backing field for IsFavorite
        private bool _isFavorite;

        // Property indicating whether the movie is marked as favorite
        // Notifies UI when changed, so bindings update automatically
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (_isFavorite != value)
                {
                    _isFavorite = value;
                    OnPropertyChanged();
                }
            }
        }

        // Event required by INotifyPropertyChanged
        // Raised whenever a property changes to update the UI
        public event PropertyChangedEventHandler? PropertyChanged;

        // Helper method to raise PropertyChanged event
        // CallerMemberName allows calling without specifying property name explicitly
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


    }
}
