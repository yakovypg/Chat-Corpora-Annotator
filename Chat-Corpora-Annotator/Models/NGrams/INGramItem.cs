namespace ChatCorporaAnnotator.Models.NGrams
{
    internal interface INGramItem
    {
        string Phrase { get; }
        int Count { get; }
    }
}
