using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ATZ.ObservableCollectionFilters
{
    public class ObservableCollectionFilter<TItem>
        where TItem : class
    {
        private ObservableCollection<TItem> _itemsSource;
        private Dictionary<NotifyCollectionChangedAction, Action<NotifyCollectionChangedEventArgs>> _sourceCollectionChangeHandlers;
        
        public Func<TItem, bool> FilterFunction { get; set; } = _ => true;
        public ObservableCollection<TItem> FilteredItems { get; } = new ObservableCollection<TItem>();

        public ObservableCollection<TItem> ItemsSource
        {
            get => _itemsSource;
            set
            {
                if (_itemsSource != null)
                {
                    _itemsSource.CollectionChanged -= SourceCollectionChanged;
                }

                _itemsSource = value;

                if (_itemsSource != null)
                {
                    _itemsSource.CollectionChanged += SourceCollectionChanged;
                }
            }
        }

        public ObservableCollectionFilter()
        {
            _sourceCollectionChangeHandlers = new Dictionary<NotifyCollectionChangedAction,Action<NotifyCollectionChangedEventArgs>>
            {
                { NotifyCollectionChangedAction.Add, HandleAdditionToItemsSource },
                { NotifyCollectionChangedAction.Move, HandleMoveInItemsSource },
                { NotifyCollectionChangedAction.Remove, HandleRemovalFromItemsSource },
                { NotifyCollectionChangedAction.Replace, HandleReplacementInItemsSource }
            };
        }

        private void HandleAdditionToItemsSource(NotifyCollectionChangedEventArgs e)
        {
            var item = e.NewItems[0] as TItem;
            if (!ItemPassesFilter(item))
            {
                return;
            }
            
            FilteredItems.Insert(TranslateSourceIndex(e.NewStartingIndex), item);
        }

        private void HandleMoveInItemsSource(NotifyCollectionChangedEventArgs e)
        {
            var item = e.NewItems[0] as TItem;
            if (!ItemPassesFilter(item))
            {
                return;
            }
            
            var oldTargetIndex = FilteredItems.IndexOf(item);
            var newTargetIndex = TranslateSourceIndex(e.NewStartingIndex);
            
            FilteredItems.Move(oldTargetIndex, newTargetIndex);
        }

        private void HandleRemovalFromItemsSource(NotifyCollectionChangedEventArgs e)
        {
            var item = e.OldItems[0] as TItem;
            FilteredItems.Remove(item);
        }

        private void HandleReplacementInItemsSource(NotifyCollectionChangedEventArgs e)
        {
            var oldItem = e.OldItems[0] as TItem;
            var newItem = e.NewItems[0] as TItem;
            
            if (!ItemPassesFilter(newItem))
            {
                if (ItemPassesFilter(oldItem))
                {
                    FilteredItems.Remove(oldItem);
                }
                
                return;
            }
            
            var index = FilteredItems.IndexOf(oldItem);
            if (index == -1)
            {
                FilteredItems.Insert(TranslateSourceIndex(e.NewStartingIndex), newItem);
            }
            else
            {
                FilteredItems[index] = newItem;
            }
        }
        
        private bool ItemPassesFilter(TItem item) => FilterFunction != null && FilterFunction(item);
        
        private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_sourceCollectionChangeHandlers.ContainsKey(e.Action))
            {
                return;
            }

            _sourceCollectionChangeHandlers[e.Action](e);
        }

        private int TranslateSourceIndex(int sourceIndex)
        {
            var referenceIndex = sourceIndex - 1;
            while (referenceIndex > -1 && !FilterFunction(_itemsSource[referenceIndex]))
            {
                referenceIndex--;
            }

            if (referenceIndex == -1)
            {
                return 0;
            }

            var referenceItem = _itemsSource[referenceIndex];
            var targetReferenceIndex = FilteredItems.IndexOf(referenceItem);
            return targetReferenceIndex + 1;
        }
    }
}