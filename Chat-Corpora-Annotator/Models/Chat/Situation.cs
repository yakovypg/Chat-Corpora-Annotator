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
    }
}
