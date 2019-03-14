using System;
using ATZ.ObservableLists;
using FluentAssertions;
using NUnit.Framework;

namespace ATZ.ObservableListFilters.Tests
{
    [TestFixture]
    public class ObservableListFilterShould : ObservableListFilterTestBase
    {
        private void VerifyFilteredItemAddition(
            int[] initialValues, 
            int position, int item, 
            int[] correctItemsSource, int[] correctFilteredItems)
        {
            var filter = CreateFilterWithItems(initialValues);
            
            filter.FilteredItems.Insert(position, Items[item]);
            VerifyItems(filter.ItemsSource, correctItemsSource);
            VerifyItems(filter.FilteredItems, correctFilteredItems);
        }

        private void VerifyFilteredItemUpdate(int newValue, int[] correctItemsSource, int[] correctFilteredItems)
        {
            var filter = new ObservableListFilter<TestClass>
            {
                FilterFunction = _ => _.Value % 2 == 0,
                ItemsSource = new ObservableList<TestClass>()
            };

            filter.ItemsSource.Add(new TestClass { Value = 2 });
            filter.ItemsSource[0].Value = newValue;
            filter.FilteredItems.ItemUpdateAt(0);
            
            VerifyItems(filter.FilteredItems, correctFilteredItems);
            VerifyItems(filter.ItemsSource, correctItemsSource);
        }

        private void VerifySourceItemAddition(
            int[] initialSourceValues, int[] initialFilteredValues, 
            int insertPosition, int insertValue)
        {
            var filter = CreateFilterWithItems(initialSourceValues);
            VerifyItems(filter.FilteredItems, initialFilteredValues);
        
            filter.ItemsSource.Insert(insertPosition, Items[insertValue]);
            VerifyItems(filter.FilteredItems, new[] { 2, 4, 6 });
        }

        private void VerifyItemsSourceItemUpdate(int initialValue, int newValue, int[] correctItemsSource, int[] correctFilteredItems)
        {
            var filter = new ObservableListFilter<TestClass>
            {
                FilterFunction = _ => _.Value % 2 == 0,
                ItemsSource = new ObservableList<TestClass>()
            };
            
            filter.ItemsSource.Add(new TestClass { Value = initialValue });
            filter.ItemsSource[0].Value = newValue;
            filter.ItemsSource.ItemUpdateAt(0);
            
            VerifyItems(filter.FilteredItems, correctFilteredItems);
            VerifyItems(filter.ItemsSource, correctItemsSource);
        }

        [Test]
        public void BeConstructable()
        {
            var unused = new ObservableListFilter<object>();
        }

        [Test]
        public void HaveDefaultFilterAsNotNull()
        {
            var filter = new ObservableListFilter<object>();
            filter.FilterFunction.Should().NotBeNull();
        }

        [Test]
        public void RetainFilterFunction()
        {
            // ReSharper disable once ConvertToLocalFunction => Does not help in this case.
            Func<object, bool> ff = x => true;
            
            var filter = new ObservableListFilter<object>();
            filter.FilterFunction.Should().NotBe(ff);

            filter.FilterFunction = ff;
            filter.FilterFunction.Should().Be(ff);
        }

        [Test]
        public void RetainReferenceOfItemsSource()
        {
            var itemsSource = new ObservableList<object>();
            
            var filter = new ObservableListFilter<object>();
            filter.ItemsSource.Should().BeNull();

            filter.ItemsSource = itemsSource;
            Assert.AreSame(filter.ItemsSource, itemsSource);
        }

        [Test]
        public void DefaultFilteredSourceToEmpty()
        {
            var filter = new ObservableListFilter<object>();
            filter.FilteredItems.Should().BeEmpty();
        }

        [Test]
        public void AddingItemToItemsSourceShouldBeAddedToFilteredItemsWhenMatchingFilter()
        {
            var itemsSource = new ObservableList<TestClass>();

            var filter = new ObservableListFilter<TestClass>
            {
                FilterFunction = _ => _.Value % 2 == 0,
                ItemsSource = itemsSource
            };

            var item = new TestClass { Value = 2 };
            itemsSource.Add(item);
            filter.FilteredItems.Should().Contain(item).And.HaveCount(1);
        }

        [Test]
        public void AddingItemToItemsSourceShouldNotBeAddedToFilteredItemsWhenNotMatchingFilter()
        {
            var itemsSource = new ObservableList<TestClass>();

            var filter = new ObservableListFilter<TestClass>
            {
                FilterFunction = _ => _.Value % 2 == 0,
                ItemsSource = itemsSource
            };

            var item = new TestClass { Value = 1 };
            itemsSource.Add(item);
            filter.FilteredItems.Should().BeEmpty();
        }

        [Test]
        public void AddItemToCorrectLocationWhenAllItemsAreMatchingFilter()
        {
            var filter = new ObservableListFilter<TestClass>
            {
                ItemsSource = new ObservableList<TestClass>()
            };

            var item1 = new TestClass { Value = 1 };
            var item2 = new TestClass { Value = 2 };
            var item3 = new TestClass { Value = 3 };
            
            filter.ItemsSource.Add(item1);
            filter.ItemsSource.Add(item3);
            filter.ItemsSource.Insert(1, item2);

            filter.FilteredItems.Should().ContainInOrder(item1, item2, item3).And.HaveCount(3);
        }

        [Test]
        public void VerifySourceItemAdditions()
        {
            VerifySourceItemAddition(new[] { 4, 6 }, new[] { 4, 6 }, 0, 2);        // (2), 4, 6
            VerifySourceItemAddition(new[] { 1, 4, 6 }, new[] { 4, 6 }, 1, 2);     // 1, (2), 4, 6
            VerifySourceItemAddition(new[] { 3, 4, 6 }, new[] { 4, 6 }, 0, 2);     // (2), 3, 4, 6
            
            VerifySourceItemAddition(new[] { 2, 6 }, new[] { 2, 6 }, 1, 4);        // 2, (4), 6 
            VerifySourceItemAddition(new[] { 2, 3, 6 }, new[] { 2, 6 }, 2, 4);     // 2, 3, (4), 6
            VerifySourceItemAddition(new[] { 2, 5, 6 }, new[] { 2, 6 }, 1, 4);     // 2, (4), 5, 6
            
            VerifySourceItemAddition(new[] { 2, 4 }, new[] { 2, 4 }, 2, 6);        // 2, 4, (6)
            VerifySourceItemAddition(new[] { 2, 4, 5 }, new[] { 2, 4 }, 3, 6);     // 2, 4, 5,(6)
            VerifySourceItemAddition(new[] { 2, 4, 7 }, new[] { 2, 4 }, 2, 6);     // 2, 4, (6), 7
        }

        [Test]
        public void IgnoreSourceMoveEventIfItemIsNotInTheFilteredItems()
        {
            var filter = CreateFilterWithItems(new[] { 1, 3, 4, 6 });
            VerifyItems(filter.FilteredItems, new[] { 4, 6 });
            
            filter.ItemsSource.Move(1, 0);
            VerifyItems(filter.FilteredItems, new[] { 4, 6 });
        }

        [Test]
        public void MoveItemInFilteredItemsWhenMovedInItemsSource()
        {
            var filter = CreateFilterWithItems(new[] { 1, 2, 3, 4, 5, 6, 7 });
            VerifyItems(filter.FilteredItems, new[] { 2, 4, 6 });
            
            filter.ItemsSource.Move(5, 2);
            VerifyItems(filter.FilteredItems, new[] { 2, 6, 4 });
        }

        [Test]
        public void RemoveItemFromFilteredItemsWhenRemovedFromItemsSource()
        {
            var filter = CreateFilterWithItems(new[] { 1, 2, 3, 4 });
            VerifyItems(filter.FilteredItems, new[] { 2, 4 });
            
            filter.ItemsSource.RemoveAt(1);
            VerifyItems(filter.FilteredItems, new[] { 4 });
        }

        [Test]
        public void IgnoreItemRemovalIfNotPresentInFilteredItems()
        {
            var filter = CreateFilterWithItems(new[] { 1, 2, 3, 4 });
            VerifyItems(filter.FilteredItems, new[] { 2, 4 });
            
            filter.ItemsSource.RemoveAt(0);
            VerifyItems(filter.FilteredItems, new[] { 2, 4 });
        }

        [Test]
        public void ReplaceItemInFilteredListWhenNewItemPassesTheFilter()
        {
            var filter = CreateFilterWithItems(new[] { 2 });

            filter.ItemsSource[0] = Items[4];
            VerifyItems(filter.FilteredItems, new[] { 4 });
        }

        [Test]
        public void AddItemToFilteredListWhenReplacingAPreviousItemThatWasNotPassingTheFilter()
        {
            var filter = CreateFilterWithItems(new[] { 1 });

            filter.ItemsSource[0] = Items[4];
            VerifyItems(filter.FilteredItems, new[] { 4 });
        }

        [Test]
        public void RemoveItemFromFilteredListWhenReplacingAPreviouslyPassingItemWithANonPassingOne()
        {
            var filter = CreateFilterWithItems(new[] { 2 });

            filter.ItemsSource[0] = Items[1];
            VerifyItems(filter.FilteredItems, Array.Empty<int>());
        }

        [Test]
        public void IgnoreItemReplacementWhenBothOldAndNewItemAreNotPassingTheFilter()
        {
            var filter = CreateFilterWithItems(new[] { 3 });

            filter.ItemsSource[0] = Items[1];
            VerifyItems(filter.FilteredItems, Array.Empty<int>());
        }

        [Test]
        public void ClearFilteredItemsWhenClearingItemsSource()
        {
            var filter = CreateFilterWithItems(new[] { 4 });
            
            filter.ItemsSource.Clear();
            VerifyItems(filter.FilteredItems, Array.Empty<int>());
        }

        [Test]
        public void VerifyFilteredItemAdditions()
        {
            VerifyFilteredItemAddition(Array.Empty<int>(), 0, 1, Array.Empty<int>(), Array.Empty<int>());    // cancelling
            
            VerifyFilteredItemAddition(new[] { 4, 6 }, 0, 2, new[] { 2, 4, 6 }, new[] { 2, 4, 6 });          // (2), 4, 6
            VerifyFilteredItemAddition(new[] { 3, 4, 6 }, 0, 2, new[] { 2, 3, 4, 6 }, new[] { 2, 4, 6 });    // (2), 3!, 4, 6
            
            VerifyFilteredItemAddition(new[] { 2, 6 }, 1, 4, new [] { 2, 4, 6 }, new[] { 2, 4, 6 });         // 2, (4), 6
            VerifyFilteredItemAddition(new[] { 2, 5, 6 }, 1, 4, new[] { 2, 4, 5, 6 }, new[] { 2, 4, 6 });    // 2, (4), 5!, 6
            
            VerifyFilteredItemAddition(new[] { 2, 4 }, 2, 6, new[] { 2, 4, 6 }, new[] { 2, 4, 6 });          // 2, 4, (6)
            VerifyFilteredItemAddition(new[] { 2, 4, 7 }, 2, 6, new[] { 2, 4, 6, 7 }, new[] { 2, 4, 6 });    // 2, 4, (6), 7!
        }

        private void VerifyFilteredItemMove(
            int[] initialValues,
            int oldIndex, int newIndex,
            int[] correctItemsSource, int[] correctFilteredItems)
        {
            var filter = CreateFilterWithItems(initialValues);
            
            filter.FilteredItems.Move(oldIndex, newIndex);
            VerifyItems(filter.ItemsSource, correctItemsSource);
            VerifyItems(filter.FilteredItems, correctFilteredItems);
        }

        [Test]
        public void VerifyFilteredItemMoves()
        {
            VerifyFilteredItemMove(new[] { 4, 2, 6 }, 1, 0, new[] { 2, 4, 6 }, new[] { 2, 4, 6 });
            VerifyFilteredItemMove(new[] {3, 4, 2, 6 }, 1, 0, new[] { 2, 3, 4, 6 }, new[] { 2, 4, 6 });
            
            VerifyFilteredItemMove(new[] { 4, 2, 6 }, 0, 1, new[] { 2, 4, 6 }, new[] { 2, 4, 6 });
            VerifyFilteredItemMove(new[] { 4, 2, 3, 6 }, 0, 1, new[] { 2, 3, 4, 6 }, new[] { 2, 4, 6 });
            
            VerifyFilteredItemMove(new[] { 2, 6, 4 }, 1, 2, new[] { 2, 4, 6 }, new[] { 2, 4, 6 });
            VerifyFilteredItemMove(new[] { 2, 6, 4, 5 }, 1, 2, new[] { 2, 4, 5, 6 }, new[] { 2, 4, 6 });
            
            VerifyFilteredItemMove(new[] { 2, 6, 4 }, 2, 1, new[] { 2, 4, 6 }, new[] { 2, 4, 6 });
            VerifyFilteredItemMove(new[] { 2, 5, 6, 4 }, 2, 1, new[] { 2, 4, 5, 6 }, new[] { 2, 4, 6 });
        }

        [Test]
        public void VerifyNonMovementMoveInFilteredItems()
        {
            VerifyFilteredItemMove(new[] { 1, 2, 3, 4, 5, 6, 7 }, 1, 1, new[] { 1, 2, 3, 4, 5, 6, 7 }, new[] { 2, 4, 6 });
        }

        [Test]
        public void VerifyNonMovementMoveInItemsSource()
        {
            var filter = CreateFilterWithItems(new[] { 1, 2, 3, 4, 5, 6, 7 });
            VerifyItems(filter.FilteredItems, new[] { 2, 4, 6 });
            
            filter.ItemsSource.Move(3, 3);
            VerifyItems(filter.FilteredItems, new[] { 2, 4, 6 });
        }

        [Test]
        public void RemoveItemFromItemsSourceWhenRemovedFromFilteredItems()
        {
            var filter = CreateFilterWithItems(new[] { 2, 4, 6 });
            filter.FilteredItems.RemoveAt(1);
            
            VerifyItems(filter.ItemsSource, new[] { 2, 6 });
        }

        [Test]
        public void ReplacingItemInFilteredItemsResultsInReplacingItInItemsSourceToo()
        {
            var filter = CreateFilterWithItems(new[] { 2, 3, 4, 7, 8 });

            filter.FilteredItems[1] = Items[6];
            VerifyItems(filter.FilteredItems, new[] { 2, 6, 8 });
            VerifyItems(filter.ItemsSource, new[] { 2, 3, 6, 7, 8 });
        }

        [Test]
        public void ReplaceAndRemoveItemInItemsSourceWhenReplacingItemInFilteredItemsIsNotPassingTheFilter()
        {
            var filter = CreateFilterWithItems(new[] { 2, 4, 6 });

            filter.FilteredItems[1] = Items[3];
            VerifyItems(filter.FilteredItems, new[] { 2, 6 });
            VerifyItems(filter.ItemsSource, new[] { 2, 3, 6 });
        }

        [Test]
        public void RebuildFilteredItemsWhenCleared()
        {
            var filter = CreateFilterWithItems(new[] { 2, 4, 6 });
            
            filter.FilteredItems.Clear();
            VerifyItems(filter.FilteredItems, new[] { 2, 4, 6 });
            VerifyItems(filter.ItemsSource, new[] { 2, 4, 6 });
        }

        [Test]
        public void NotCrashWhenItemsSourceIsNullAndFilteredItemsIsCleared()
        {
            var filter = new ObservableListFilter<TestClass>();
            filter.FilterFunction.Should().NotBeNull();
            filter.ItemsSource.Should().BeNull();

            filter.FilteredItems.Clear();
            filter.FilteredItems.Should().BeEmpty();
        }

        [Test]
        public void NotCrashWhenFilterFunctionIsNullAndFilteredItemsIsCleared()
        {
            var filter = new ObservableListFilter<TestClass>
            {
                FilterFunction = null,
                ItemsSource = new ObservableList<TestClass>()
            };
            filter.ItemsSource.Add(Items[2]);
            
            filter.FilteredItems.Clear();
            filter.FilteredItems.Should().BeEmpty();
        }

        [Test]
        public void HandleItemsSourceItemUpdateCorrectly()
        {
            VerifyItemsSourceItemUpdate(1, 3, new[] { 3 }, Array.Empty<int>());
            VerifyItemsSourceItemUpdate(1, 2, new[] { 2 }, new[] { 2 });
            VerifyItemsSourceItemUpdate(2, 3, new[] { 3 }, Array.Empty<int>());
            VerifyItemsSourceItemUpdate(2, 4, new[] { 4 }, new[] { 4 });
        }

        [Test]
        public void HandleFilteredItemsUpdateCorrectly()
        {
            VerifyFilteredItemUpdate(1, new[] { 1 }, Array.Empty<int>());
            VerifyFilteredItemUpdate(2, new[] { 2 }, new[] { 2 });
        }

        [Test]
        public void RebuildFilteredItemsWhenChangingItemsSource()
        {
            var filter = CreateFilterWithItems(new[] { 4 });
            VerifyItems(filter.FilteredItems, new[] { 4 });
            
            var itemsSource = new ObservableList<TestClass>
            {
                new TestClass { Value = 1 },
                new TestClass { Value = 2 }
            };

            filter.ItemsSource = itemsSource;
            VerifyItems(filter.FilteredItems, new[] { 2 });
        }

        [Test]
        public void NotAddToTheFilterWhenItemsSourceIsNull()
        {
            var filter = new ObservableListFilter<TestClass>
            {
                FilterFunction = _ => _.Value % 2 == 0,
                ItemsSource = null
            };
            
            filter.FilteredItems.Add(Items[2]);
            VerifyItems(filter.FilteredItems, Array.Empty<int>());
        }

        [Test]
        public void RebuildFilteredItemsOnFilterFunctionChange()
        {
            var filter = new ObservableListFilter<TestClass>
            {
                FilterFunction = _ => _.Value % 2 == 0,
                ItemsSource = new ObservableList<TestClass>()
            };

            for (var i = 0; i < 10; ++i)
            {
                filter.ItemsSource.Add(new TestClass { Value = i});
            }
            
            VerifyItems(filter.FilteredItems, new[] { 0, 2, 4, 6, 8 });

            filter.FilterFunction = _ => _.Value % 2 == 1;
            VerifyItems(filter.FilteredItems, new[] { 1, 3, 5, 7, 9 });
        }

        [Test]
        public void BeAbleToFunctionWithMultipleObservers()
        {
            var filter = new ObservableListFilter<TestClass>
            {
                FilterFunction = _ => _.Value % 2 == 0,
                ItemsSource = new ObservableList<TestClass> { new TestClass { Value =  2 }}
            };

            filter.FilteredItems.CollectionChanged += delegate
            {
                // Observer
            };
            
            filter.FilteredItems.Clear();
        }

        [Test]
        public void NotCheckFilterFunctionForEquality()
        {
            /*
             * In certain conditions, the filter function can be the same object but still evaluate differently compared
             * to the previous run, so we should not make any decision based on the equality of the two functions to
             * reapply the filter to the items or not. Consider the following:
             *
             * public class Data
             * {
             *     public DateTime DateTime { get; set; }
             * }
             * 
             * private static readonly Data data = new Data();
             * private static Func<DateTime, bool> F4()
             * {
             *     return dt => data.DateTime == dt;
             * }
             *
             * [Test]
             * public void X4()
             * {
             *     Func<DateTime, bool> f1 = F3();
             *     Func<DateTime, bool> f2 = F3();
             *     Assert.IsTrue(f1 == f2);
             * }
             *
             * The assertion is passing (the two functions are equal), while between the two actual object creation
             * nothing stops us from updating the data.DateTime property to a different value. 
             * */

            // ReSharper disable once ConvertToLocalFunction => No, we want to test this with a Func object.
            Func<TestClass, bool> filterFunction = _ => true; 
            
            var filter = new ObservableListFilter<TestClass>
            {
                FilterFunction = filterFunction,
                ItemsSource = new ObservableList<TestClass>()
            };

            var monitor = filter.FilteredItems.Monitor();

            filter.FilterFunction = filterFunction;
            monitor.Should().Raise(nameof(ObservableList<TestClass>.CollectionChanged));
        }
    }
}