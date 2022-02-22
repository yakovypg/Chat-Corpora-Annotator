namespace ChatCorporaAnnotator.Models.Suggester
{
    internal interface IImportedQuery
    {
        string Name { get; }
        string Content { get; }
        string Presenter { get; }
    }
}
