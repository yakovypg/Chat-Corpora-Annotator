using System.Collections.Generic;

namespace IndexEngine.Indexes
{
    public interface IIndex<TKey, TValue> : IUnloadable
    {
        IDictionary<TKey, TValue> IndexCollection { get; }
        int ItemCount { get; }
        bool CheckFiles();
        bool CheckDirectory();
        void ReadIndexFromDisk();
        void FlushIndexToDisk();

        void UpdateIndexEntry(TKey key, TValue value);
        void DeleteIndexEntry(TKey key);
        void AddIndexEntry(TKey key, TValue value);
        int GetValueCount(TKey key);
    }
    public interface INestedIndex<TKey, TValue, K, V> : IIndex<TKey, TValue> where TValue : IDictionary<K, V>
    {
        void AddInnerIndexEntry(TKey key, K inkey, V invalue);
        void DeleteInnerIndexEntry(TKey key, K inkey);
        void InitializeIndex(List<TKey> list);
        int GetInnerValueCount(TKey key, K inkey);
    }

    public interface IContainer<T> : IUnloadable
    {
        IEnumerable<T> TypeContainer { get; }
        void Add(T item);
        void AddRange(IEnumerable<T> items);
    }

    public interface IUnloadable
    {
        void UnloadData();
    }
}
