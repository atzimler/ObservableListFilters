using System;

namespace ATZ.ObservableCollectionFilters
{
    public class ObservableCollectionFilter<TItem>
    {
        public Func<TItem, bool> FilterFunction { get; set; } = _ => true;
    }
}