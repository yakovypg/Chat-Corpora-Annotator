using System;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.AppEventArgs
{
    public class LuceneQueryEventArgs : EventArgs
    {
        public string Query { get; }
        public int Count { get; }

        public string[] Users { get; }
        public DateTime Start { get; }
        public DateTime Finish { get; }
        public bool Window { get; }

        public bool FilteredByDate { get; }
        public bool FilteredByUser { get; }

        public LuceneQueryEventArgs(string query, int count, string[] users, DateTime[] dates, bool window)
        {
            Query = query;
            Count = count;
            Start = dates[0];
            Finish = dates[1];
            Users = users;
            FilteredByUser = true;
            FilteredByDate = true;
            Window = window;
        }

        public LuceneQueryEventArgs(string query, int count, DateTime[] dates, bool window)
        {
            Query = query;
            Count = count;
            Start = dates[0];
            Finish = dates[1];

            FilteredByUser = false;
            FilteredByDate = true;
            Window = window;
        }

        public LuceneQueryEventArgs(string query, bool window)
        {
            Query = query;
            Window = window;
            FilteredByUser = false;
            FilteredByDate = false;
        }
        public LuceneQueryEventArgs(string query, int count, bool window)
        {
            Query = query;
            Count = count;

            Window = window;
            FilteredByUser = false;
            FilteredByDate = false;
        }

        public LuceneQueryEventArgs(string query, int count, string[] users, bool window)
        {
            Query = query;
            Count = count;

            Users = users;
            FilteredByUser = true;
            FilteredByDate = false;
            Window = window;
        }
    }

    public delegate void LuceneQueryEventHandler(object sender, LuceneQueryEventArgs args);
}
