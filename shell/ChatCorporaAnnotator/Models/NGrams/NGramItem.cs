namespace ChatCorporaAnnotator.Models.NGrams
{
    internal struct NGramItem : INGramItem
    {
        public string Phrase { get; }
        public int Count { get; }

        public NGramItem(string phrase, int count)
        {
            Phrase = phrase;
            Count = count;
        }
    }
}
