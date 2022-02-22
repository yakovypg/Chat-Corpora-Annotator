using Microsoft.VisualBasic.FileIO;
using SoftCircuits.CsvParser;

namespace ChatCorporaAnnotator.Services.Csv
{
    internal class CsvReadService : ICsvReadService
    {
        public string[] GetFields(string path, string delimiter)
        {
            string[] fields;

            using (var parser = new TextFieldParser(path))
            {
                parser.SetDelimiters(delimiter); // delimiter select
                fields = parser.ReadFields();
            }

            return fields;
        }

        public int GetLineCount(string path, bool header)
        {
            int count = 0;
            string[] row = null;

            using (var csv = new CsvReader(path))
            {
                if (header)
                    csv.ReadRow(ref row); // header read;

                while (csv.ReadRow(ref row))
                    count++;
            }
            return count;
        }
    }
}
