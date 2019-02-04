using System;
using System.Collections.ObjectModel;

namespace ATZ.ObservableCollectionFilters
{
    public class ObservableCollectionFilter<TItem>
    {
        public Func<TItem, bool> FilterFunction { get; set; } = _ => true;
        public ObservableCollection<TItem> ItemsSource { get; set; }
    }
}