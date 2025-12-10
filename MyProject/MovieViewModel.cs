using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject
{
    internal class MovieViewModel
    {
        public ObservableCollection<Movie> Movies { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
