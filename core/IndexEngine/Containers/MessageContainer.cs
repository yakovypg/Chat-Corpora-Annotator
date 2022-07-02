using IndexEngine.Indexes;

namespace IndexEngine.Containers
{
    public static class MessageContainer
    {
        public static List<DynamicMessage> Messages { get; set; } = new List<DynamicMessage>();

        public static void InsertTagsInDynamicMessage(int id, int offset)
        {
            if (id > Messages.Count + offset)
                return;

            foreach (var str in SituationIndex.GetInstance().InvertedIndex[id])
            {
                Messages[id].Situations.TryAdd(str.Key, str.Value);
            }
        }

        public static void UpdateTagsInDynamicMessage(int id, int offset)
        {
            Messages[id].Situations.Clear();
            InsertTagsInDynamicMessage(id, offset);
        }
    }
}
