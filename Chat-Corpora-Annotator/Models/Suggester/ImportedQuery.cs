using System.Linq;

namespace ChatCorporaAnnotator.Models.Suggester
{
    internal class ImportedQuery : IImportedQuery
    {
        public string Name { get; }
        public string Content { get; }
        public string Presenter { get; }

        public ImportedQuery(string name, string content)
        {
            Name = name;
            Content = content;
            Presenter = string.IsNullOrEmpty(name) ? content : name;
        }

        public override string ToString()
        {
            return Presenter;
        }

        public static IImportedQuery Parse(string text, char delimiter = '\0')
        {
            if (string.IsNullOrEmpty(text))
                return new ImportedQuery(string.Empty, string.Empty);

            if (delimiter == '\0' || !text.Contains(delimiter))
                return new ImportedQuery(string.Empty, text);

            int index = text.IndexOf(delimiter);

            string name = text.Remove(index);
            string content = text.Substring(index + 1);

            if (string.IsNullOrEmpty(name))
                name = "unnamed";

            return new ImportedQuery(name, content);
        }
    }
}
