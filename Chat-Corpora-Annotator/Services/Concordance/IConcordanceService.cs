using Lucene.Net.Search;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Services.Concordance
{
    internal interface IConcordanceService
    {
        Query ConQuery { get; set; }
        List<string> Concordance { get; set; }

        void FindConcordance(string query, string TextFieldKey, int count);
    }
}
