using System;
using System.Collections.ObjectModel;

namespace MyProject
{
    public class HistoryGroup : ObservableCollection<MovieHistoryEntry>
    {
        public string Date { get; private set; }

        public HistoryGroup(string date, ObservableCollection<MovieHistoryEntry> entries) : base(entries)
        {
            Date = date;
        }
    }
}
