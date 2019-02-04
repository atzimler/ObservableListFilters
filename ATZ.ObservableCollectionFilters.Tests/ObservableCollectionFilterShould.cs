using System;
using System.Collections.ObjectModel;
using FluentAssertions;
using NUnit.Framework;

namespace ATZ.ObservableCollectionFilters.Tests
{
    [TestFixture]
    public class ObservableCollectionFilterShould
    {
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
    }
}