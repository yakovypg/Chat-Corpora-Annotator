namespace ChatCorporaAnnotator.Models.Indexing
{
    internal interface IFileColumn
    {
        string Header { get; }
        bool IsSelected { get; set; }
    }
}
