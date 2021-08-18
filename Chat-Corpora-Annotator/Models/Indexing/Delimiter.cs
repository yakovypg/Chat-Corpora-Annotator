namespace ChatCorporaAnnotator.Models.Indexing
{
    internal class Delimiter : IDelimiter
    {
        public string Name { get; }
        public string Source { get; }

        public Delimiter(string name, string source)
        {
            Name = name;
            Source = source;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is Delimiter other && Name == other.Name && Source == other.Source;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
