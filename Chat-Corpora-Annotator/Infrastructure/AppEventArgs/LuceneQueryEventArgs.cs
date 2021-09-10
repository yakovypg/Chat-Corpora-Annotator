using System;

namespace ChatCorporaAnnotator.Infrastructure.AppEventArgs
{
    internal class LuceneQueryEventArgs : EventArgs
    {
        public int MessagesCount { get; }

        public string Query { get; }
        public string[] Users { get; }

        public DateTime StartDate { get; }
        public DateTime EndDate { get; }

        public bool FilteredByDate { get; }
        public bool FilteredByUser { get; }

        public LuceneQueryEventArgs(string query, int messagesCount = 0, string[] users = null, DateTime[] dates = null)
        {
            MessagesCount = messagesCount;

            Query = query;
            Users = users;

            if (dates != null)
            {
                StartDate = dates[0];
                EndDate = dates[1];
            }

            FilteredByDate = dates != null;
            FilteredByUser = users != null;
        }
    }

    internal delegate void LuceneQueryEventHandler(object sender, LuceneQueryEventArgs args);
}
