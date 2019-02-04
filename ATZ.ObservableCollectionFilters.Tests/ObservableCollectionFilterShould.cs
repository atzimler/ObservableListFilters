using System;
using System.Collections.ObjectModel;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace ATZ.ObservableCollectionFilters.Tests
{
    [TestFixture]
    public class ObservableCollectionFilterShould
    {
        private readonly TestClass[] _items =
        {
            new TestClass { Value = 0 },
            new TestClass { Value = 1 },
            new TestClass { Value = 2 },
            new TestClass { Value = 3 },
            new TestClass { Value = 4 },
            new TestClass { Value = 5 },
            new TestClass { Value = 6 },
            new TestClass { Value = 7 },
            new TestClass { Value = 8 },
            new TestClass { Value = 9 }
        };
        
        private ObservableCollectionFilter<TestClass> CreateFilterWithItems(int[] initialValues)
        {
            var filter = new ObservableCollectionFilter<TestClass>
            {
                FilterFunction = _ => _.Value % 2 == 0,
                ItemsSource = new ObservableCollection<TestClass>()
            };

            foreach (var initialValue in initialValues)
            {
                filter.ItemsSource.Add(_items[initialValue]);
            }

            return filter;
        }

        private void VerifyItems(ObservableCollection<TestClass> items, int[] correctValues)
        {
            items.Select(_ => _.Value).Should().ContainInOrder(correctValues).And.HaveCount(correctValues.Length);
        }

        private void VerifySourceItemAddition(
            int[] initialSourceValues, int[] initialFilteredValues, 
            int insertPosition, int insertValue)
        {
            var filter = CreateFilterWithItems(initialSourceValues);
            VerifyItems(filter.FilteredItems, initialFilteredValues);
        
            filter.ItemsSource.Insert(insertPosition, _items[insertValue]);
            VerifyItems(filter.FilteredItems, new[] { 2, 4, 6 });
        }

        [Test]
        public void BeConstructable()
        {
            var unused = new ObservableCollectionFilter<object>();
        }

        [Test]
        public void HaveDefaultFilterAsNotNull()
        {
            var filter = new ObservableCollectionFilter<object>();
            filter.FilterFunction.Should().NotBeNull();
        }

        [Test]
        public void RetainFilterFunction()
        {
            // ReSharper disable once ConvertToLocalFunction => Does not help in this case.
            Func<object, bool> ff = x => true;
            
            var filter = new ObservableCollectionFilter<object>();
            filter.FilterFunction.Should().NotBe(ff);

            filter.FilterFunction = ff;
            filter.FilterFunction.Should().Be(ff);
        }

        [Test]
        public void RetainReferenceOfItemsSource()
        {
            var itemsSource = new ObservableCollection<object>();
            
            var filter = new ObservableCollectionFilter<object>();
            filter.ItemsSource.Should().BeNull();

            filter.ItemsSource = itemsSource;
            Assert.AreSame(filter.ItemsSource, itemsSource);
        }

        [Test]
        public void DefaultFilteredSourceToEmpty()
        {
            var filter = new ObservableCollectionFilter<object>();
            filter.FilteredItems.Should().BeEmpty();
        }

        [Test]
        public void AddingItemToItemsSourceShouldBeAddedToFilteredItemsWhenMatchingFilter()
        {
            var itemsSource = new ObservableCollection<TestClass>();

            var filter = new ObservableCollectionFilter<TestClass>
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
            var itemsSource = new ObservableCollection<TestClass>();

            var filter = new ObservableCollectionFilter<TestClass>
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
            var filter = new ObservableCollectionFilter<TestClass>
            {
                ItemsSource = new ObservableCollection<TestClass>()
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

            filter.ItemsSource[0] = _items[4];
            VerifyItems(filter.FilteredItems, new[] { 4 });
        }

        [Test]
        public void AddItemToFilteredListWhenReplacingAPreviousItemThatWasNotPassingTheFilter()
        {
            var filter = CreateFilterWithItems(new[] { 1 });

            filter.ItemsSource[0] = _items[4];
            VerifyItems(filter.FilteredItems, new[] { 4 });
        }

        [Test]
        public void RemoveItemFromFilteredListWhenReplacingAPreviouslyPassingItemWithANonPassingOne()
        {
            var filter = CreateFilterWithItems(new[] { 2 });

            filter.ItemsSource[0] = _items[1];
            VerifyItems(filter.FilteredItems, Array.Empty<int>());
        }

        [Test]
        public void IgnoreItemReplacementWhenBothOldAndNewItemAreNotPassingTheFilter()
        {
            var filter = CreateFilterWithItems(new[] { 3 });

            filter.ItemsSource[0] = _items[1];
            VerifyItems(filter.FilteredItems, Array.Empty<int>());
        }

        [Test]
        public void ClearFilteredItemsWhenClearingItemsSource()
        {
            var filter = CreateFilterWithItems(new[] { 4 });
            
            filter.ItemsSource.Clear();
            VerifyItems(filter.FilteredItems, Array.Empty<int>());
        }

        private void VerifyFilteredItemAddition(
            int[] initialValues, 
            int position, int item, 
            int[] correctItemsSource, int[] correctFilteredItems)
        {
            var filter = CreateFilterWithItems(initialValues);
            
            filter.FilteredItems.Insert(position, _items[item]);
            VerifyItems(filter.ItemsSource, correctItemsSource);
            VerifyItems(filter.FilteredItems, correctFilteredItems);
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
        
        // TODO: Test moves in both FilteredItems and ItemsSource where old and new position are equal.
    }
}