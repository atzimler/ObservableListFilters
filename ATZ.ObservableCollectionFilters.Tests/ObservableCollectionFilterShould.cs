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

        private void VerifyFilteredItems(ObservableCollectionFilter<TestClass> filter, int[] correctValues)
        {
            filter.FilteredItems.Select(_ => _.Value).Should().BeEquivalentTo(correctValues).And.HaveCount(correctValues.Length);
        }

        private void VerifyItemAddition(
            int[] initialSourceValues, int[] initialFilteredValues, 
            int insertPosition, int insertValue)
        {
            var filter = CreateFilterWithItems(initialSourceValues);
            VerifyFilteredItems(filter, initialFilteredValues);
        
            filter.ItemsSource.Insert(insertPosition, _items[insertValue]);
            VerifyFilteredItems(filter, new[] { 2, 4, 6 });
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
        public void VerifyItemAdditions()
        {
            VerifyItemAddition(new[] { 4, 6 }, new[] { 4, 6 }, 0, 2);        // (2), 4, 6
            VerifyItemAddition(new[] { 1, 4, 6 }, new[] { 4, 6 }, 1, 2);     // 1, (2), 4, 6
            VerifyItemAddition(new[] { 3, 4, 6 }, new[] { 4, 6 }, 0, 2);     // (2), 3, 4, 6
            
            VerifyItemAddition(new[] { 2, 6 }, new[] { 2, 6 }, 1, 4);        // 2, (4), 6 
            VerifyItemAddition(new[] { 2, 3, 6 }, new[] { 2, 6 }, 2, 4);     // 2, 3, (4), 6
            VerifyItemAddition(new[] { 2, 5, 6 }, new[] { 2, 6 }, 1, 4);     // 2, (4), 5, 6
            
            VerifyItemAddition(new[] { 2, 4 }, new[] { 2, 4 }, 2, 6);        // 2, 4, (6)
            VerifyItemAddition(new[] { 2, 4, 5 }, new[] { 2, 4 }, 3, 6);     // 2, 4, 5,(6)
            VerifyItemAddition(new[] { 2, 4, 7 }, new[] { 2, 4 }, 2, 6);     // 2, 4, (6), 7
        }
    }
}