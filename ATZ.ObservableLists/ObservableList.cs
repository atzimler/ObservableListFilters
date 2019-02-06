using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;

namespace ATZ.ObservableLists
{
    public class ObservableList<T> 
        : IReadOnlyList<T>, IList, IList<T>, INotifyCollectionChanged
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

        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate {  };

        private bool ApplyChange(NotifyCollectionChangedEventArgs e)
        {
            var c = EqualityComparer<T>.Default;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex <= _items.Count)
                    {
                        _items.Insert(e.NewStartingIndex, (T)e.NewItems[0]);
                        return true;
                    }
                    break;
                
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < _items.Count && c.Equals(_items[e.OldStartingIndex], (T)e.OldItems[0]))
                    {
                        _items.RemoveAt(e.OldStartingIndex);
                        return true;
                    }
                    break;
                
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < _items.Count && c.Equals(_items[e.OldStartingIndex], (T)e.OldItems[0]))
                    {
                        _items[e.OldStartingIndex] = (T)e.NewItems[0];
                        return true;
                    }
                    break;
            }

            return false;
        }
        
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

        private readonly Queue<NotifyCollectionChangedEventArgs> _changes = new Queue<NotifyCollectionChangedEventArgs>();
        private bool _processing;

        private void ProcessChange()
        {
            var change = _changes.Dequeue();
            if (ApplyChange(change))
            {
                OnCollectionChanged(change);
            };
        }
        
        private void ProcessChanges(NotifyCollectionChangedEventArgs e)
        {
            _changes.Enqueue(e);
            if (_processing)
            {
                return;
            }

            try
            {
                while (_changes.Count > 0)
                {
                    ProcessChange();
                }
            }
            finally
            {
                _processing = false;
            }
        }
        
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged(this, e);
        }

        private void SetAt(int index, T value) 
            => ProcessChanges(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, _items[index], index));
        
        int IList.Add(object item)
        {
            Add(AssertArgumentIsOfTypeT(item));

            return Count - 1;
        }

        public void Add(T item) => Insert(_items.Count, item);

        public void Clear()
        {
            _items.Clear();            
        }

        bool IList.Contains(object item) => ((IList)_items).Contains(item);
        public bool Contains(T item) => _items.Contains(item);

        public void CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);
        public void CopyTo(T[] array, int index) => _items.CopyTo(array, index);

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        int IList.IndexOf(object item) => ((IList)_items).IndexOf(item);
        public int IndexOf(T item) => _items.IndexOf(item);

        void IList.Insert(int index, object item) => Insert(index, AssertArgumentIsOfTypeT(item));
        public void Insert(int index, T item) => ProcessChanges(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new[] { item }, index));

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

        public void RemoveAt(int index) => ProcessChanges(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new[] { _items[index] }, index)); 
        
//        public event PropertyChangedEventHandler PropertyChanged;
    }
}