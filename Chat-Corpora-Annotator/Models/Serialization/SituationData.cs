using System.Collections.Generic;

namespace ChatCorporaAnnotator.Models.Serialization
{
    internal class SituationData
    {
        public int Id { get; }
        public string Header { get; }
        public List<int> Messages { get; }

        public SituationData(int id, string header, IEnumerable<int> messages)
        {
            Id = id;
            Header = header;
            Messages = new List<int>(messages ?? new int[0]);
        }

        public override string ToString()
        {
            return $"{Header} {Id}";
        }

        public override bool Equals(object obj)
        {
            return obj is SituationData other &&
                   Id == other.Id &&
                   Header == other.Header;
        }

        public override int GetHashCode()
        {
            int hashCode = -1985413547;

            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Header);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<int>>.Default.GetHashCode(Messages);

            return hashCode;
        }
    }
}
