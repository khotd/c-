using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab3.Collections
{
    public class SimpleDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private class Entry
        {
            public TKey Key;
            public TValue Value;
            public Entry Next;
        }

        private Entry[] _buckets = new Entry[16]; // Простое число
        private int _count;

        public TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out TValue value))
                    return value;
                throw new KeyNotFoundException();
            }
            set => AddOrUpdate(key, value);
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var keys = new List<TKey>();
                foreach (var kvp in this)
                    keys.Add(kvp.Key);
                return keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var values = new List<TValue>();
                foreach (var kvp in this)
                    values.Add(kvp.Value);
                return values;
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
        
        public int Count => _count;
        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            if (ContainsKey(key))
                throw new ArgumentException("Дубликат ключа");
            
            AddInternal(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public void Clear()
        {
            Array.Clear(_buckets, 0, _buckets.Length);
            _count = 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
            => ContainsKey(item.Key) && EqualityComparer<TValue>.Default.Equals(this[item.Key], item.Value);

        public bool ContainsKey(TKey key) => FindEntry(key) != null;

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (var kvp in this)
                array[arrayIndex++] = kvp;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var bucket in _buckets)
            {
                for (var entry = bucket; entry != null; entry = entry.Next)
                {
                    yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Remove(TKey key)
        {
            int bucketIndex = GetBucketIndex(key);
            Entry current = _buckets[bucketIndex];
            Entry previous = null;

            while (current != null)
            {
                if (EqualityComparer<TKey>.Default.Equals(current.Key, key))
                {
                    if (previous == null)
                        _buckets[bucketIndex] = current.Next;
                    else
                        previous.Next = current.Next;
                    
                    _count--;
                    return true;
                }
                previous = current;
                current = current.Next;
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        public bool TryGetValue(TKey key, out TValue value)
        {
            var entry = FindEntry(key);
            if (entry != null)
            {
                value = entry.Value;
                return true;
            }
            value = default;
            return false;
        }

        // Вспомогательные методы
        private Entry FindEntry(TKey key)
        {
            int bucketIndex = GetBucketIndex(key);
            for (var entry = _buckets[bucketIndex]; entry != null; entry = entry.Next)
            {
                if (EqualityComparer<TKey>.Default.Equals(entry.Key, key))
                    return entry;
            }
            return null;
        }

        private void AddInternal(TKey key, TValue value)
        {
            int bucketIndex = GetBucketIndex(key);
            var newEntry = new Entry { Key = key, Value = value, Next = _buckets[bucketIndex] };
            _buckets[bucketIndex] = newEntry;
            _count++;
        }

        private void AddOrUpdate(TKey key, TValue value)
        {
            var entry = FindEntry(key);
            if (entry != null)
                entry.Value = value;
            else
                AddInternal(key, value);
        }

        private int GetBucketIndex(TKey key)
            => Math.Abs((key?.GetHashCode() ?? 0) % _buckets.Length);
    }
}