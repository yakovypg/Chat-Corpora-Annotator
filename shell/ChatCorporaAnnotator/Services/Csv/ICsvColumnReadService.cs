using ChatCorporaAnnotator.Models.Indexing;

namespace ChatCorporaAnnotator.Services.Csv
{
    internal interface ICsvColumnReadService
    {
        FileColumn[] GetColumns(string path, string delimiter);
        bool TryGetColumns(string path, string delimiter, out FileColumn[]? columns);
    }
}
