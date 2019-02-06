using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        
        #region IList
        [Test]
        public void ThrowProperExceptionWhenAddingObjectWithIncorrectType()
        {
            foreach (var obj in new object[] { "x", 13.42 })
            {
                var comparisonList = new List<int>();
                var correctException = Assert.Throws<ArgumentException>(() => ((IList)comparisonList).Add(obj));
            
                var ol = new ObservableList<int>();
                var ex = Assert.Throws<ArgumentException>(() => ((IList)ol).Add(obj));
                ex.Message.Should().Be(correctException.Message);
                ex.ParamName.Should().Be(correctException.ParamName);
            }
        }

        [Test]
        public void InsertItemCorrectly()
        {
            var ol = new ObservableList<int>();
            ((IList)ol).Insert(0, 42);

            ol.Count.Should().Be(1);
            ol[0].Should().Be(42);
        }

        [Test]
        public void IgnoreRemovalRequestOfIncorrectType()
        {
            var ol = new ObservableList<int?> { null };
            ((IList)ol).Remove(13.42);

            ol.Count.Should().Be(1);
            ol[0].Should().BeNull();
        }

        [Test]
        public void RemoveItemWithCorrectTypePresentInTheList()
        {
            var ol = new ObservableList<int> { 13, 42 };
            ((IList)ol).Remove(13);

            ol.Count.Should().Be(1);
            ol[0].Should().Be(42);
        }

        [Test]
        public void RemoveItemAtSpecifiedIndex()
        {
            var ol = new ObservableList<int> { 8, 13, 42 };
            ol.RemoveAt(1);

            ol.Should().ContainInOrder(new[] { 8, 42 }).And.HaveCount(2);
        }
        #endregion
        
        #region INotifyCollectionChanged
        [Test]
        public void NotifyWhenAddingItemToTheList()
        {
            var ol = new ObservableList<int> { 12, 43 };

            var monitor = ol.Monitor();
            ol.CollectionChanged += (o, e) =>
            {
                e.Action.Should().Be(NotifyCollectionChangedAction.Add);
                e.NewStartingIndex.Should().Be(1);
                e.NewItems[0].Should().Be(42);
            };
            
            ol.Insert(1, 42);
            monitor.Should().Raise(nameof(ol.CollectionChanged));
        }

        [Test]
        public void NotifyWhenRemovingItemFromTheList()
        {
            var ol = new ObservableList<int> { 12, 13, 42 };

            var monitor = ol.Monitor();
            ol.CollectionChanged += (o, e) =>
            {
                e.Action.Should().Be(NotifyCollectionChangedAction.Remove);
                e.OldStartingIndex.Should().Be(1);
                e.OldItems[0].Should().Be(13);
            };
            
            ol.RemoveAt(1);
            monitor.Should().Raise(nameof(ol.CollectionChanged));
        }

        [Test]
        public void NotifyWhenRemovingItemFromTheListByReference()
        {
            var ol = new ObservableList<int> { 12, 13, 42 };

            var monitor = ol.Monitor();
            ol.CollectionChanged += (o, e) =>
            {
                e.Action.Should().Be(NotifyCollectionChangedAction.Remove);
                e.OldStartingIndex.Should().Be(1);
                e.OldItems[0].Should().Be(13);
            };

            ol.Remove(13);
            monitor.Should().Raise(nameof(ol.CollectionChanged));
        }

        [Test]
        public void NotifyWhenReplacingItemInTheList()
        {
            var ol = new ObservableList<int> { 8, 13, 42 };

            var monitor = ol.Monitor();
            ol.CollectionChanged += (o, e) =>
            {
                e.Action.Should().Be(NotifyCollectionChangedAction.Replace);
                e.NewStartingIndex.Should().Be(1);
                e.OldStartingIndex.Should().Be(1);
                e.NewItems[0].Should().Be(12);
                e.OldItems[0].Should().Be(13);
            };

            ol[1] = 12;
            monitor.Should().Raise(nameof(ol.CollectionChanged));
        }

        [Test]
        public void NotifyWhenClearingTheList()
        {
            var ol = new ObservableList<int> { 4, 13 };
            ol.CollectionChanged += (o, e) => { e.Action.Should().Be(NotifyCollectionChangedAction.Reset); };

            var monitor = ol.Monitor();
            ol.Clear();

            monitor.Should().Raise(nameof(ol.CollectionChanged));
        }

        [Test]
        public void NotifyWhenMovingItemInTheList()
        {
            var ol = new ObservableList<int> { 42, 12 };
            ol.CollectionChanged += (o, e) =>
            {
                e.Action.Should().Be(NotifyCollectionChangedAction.Move);
                e.OldItems[0].Should().Be(42);
                e.NewItems[0].Should().Be(42);
                e.OldStartingIndex.Should().Be(0);
                e.NewStartingIndex.Should().Be(1);
            };

            var monitor = ol.Monitor();
            ol.Move(0, 1);

            monitor.Should().Raise(nameof(ol.CollectionChanged));
        }

        [Test]
        public void MoveItemLowerCorrectly()
        {
            var ol = new ObservableList<int> { 2, 3, 1 };
            ol.Move(2, 0);

            ol.Should().ContainInOrder(1, 2, 3).And.HaveCount(3);
        }

        [Test]
        public void MoveItemHigherCorrectly()
        {
            var ol = new ObservableList<int> { 3, 1, 2 };
            ol.Move(0, 2);

            ol.Should().ContainInOrder(1, 2, 3).And.HaveCount(3);
        }

        [Test]
        public void NotRaiseCollectionChangedWhenItemIsMovedToItsCurrentPosition()
        {
            var ol = new ObservableList<int> { 1, 2, 3 };

            var monitor = ol.Monitor();
            ol.Move(1, 1);
            ol.Should().ContainInOrder(1, 2, 3).And.HaveCount(3);
            
            monitor.Should().NotRaise(nameof(ol.CollectionChanged));
        }

        [Test]
        public void NotReplaceItemIfItHasBeenChangedWhileNotifyCollectionChangedWasProcessed()
        {
            var ol = new ObservableList<int> { 1, 2, 3 };
            var changeHandlerExecuted = false;

            ol.CollectionChanged += (o, e) =>
            {
                if (changeHandlerExecuted)
                {
                    return;
                }
                
                ol.Move(3, 0);
                ol[0] = 4;
                changeHandlerExecuted = true;
            };
            
            ol.Add(0);

            ol.Should().ContainInOrder(0, 1, 2, 3).And.HaveCount(4);
        }
        #endregion
    }
}