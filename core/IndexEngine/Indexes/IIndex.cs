namespace IndexEngine.Indexes
{
    public interface IIndex<TKey, TValue>
    {
        int ItemCount { get; }
        IDictionary<TKey, TValue> IndexCollection { get; }

        void UnloadData();

        bool CheckFiles();
        bool CheckDirectory();

        void FlushIndexToDisk();
        void ReadIndexFromDisk();

        int GetValueCount(TKey key);
        bool DeleteIndexEntry(TKey key);
        void AddIndexEntry(TKey key, TValue value);
        void UpdateIndexEntry(TKey key, TValue value);
    }
}
