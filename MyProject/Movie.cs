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

        public string genreString => genre != null ? string.Join(", ", genre) : "";

        private bool _isFavorite;
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

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


    }
}
