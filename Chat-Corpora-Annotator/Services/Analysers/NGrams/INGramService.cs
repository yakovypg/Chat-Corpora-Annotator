using CSharpTest.Net.Collections;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Services.Analysers.NGrams
{
    internal interface INGramService
    {
        bool IndexExists { get; }
        bool IndexIsRead { get; }

        List<BTreeDictionary<string, int>> GetReadableResultsForTerm(string term);

        void ReadIndexFromDisk();
        void BuildFullIndex();
        void CheckIndex();
    }
}
