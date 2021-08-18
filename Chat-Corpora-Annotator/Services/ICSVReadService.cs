namespace ChatCorporaAnnotator.Services
{
    internal interface ICSVReadService
    {
        string[] GetFields(string path, string delimiter);
        int GetLineCount(string path, bool header);
    }
}
