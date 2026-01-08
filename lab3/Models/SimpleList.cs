using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab3.Collections
{
    public class SimpleList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private T[] _items;
        private int _count;
        private const int DefaultCapacity = 4;

        public SimpleList()
        {
            _items = new T[DefaultCapacity];
            _count = 0;
        }

        public SimpleList(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            
            _items = new T[capacity];
            _count = 0;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new IndexOutOfRangeException();
                return _items[index];
            }
            set
            {
                if (index < 0 || index >= _count)
                    throw new IndexOutOfRangeException();
                _items[index] = value;
            }
        }

        public int Count => _count;
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            if (_count == _items.Length)
                EnsureCapacity(_count + 1);
            _items[_count++] = item;
        }

        public void Clear()
        {
            Array.Clear(_items, 0, _count);
            _count = 0;
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < _count; i++)
                if (EqualityComparer<T>.Default.Equals(_items[i], item))
                    return true;
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < _count)
                throw new ArgumentException("Недостаточно места в целевом массиве");

            Array.Copy(_items, 0, array, arrayIndex, _count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
                yield return _items[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(T item)
        {
            for (int i = 0; i < _count; i++)
                if (EqualityComparer<T>.Default.Equals(_items[i], item))
                    return i;
            return -1;
        }

        public void Insert(int index, T item)
        {
            if (index < 0 || index > _count)
                throw new IndexOutOfRangeException();

            if (_count == _items.Length)
                EnsureCapacity(_count + 1);
            
            if (index < _count)
                Array.Copy(_items, index, _items, index + 1, _count - index);
            
            _items[index] = item;
            _count++;
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException();

            _count--;
            if (index < _count)
                Array.Copy(_items, index + 1, _items, index, _count - index);
            _items[_count] = default;
        }

        private void EnsureCapacity(int min)
        {
            if (_items.Length < min)
            {
                int newCapacity = Math.Max(_items.Length * 2, min);
                T[] newItems = new T[newCapacity];
                Array.Copy(_items, newItems, _count);
                _items = newItems;
            }
        }
    }
}