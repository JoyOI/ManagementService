using System;
using System.Collections.Generic;
using System.Text;

namespace JoyOI.ManagementService.Utils
{
    /// <summary>
    /// LRU缓存类
    /// 来源于: https://stackoverflow.com/questions/754233/is-it-there-any-lru-implementation-of-idictionary
    /// </summary>
    public class LRUCache<K, V>
    {
        private int _capacity;
        private Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>> _cacheMap;
        private LinkedList<LRUCacheItem<K, V>> _lruList;

        public LRUCache(int capacity)
        {
            _capacity = capacity;
            if (typeof(K) == typeof(byte[]))
            {
                _cacheMap = new Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>>(
                    (IEqualityComparer<K>)new ByteArrayComparer());
            }
            else
            {
                _cacheMap = new Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>>();
            }
            _lruList = new LinkedList<LRUCacheItem<K, V>>();
        }

        public V Get(K key)
        {
            lock (this)
            {
                LinkedListNode<LRUCacheItem<K, V>> node;
                if (_cacheMap.TryGetValue(key, out node))
                {
                    V value = node.Value.value;
                    _lruList.Remove(node);
                    _lruList.AddLast(node);
                    return value;
                }
                return default(V);
            }
        }

        public void Set(K key, V val)
        {
            lock (this)
            {
                LinkedListNode<LRUCacheItem<K, V>> node;
                if (_cacheMap.TryGetValue(key, out node))
                {
                    _lruList.Remove(node);
                    _cacheMap.Remove(key);
                }

                if (_cacheMap.Count >= _capacity)
                {
                    RemoveFirst();
                }

                var cacheItem = new LRUCacheItem<K, V>(key, val);
                node = new LinkedListNode<LRUCacheItem<K, V>>(cacheItem);
                _lruList.AddLast(node);
                _cacheMap.Add(key, node);
            }
        }

        private void RemoveFirst()
        {
            // Remove from LRUPriority
            LinkedListNode<LRUCacheItem<K, V>> node = _lruList.First;
            _lruList.RemoveFirst();

            // Remove from cache
            _cacheMap.Remove(node.Value.key);
        }

        class LRUCacheItem<K, V>
        {
            public LRUCacheItem(K k, V v)
            {
                key = k;
                value = v;
            }
            public K key;
            public V value;
        }

        class ByteArrayComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] left, byte[] right)
            {
                if (left == null || right == null)
                {
                    return left == right;
                }
                if (left.Length != right.Length)
                {
                    return false;
                }
                for (int i = 0; i < left.Length; i++)
                {
                    if (left[i] != right[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public int GetHashCode(byte[] key)
            {
                if (key == null)
                    throw new ArgumentNullException("key");
                int sum = 0;
                foreach (byte cur in key)
                {
                    sum += cur;
                }
                return sum;
            }
        }
    }
}
