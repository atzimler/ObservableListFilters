using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ATZ.ObservableLists
{
    public class ObservableList<T> 
        : ICollection<T>, ICollection, IReadOnlyList<T>
        //        : IList<T>, IList, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private readonly List<T> _items = new List<T>();


        public T this[int index]
        {
            get => _items[index];
//            set => _items[index] = value;
        }

        public bool IsReadOnly => ((ICollection<T>)_items).IsReadOnly;
        public bool IsSynchronized => ((ICollection)_items).IsSynchronized;
        public object SyncRoot => ((ICollection)_items).SyncRoot;
        
        public void Add(T item)
        {
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();            
        }
        
        public bool Contains(T item) => _items.Contains(item);

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_items).CopyTo(array, index);
        }
        
        public void CopyTo(T[] array, int index)
        {
            _items.CopyTo(array, index);
        }

        public int Count => _items.Count;

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        public bool Remove(T item)
        {
            return _items.Remove(item);
        }
        
//        private int _count;
//        private bool _isReadOnly;
//        private int _count1;
//        private bool _isReadOnly1;
//
//        public int IndexOf(object value)
//        {
//            throw new NotImplementedException();
//        }
//
//        public void Insert(int index, object value)
//        {
//            throw new NotImplementedException();
//        }
//
//        void IList.RemoveAt(int index)
//        {
//            throw new NotImplementedException();
//        }
//
//        public bool IsFixedSize { get; }
//
//        bool IList.IsReadOnly
//        {
//            get { return _isReadOnly1; }
//        }
//
//        object IList.this[int index] { get; set; }
//        public void CopyTo(Array array, int index)
//        {
//            throw new NotImplementedException();
//        }
//
//        public bool IsSynchronized { get; }
//        public object SyncRoot { get; }
//
//        int ICollection<T>.Count
//        {
//            get { return _count; }
//        }
//
//        bool ICollection<T>.IsReadOnly
//        {
//            get { return _isReadOnly; }
//        }
//
//        public int IndexOf(T item)
//        {
//            throw new NotImplementedException();
//        }
//
//        public void Insert(int index, T item)
//        {
//            throw new NotImplementedException();
//        }
//
//        void IList<T>.RemoveAt(int index)
//        {
//            throw new NotImplementedException();
//        }
//
//
//        public event NotifyCollectionChangedEventHandler CollectionChanged;
//        public event PropertyChangedEventHandler PropertyChanged;
    }
}