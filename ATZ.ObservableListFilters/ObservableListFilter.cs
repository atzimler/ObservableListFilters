using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using ATZ.ObservableLists;

namespace ATZ.ObservableListFilters
{
    /// <summary>
    /// Class to filter observable list items.
    /// </summary>
    /// <typeparam name="TItem">The type of the items in the ObservableList.</typeparam>
    public class ObservableListFilter<TItem>
        where TItem : class
    {
        private readonly Dictionary<NotifyCollectionChangedAction, Action<NotifyCollectionChangedEventArgs>> _filterCollectionChangeHandlers;
        private Func<TItem, bool> _filterFunction = _ => true;
        private readonly InternalChange _internalChange = new InternalChange();
        private ObservableList<TItem> _itemsSource;
        private readonly Dictionary<NotifyCollectionChangedAction, Action<NotifyCollectionChangedEventArgs>> _sourceCollectionChangeHandlers;

        /// <summary>
        /// Function to determine if the item should appear in the filtered result (true) or not (false).
        /// </summary>
        /// <remarks>Setting the function triggers to re-evaluate the filter for all items. If the value is null, no item passes the filter.</remarks>
        public Func<TItem, bool> FilterFunction
        {
            get => _filterFunction;
            set
            {
                _filterFunction = value;
                FilteredItems.Clear();
            }
        }
        
        /// <summary>
        /// The items from the ItemsSource collection that passed through the filter.
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <listheader><description>Changes applied to this list causes the following effects:</description></listheader>
        ///         <item>
        ///             <term>Add</term>
        ///             <description>The item is inserted at the translated position into the ItemsSource.
        ///                 Then, if the item is not passing the filter it is removed from the FilteredItems collection.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>Move</term>
        ///             <description>The item is moved accordingly to the translated position in the ItemsSource.</description>
        ///         </item>
        ///         <item>
        ///             <term>Remove</term>
        ///             <description>The item is also removed from the ItemsSource.</description>
        ///         </item>
        ///         <item>
        ///             <term>Replace</term>
        ///             <description>Replaces the item in the ItemsSource too.
        ///                 If the replacement item is not passing the filter function, it is then removed from the FilteredItems.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <term>Reset</term>
        ///             <describe>(by calling the Clear() method). Rebuilds the items from the ItemsSource into the FilteredItems.
        ///                 If you want to remove all items, call Clear() on the ItemsSource instead.</describe>
        ///         </item>
        ///     </list>
        /// </remarks>
        public ObservableList<TItem> FilteredItems { get; } = new ObservableList<TItem>();

        /// <summary>
        /// The items to filter from.
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <listheader><description>Changes applied to this list causes the following effects:</description></listheader>
        ///         <item>
        ///             <term>Changing the collection object</term>
        ///             <description>Re-evaluates the filter and updates the FilteredItems.</description>
        ///         </item>
        ///         <item>
        ///             <term>Add</term>
        ///             <description>If the new item passes the FilterFunction, it is also added at appropriate position to the FilteredItems</description>
        ///         </item>
        ///         <item>
        ///             <term>Move</term>
        ///             <description>If the item passes the FilterFunction, it is moved according to the move in the FilteredItems too.</description>
        ///         </item>
        ///         <item>
        ///             <term>Remove</term>
        ///             <description>The item is also removed from the FilteredItems if present.</description>
        ///         </item>
        ///         <item>
        ///             <term>Replace</term>
        ///             <description>After replacing the item in the ItemsSource, it is re-evaluated. If the item is passing the FilterFunction,
        ///                 the new item it is inserted or updated (based on if it was present before). If the item is not passing the FilterFunction,
        ///                 it will not be placed into the FilteredItems or if present it will be removed.</description>
        ///         </item>
        ///         <item>
        ///             <term>Reset</term>
        ///             <description>(by calling the Clear() method). Also clears the FilteredItems.</description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public ObservableList<TItem> ItemsSource
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
                    _itemsSource.ItemUpdated -= SourceItemUpdated;
                }

                _itemsSource = value;

                if (_itemsSource != null)
                {
                    _itemsSource.CollectionChanged += SourceCollectionChanged;
                    _itemsSource.ItemUpdated += SourceItemUpdated;
                }
                
                FilteredItems.Clear();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ObservableListFilter()
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
            FilteredItems.ItemUpdated += FilteredItemUpdated;
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

        private void FilteredItemUpdated(object sender, ItemUpdatedEventArgs e)
        {
            _internalChange.Execute(() =>
            {
                if (!ItemPassesFilter(FilteredItems[e.Index]))
                {
                    FilteredItems.RemoveAt(e.Index);
                }
            });
        }

        private void HandleAdditionToFilteredItems(NotifyCollectionChangedEventArgs e)
        {
            if (FilteredItems.OriginalRequest.Action == NotifyCollectionChangedAction.Reset)
            {
                return;
            }
            
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
            if (FilteredItems.OriginalRequest.Action == NotifyCollectionChangedAction.Replace)
            {
                return;
            }
            
            _itemsSource?.Remove(e.OldItems[0] as TItem);
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
        
        private void SourceItemUpdated(object sender, ItemUpdatedEventArgs e)
        {
            _internalChange.Execute(() =>
            {
                if (ItemPassesFilter(_itemsSource[e.Index]))
                {
                    AddItemToFilteredItemsFromItemsSourceAt(e.Index);
                }
                else
                {
                    RemoveItemFromFilteredItems(_itemsSource[e.Index]);
                }
            });
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

    }
}