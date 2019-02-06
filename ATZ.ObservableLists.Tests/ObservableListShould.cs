using System;
using FluentAssertions;
using NUnit.Framework;

namespace ATZ.ObservableLists.Tests
{
    [TestFixture]
    public class ObservableListShould
    {
        #region ICollection<T>
        [Test]
        public void NotContainItemNotAdded()
        {
            // ReSharper disable once CollectionNeverUpdated.Local => Checking default state.
            var ol = new ObservableList<int>();
            ol.Contains(13).Should().BeFalse();
        }

        [Test]
        public void ContainAddedItem()
        {
            var ol = new ObservableList<int> { 42 };
            ol.Contains(42).Should().BeTrue();
        }

        [Test]
        public void HaveZeroAsDefaultCount()
        {
            // ReSharper disable once CollectionNeverUpdated.Local => Checking default state.
            var ol = new ObservableList<int>();
            ol.Count.Should().Be(0);
        }

        [Test]
        public void AddingItemIncreasesCount()
        {
            var ol = new ObservableList<int> { 42 };
            ol.Count.Should().Be(1);
        }

        [Test]
        public void ClearItemsCorrectly()
        {
            var ol = new ObservableList<int> { 42 };

            ol.Clear();
            ol.Count.Should().Be(0);
        }

        [Test]
        public void RemoveItemCorrectly()
        {
            var ol = new ObservableList<int> { 42, 13 };

            ol.Remove(13);
            ol.Count.Should().Be(1);
            ol.Contains(42).Should().BeTrue();
            ol.Contains(13).Should().BeFalse();
        }

        [Test]
        public void CopyToArrayCorrectly()
        {
            var ol = new ObservableList<int> { 42, 13 };

            var items = new int[2];
            ol.CopyTo(items, 0);
            items.Should().Contain(new[] { 13, 42 }).And.HaveCount(2);
        }
        #endregion
        
        #region ICollection
        [Test]
        public void CopyToGenericArrayCorrectly()
        {
            var ol = new ObservableList<int> { 42, 13 };

            Array items = new int[2];
            ol.CopyTo(items, 0);
            items.Should().Contain(new[] { 13, 42 }).And.HaveCount(2);
        }
        #endregion

        #region IReadOnlyList<T>
        [Test]
        public void BeAbleToReturnItemByIndex()
        {
            var ol = new ObservableList<int> { 42 };

            ol[0].Should().Be(42);
        }
        #endregion
    }
}