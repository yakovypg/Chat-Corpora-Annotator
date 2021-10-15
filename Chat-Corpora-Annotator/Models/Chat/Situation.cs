using System.Collections.Generic;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal class Situation : ISituation
    {
        public int Id { get; set; }
        public string Header { get; set; }

        public Situation(int id, string header)
        {
            Id = id;
            Header = header;
        }

        public override string ToString()
        {
            return Header;
        }

        public override bool Equals(object obj)
        {
            return obj is Situation other &&
                   Id == other.Id &&
                   Header == other.Header;
        }

        public override int GetHashCode()
        {
            int hashCode = -1895679360;

            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Header);

            return hashCode;
        }
    }
}
