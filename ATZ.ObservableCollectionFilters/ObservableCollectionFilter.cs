using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ATZ.ObservableCollectionFilters
{
    public class ObservableCollectionFilter<TItem>
        where TItem : class
    {
        private readonly Dictionary<NotifyCollectionChangedAction, Action<NotifyCollectionChangedEventArgs>> _filterCollectionChangeHandlers;
        private Func<TItem, bool> _filterFunction = _ => true;
        private readonly InternalChange _internalChange = new InternalChange();
        private ObservableCollection<TItem> _itemsSource;
        private readonly Dictionary<NotifyCollectionChangedAction, Action<NotifyCollectionChangedEventArgs>> _sourceCollectionChangeHandlers;

        public Func<TItem, bool> FilterFunction
        {
            get => _filterFunction;
            set
            {
                if (_filterFunction == value)
                {
                    return;
                }
                
                _filterFunction = value;
                FilteredItems.Clear();
            }
        }
        
        public ObservableCollection<TItem> FilteredItems { get; } = new ObservableCollection<TItem>();

        public ObservableCollection<TItem> ItemsSource
        {
            get => _itemsSource;
            set
            {
                if (_itemsSource == value)
                {
                    return;
                }
                
                if (_itemsSource != null)
                {
                    _itemsSource.CollectionChanged -= SourceCollectionChanged;
                }

                _itemsSource = value;

                if (_itemsSource != null)
                {
                    _itemsSource.CollectionChanged += SourceCollectionChanged;
                }
                
                FilteredItems.Clear();
            }
        }

        public ObservableCollectionFilter()
        {
            _sourceCollectionChangeHandlers = new Dictionary<NotifyCollectionChangedAction,Action<NotifyCollectionChangedEventArgs>>
            {
                { NotifyCollectionChangedAction.Add, HandleAdditionToItemsSource },
                { NotifyCollectionChangedAction.Move, HandleMoveInItemsSource },
                { NotifyCollectionChangedAction.Remove, HandleRemovalFromItemsSource },
                { NotifyCollectionChangedAction.Replace, HandleReplacementInItemsSource },
                { NotifyCollectionChangedAction.Reset, _ => HandleResetOnItemsSource() }
            };
            
            _filterCollectionChangeHandlers = new Dictionary<NotifyCollectionChangedAction, Action<NotifyCollectionChangedEventArgs>>
            {
                { NotifyCollectionChangedAction.Add, HandleAdditionToFilteredItems },
                { NotifyCollectionChangedAction.Move, HandleMoveInFilteredItems },
                { NotifyCollectionChangedAction.Remove, HandleRemovalFromFilteredItems },
                { NotifyCollectionChangedAction.Replace, HandleReplacementInFilteredItems },
                { NotifyCollectionChangedAction.Reset, _ => HandleResetOnFilteredItems() }
            };

            FilteredItems.CollectionChanged += FilteredCollectionChanged;
        }

        private void AddItemToFilteredItemsFromItemsSourceAt(int index)
        {
            var item = _itemsSource[index];
            if (FilteredItems.IndexOf(item) == -1)
            {
                FilteredItems.Insert(TranslateSourceIndex(index), item);
            }
        }

        private void BuildFilteredItems()
        {
            foreach (var item in _itemsSource)
            {
                if (_filterFunction(item))
                {
                    FilteredItems.Add(item);
                }
            }
        }
        
        private void FilteredCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_filterCollectionChangeHandlers.ContainsKey(e.Action))
            {
                return;
            }

            _internalChange.Execute(() => _filterCollectionChangeHandlers[e.Action](e));
        }

        private void HandleAdditionToFilteredItems(NotifyCollectionChangedEventArgs e)
        {
            var item = e.NewItems[0] as TItem;
            if (!ItemPassesFilter(item) || _itemsSource == null)
            {
                FilteredItems.RemoveAt(e.NewStartingIndex);
                return;
            }

            _itemsSource.Insert(TranslateTargetIndex(e.NewStartingIndex, -1), item);
        }

        private void HandleAdditionToItemsSource(NotifyCollectionChangedEventArgs e)
        {
            var item = e.NewItems[0] as TItem;
            if (!ItemPassesFilter(item))
            {
                return;
            }
            
            AddItemToFilteredItemsFromItemsSourceAt(e.NewStartingIndex);
        }

        private void HandleMoveInFilteredItems(NotifyCollectionChangedEventArgs e)
        {
            var item = e.NewItems[0] as TItem;
            var oldSourceIndex = ItemsSource.IndexOf(item);

            var referenceDirection = Math.Sign(e.NewStartingIndex - e.OldStartingIndex);
            ItemsSource.Move(oldSourceIndex, TranslateTargetIndex(e.NewStartingIndex, referenceDirection));
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

        private void HandleRemovalFromFilteredItems(NotifyCollectionChangedEventArgs e)
        {
            ItemsSource.Remove(e.OldItems[0] as TItem);
        }
        
        private void HandleRemovalFromItemsSource(NotifyCollectionChangedEventArgs e)
        {
            RemoveItemFromFilteredItems(e.OldItems[0] as TItem);
        }

        private void HandleReplacementInFilteredItems(NotifyCollectionChangedEventArgs e)
        {
            var newItem = e.NewItems[0] as TItem;
            
            var sourceIndex = _itemsSource.IndexOf(e.OldItems[0] as TItem);
            _itemsSource[sourceIndex] = newItem;

            if (!ItemPassesFilter(newItem))
            {
                FilteredItems.RemoveAt(e.NewStartingIndex);
            }
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

        private void HandleResetOnFilteredItems()
        {
            if (_itemsSource == null || _filterFunction == null)
            {
                return;
            }
            
            BuildFilteredItems();
        }
        
        private void HandleResetOnItemsSource()
        {
            FilteredItems.Clear();
        }
        
        private bool ItemPassesFilter(TItem item) => _filterFunction != null && _filterFunction(item);

        private void RemoveItemFromFilteredItems(TItem item)
        {
            FilteredItems.Remove(item);
        }
        
        private void SourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_sourceCollectionChangeHandlers.ContainsKey(e.Action))
            {
                return;
            }

            _internalChange.Execute(() => _sourceCollectionChangeHandlers[e.Action](e));
        }

        private int TranslateSourceIndex(int sourceIndex)
        {
            var referenceIndex = sourceIndex - 1;
            while (referenceIndex > -1 && !_filterFunction(_itemsSource[referenceIndex]))
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

        private int TranslateTargetIndex(int targetIndex, int referencePosition)
        {
            if (targetIndex == 0)
            {
                return 0;
            }

            if (targetIndex + referencePosition >= FilteredItems.Count)
            {
                return _itemsSource.Count - 1;
            }

            var referenceItem = FilteredItems[targetIndex + referencePosition];
            var referenceIndex = _itemsSource.IndexOf(referenceItem);
            return referenceIndex - referencePosition;
        }

        public void FilteredItemUpdated(int index)
        {
            _internalChange.Execute(() =>
            {
                if (!ItemPassesFilter(FilteredItems[index]))
                {
                    FilteredItems.RemoveAt(index);
                }
            });
        }

        public void SourceItemUpdated(int index)
        {
            _internalChange.Execute(() =>
            {
                if (ItemPassesFilter(_itemsSource[index]))
                {
                    AddItemToFilteredItemsFromItemsSourceAt(index);
                }
                else
                {
                    RemoveItemFromFilteredItems(_itemsSource[index]);
                }
            });
        }
    }
}