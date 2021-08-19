namespace ChatCorporaAnnotator.Models.Chat
{
    internal class Situation : ISituation
    {
        public int ID { get; set; }
        public string Header { get; set; }

        public Situation(int id, string header)
        {
            ID = id;
            Header = header;
        }

        public override string ToString()
        {
            return Header;
        }
    }
}
