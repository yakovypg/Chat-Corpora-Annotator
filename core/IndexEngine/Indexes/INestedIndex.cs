namespace IndexEngine.Indexes
{
    public interface INestedIndex<TKey, TValue, K, V> : IIndex<TKey, TValue> where TValue : IDictionary<K, V>
    {
        void InitializeIndex(List<TKey> list);

        int GetInnerValueCount(TKey key, K inkey);
        bool DeleteInnerIndexEntry(TKey key, K inkey);
        void AddInnerIndexEntry(TKey key, K inkey, V invalue);
    }
}
