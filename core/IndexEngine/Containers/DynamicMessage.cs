using IndexEngine.Data.Paths;
using Lucene.Net.Documents;

namespace IndexEngine.Containers
{
    public class DynamicMessage
    {
        public int Id { get; set; }

        public Dictionary<string, int> Situations { get; } = new Dictionary<string, int>();
        public Dictionary<string, object> Contents { get; } = new Dictionary<string, object>();

        public DynamicMessage(string[] fields, string[] data)
        {
            for (int i = 0; i < fields.Length; i++)
                Contents.Add(fields[i], data[i]);
        }

        public DynamicMessage(List<string> data, List<string> selectedFields, string dateFieldKey, int id)
        {
            Id = id;

            if (data.Count != selectedFields.Count)
                throw new ApplicationException("Wrong array size");

            for (int i = 0; i < data.Count; i++)
            {
                if (selectedFields[i] == dateFieldKey)
                    Contents.Add(selectedFields[i], DateTools.StringToDate(data[i]));
                else
                    Contents.Add(selectedFields[i], data[i]);
            }
        }

        public string[] GetContent(string[] fields)
        {
            if (fields == null || fields.Length == 0)
                fields = Contents.Keys.ToArray();

            string[] items = new string[fields.Length];

            for (int i = 0; i < fields.Length; ++i)
            {
                if (!Contents.TryGetValue(fields[i], out object? value))
                    throw new KeyNotFoundException($"Message does not contain the field {fields[i]}.");

                if (fields[i] == ProjectInfo.DateFieldKey)
                {
                    string dateText = value?.ToString() ?? string.Empty;
                    DateTime date = DateTime.Parse(dateText);

                    items[i] = date.ToString("O");
                }
                else
                {
                    items[i] = value?.ToString() ?? string.Empty;
                }
            }

            return items;
        }
    }
}