namespace ChatCorporaAnnotator.Models.History
{
    internal interface IRecentProject
    {
        string Name { get; }
        string Path { get; }
        object Icon { get; set; }
    }
}
