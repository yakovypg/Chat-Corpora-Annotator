using ChatCorporaAnnotator.Infrastructure.Extensions;
using ChatCorporaAnnotator.Models.Indexing;
using System.Linq;

namespace ChatCorporaAnnotator.Services.Csv
{
    internal class CsvColumnReadService : ICsvColumnReadService
    {
        public FileColumn[] GetColumns(string path, string delimiter)
        {
            var csvReader = new CsvReadService();
            string[] fields = csvReader.GetFields(path, delimiter);

            return !fields.IsNullOrEmpty()
                ? fields.Select(t => new FileColumn(t)).ToArray()
                : new FileColumn[0];
        }

        public bool TryGetColumns(string path, string delimiter, out FileColumn[]? columns)
        {
            try
            {
                columns = GetColumns(path, delimiter);
                return true;
            }
            catch
            {
                columns = null;
                return false;
            }
        }
    }
}
