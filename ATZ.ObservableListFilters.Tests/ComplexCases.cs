using System.Linq;
using NUnit.Framework;

namespace ATZ.ObservableListFilters.Tests
{
    [TestFixture]
    public class ComplexCases : ObservableListFilterTestBase
    {
        private void VerifyThatItemsSourceAndFilteredItemsOrderMatch(ObservableListFilter<TestClass> filter)
        {
            VerifyItems(
                filter.FilteredItems,
                filter.ItemsSource.Where(_ => _.Value % 2 == 0).Select(_ => _.Value).ToArray());
        }

        [Test]
        public void MovingItemToTheBeginningInTheFilteredItemsAndMakingAnItemPassShouldPreserveCorrectOrdering()
        {
            var filter = CreateFilterWithNewItems(new[] { 3, 4, 5, 6 });
            var itemToBeUpdated = filter.ItemsSource[0];
            
            filter.FilteredItems.Move(1, 0);
            
            itemToBeUpdated.Value = 2;
            filter.ItemsSource.ItemUpdate(itemToBeUpdated);

            VerifyThatItemsSourceAndFilteredItemsOrderMatch(filter);
        }

        [Test]
        public void MovingItemToTheEndInTheFilteredItemsAndMakingAnItemPassShouldPreserveCorrectOrdering()
        {
            var filter = CreateFilterWithItems(new[] { 4, 5, 6, 7 });
            var itemToBeUpdated = filter.ItemsSource[3];
            
            filter.FilteredItems.Move(0, 1);
            itemToBeUpdated.Value = 8;
            filter.ItemsSource.ItemUpdate(itemToBeUpdated);

            VerifyThatItemsSourceAndFilteredItemsOrderMatch(filter);
        }
    }
}