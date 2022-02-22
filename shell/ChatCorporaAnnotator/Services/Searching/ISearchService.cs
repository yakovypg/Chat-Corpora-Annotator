using IndexEngine.Containers;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Services.Searching
{
    internal interface ISearchService
    {
        TopDocs Hits { get; set; }
        Query UserQuery { get; set; }

        FieldCacheTermsFilter UserFilter { get; set; }
        FieldCacheRangeFilter<string> DateFilter { get; set; }

        void ConstructUserFilter(string senderFieldKey, string[] users);
        void ConstructDateFilter(string dateFieldKey, DateTime start, DateTime finish);

        void SearchText(int count);
        void SearchText_UserFilter(int count);
        void SearchText_DateFilter(int count);
        void SearchText_UserDateFilter(int count);

        List<DynamicMessage> MakeSearchResultsReadable();
    }
}
