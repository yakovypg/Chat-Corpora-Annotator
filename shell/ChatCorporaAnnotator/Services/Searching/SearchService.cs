using IndexEngine.Containers;
using IndexEngine.Data.Paths;
using IndexEngine.Search;
using Lucene.Net.Documents;
using Lucene.Net.Queries;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Services.Searching
{
    internal class SearchService : ISearchService
    {
        public TopDocs Hits { get; set; }
        public Query UserQuery { get; set; }

        public FieldCacheTermsFilter UserFilter { get; set; }
        public FieldCacheRangeFilter<string> DateFilter { get; set; }

        public void ConstructDateFilter(string dateFieldKey, DateTime start, DateTime finish)
        {
            string startString = DateTools.DateToString(start, DateTools.Resolution.MILLISECOND);
            string finishString = DateTools.DateToString(finish, DateTools.Resolution.MILLISECOND);

            if (!string.IsNullOrEmpty(startString) && !string.IsNullOrEmpty(finishString))
            {
                DateFilter = FieldCacheRangeFilter.NewStringRange(dateFieldKey,
                     lowerVal: startString, includeLower: true,
                     upperVal: finishString, includeUpper: true);
            }
            else if (string.IsNullOrEmpty(startString) && !string.IsNullOrEmpty(finishString))
            {
                DateFilter = FieldCacheRangeFilter.NewStringRange(dateFieldKey,
                     lowerVal: startString, includeLower: true,
                     upperVal: null, includeUpper: false);
            }
            else if (!string.IsNullOrEmpty(startString) && string.IsNullOrEmpty(finishString))
            {
                DateFilter = FieldCacheRangeFilter.NewStringRange(dateFieldKey,
                     lowerVal: null, includeLower: false,
                     upperVal: finishString, includeUpper: true);
            }
        }

        public void ConstructUserFilter(string senderFieldKey, string[] users)
        {
            UserFilter = new FieldCacheTermsFilter(senderFieldKey, users);
        }

        public List<DynamicMessage> MakeSearchResultsReadable()
        {
            List<DynamicMessage> searchResults = new List<DynamicMessage>();

            for (int i = 0; i < Hits.TotalHits; i++)
            {
                List<string> data = new List<string>();
                ScoreDoc d = Hits.ScoreDocs[i];
                Document idoc = LuceneService.Searcher.Doc(d.Doc);

                foreach (var field in ProjectInfo.Data.SelectedFields)
                {
                    data.Add(idoc.GetField(field).GetStringValue());
                }

                DynamicMessage message = new DynamicMessage(data, ProjectInfo.Data.SelectedFields, ProjectInfo.DateFieldKey,
                idoc.GetField(ProjectInfo.IdKey).GetInt32Value().Value);

                searchResults.Add(message);
            }

            return searchResults;
        }

        public void SearchText(int count)
        {
            if (UserQuery == null)
                return;

            Hits = LuceneService.Searcher.Search(UserQuery, count);
        }

        public void SearchText_DateFilter(int count)
        {
            if (UserQuery == null || DateFilter == null)
                return;

            Hits = LuceneService.Searcher.Search(UserQuery, DateFilter, count);
        }

        public void SearchText_UserDateFilter(int count)
        {
            if (UserQuery == null || DateFilter == null || UserFilter == null)
                return;

            var filter = new BooleanFilter
            {
                new FilterClause(DateFilter, Occur.MUST),
                new FilterClause(UserFilter, Occur.MUST)
            };

            Hits = LuceneService.Searcher.Search(UserQuery, filter, count);
        }

        public void SearchText_UserFilter(int count)
        {
            if (UserQuery == null || UserFilter == null)
                return;

            Hits = LuceneService.Searcher.Search(UserQuery, UserFilter, count);
        }
    }
}
