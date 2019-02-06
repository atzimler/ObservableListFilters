using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;

namespace ATZ.ObservableLists
{
    public class ObservableList<T> 
        : IReadOnlyList<T>, IList, IList<T>
        //        : IList<T>, IList, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly List<T> _items = new List<T>();

        object IList.this[int index]
        {
            get => _items[index];
            set => SetAt(index, AssertArgumentIsOfTypeT(value));
        }

        public T this[int index]
        {
            get => _items[index];
            set => SetAt(index, value);
        }

        public int Count => _items.Count;
        public bool IsFixedSize => ((IList)_items).IsFixedSize;
        public bool IsReadOnly => ((ICollection<T>)_items).IsReadOnly;
        public bool IsSynchronized => ((ICollection)_items).IsSynchronized;
        public object SyncRoot => ((ICollection)_items).SyncRoot;

        private T AssertArgumentIsOfTypeT(object item)
        {
            try
            {
                return (T)item;
            }
            catch (InvalidCastException)
            {
                // ReSharper disable once NotResolvedInText => Behaving exactly as the .NET Framework.
                throw new ArgumentException(
                    $@"The value ""{Convert.ToString(item, CultureInfo.InvariantCulture)}"" is not of type ""{typeof(T)}"" and cannot be used in this generic collection.", 
                    "value");
            }
        }

        private void SetAt(int index, T value)
        {
            _items[index] = value;
        }
        
        int IList.Add(object item)
        {
            Add(AssertArgumentIsOfTypeT(item));

            return Count - 1;
        }
        
        public void Add(T item)
        {
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();            
        }

        bool IList.Contains(object item) => ((IList)_items).Contains(item);
        public bool Contains(T item) => _items.Contains(item);

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_items).CopyTo(array, index);
        }
        
        public void CopyTo(T[] array, int index)
        {
            _items.CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        int IList.IndexOf(object item) => ((IList)_items).IndexOf(item);
        public int IndexOf(T item) => _items.IndexOf(item);

        void IList.Insert(int index, object item) => Insert(index, AssertArgumentIsOfTypeT(item));

        public void Insert(int index, T item)
        {
            _items.Insert(index, item);
        }

        void IList.Remove(object item)
        {
            if (item is T x)
            {
                Remove(x);
            }
        }
        
        public bool Remove(T item)
        {
            return _items.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }
        
//        public event NotifyCollectionChangedEventHandler CollectionChanged;
//        public event PropertyChangedEventHandler PropertyChanged;
    }
}