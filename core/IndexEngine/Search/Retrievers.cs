﻿using IndexEngine.Data.Paths;
using IndexEngine.NLP;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using System.Collections.Generic;
using System.Linq;

namespace IndexEngine.Search
{
    public enum NER
    {
        ORG,
        LOC,
        TIME,
        URL,
        DATE
    }

    public static class Retrievers
    {
        public static HashSet<int> HasWordOfList(List<string> words)
        {
            HashSet<int> results = new HashSet<int>();

            foreach (var word in words)
            {

                //TermQuery query = new TermQuery(new Lucene.Net.Index.Term(word));
                //BooleanQuery boolquery = new BooleanQuery();
                //boolquery.Add(query, Occur.MUST);

                //А на самом деле можно построить фильтр сразу на весь список слов без цикла.
                //string[] temp = new string[1];
                //temp[0] = word;
                //FieldCacheTermsFilter filter = new FieldCacheTermsFilter(ProjectInfo.TextFieldKey, temp);
                //var boolFilter = new BooleanFilter();
                //boolFilter.Add(new FilterClause(filter, Occur.MUST));

                Query query = LuceneService.Parser.Parse(word);
                TopDocs docs = LuceneService.Searcher.Search(query, LuceneService.DirReader.MaxDoc);

                foreach (var doc in docs.ScoreDocs)
                {
                    Document idoc = LuceneService.Searcher.IndexReader.Document(doc.Doc);
                    results.Add(idoc.GetField(ProjectInfo.IdKey).GetInt32Value().Value);
                }
            }

            return results;
        }

        public static List<int> HasNERTag(NER tag)
        {
            switch (tag)
            {
                case NER.ORG: return Extractor.OrgList.Keys.ToList();
                case NER.LOC: return Extractor.LocList.Keys.ToList();
                case NER.TIME: return Extractor.TimeList.Keys.ToList();
                case NER.URL: return Extractor.URLList.Keys.ToList();
                case NER.DATE: return Extractor.DateList.Keys.ToList();

                default: return new List<int>();
            }
        }

        public static List<int> HasQuestion()
        {
            return Extractor.IsQuestionList.ToList();
        }

        public static HashSet<int> HasUser(string user)
        {
            HashSet<int> results = new HashSet<int>();
            TermQuery query = new TermQuery(new Lucene.Net.Index.Term(ProjectInfo.SenderFieldKey, user));

            BooleanQuery boolquery = new BooleanQuery();
            boolquery.Add(query, Occur.MUST);

            TopDocs docs = LuceneService.Searcher.Search(boolquery, LuceneService.DirReader.MaxDoc);

            foreach (var doc in docs.ScoreDocs)
            {
                Document idoc = LuceneService.Searcher.IndexReader.Document(doc.Doc);
                results.Add(idoc.GetField(ProjectInfo.IdKey).GetInt32Value().Value);
            }

            return results;
        }

        public static HashSet<int> HasUserMentioned(string user)
        {
            HashSet<int> results = new HashSet<int>();
            Query query = LuceneService.Parser.Parse(user);
            TopDocs docs = LuceneService.Searcher.Search(query, LuceneService.DirReader.MaxDoc);

            foreach (var doc in docs.ScoreDocs)
            {
                Document idoc = LuceneService.Searcher.IndexReader.Document(doc.Doc);
                results.Add(idoc.GetField(ProjectInfo.IdKey).GetInt32Value().Value);
            }

            return results;
        }
    }
}