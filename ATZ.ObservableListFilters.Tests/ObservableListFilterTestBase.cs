using System;
using System.Collections.Generic;
using System.Linq;
using ATZ.ObservableLists;
using FluentAssertions;

namespace ATZ.ObservableListFilters.Tests
{
    public class ObservableListFilterTestBase
    {
        protected readonly TestClass[] Items =
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

        private ObservableListFilter<TestClass> CreateFilterWithItems(int[] initialValues, Func<int, TestClass> itemSelector)
        {
            var filter = new ObservableListFilter<TestClass>
            {
                FilterFunction = _ => _.Value % 2 == 0,
                ItemsSource = new ObservableList<TestClass>()
            };

            foreach (var initialValue in initialValues)
            {
                filter.ItemsSource.Add(itemSelector(initialValue));
            }

            return filter;
        }

        protected ObservableListFilter<TestClass> CreateFilterWithItems(int[] initialValues)
            => CreateFilterWithItems(initialValues, _ => Items[_]);

        protected ObservableListFilter<TestClass> CreateFilterWithNewItems(int[] initialValues)
            => CreateFilterWithItems(initialValues, _ => new TestClass { Value = _ });

        protected void VerifyItems(IEnumerable<TestClass> items, int[] correctValues)
        {
            items.Select(_ => _.Value).Should().ContainInOrder(correctValues).And.HaveCount(correctValues.Length);
        }
    }
}