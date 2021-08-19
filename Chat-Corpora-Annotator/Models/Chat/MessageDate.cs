using System;

namespace ChatCorporaAnnotator.Models.Chat
{
    internal struct MessageDate : IMessageDate
    {
        public DateTime Source { get; }

        public string Presenter => Source.ToString();

        public string ShortDatePresenter => Source.ToShortDateString();
        public string LongDatePresenter => Source.ToLongDateString();

        public string LongTimePresenter => Source.ToLongTimeString();
        public string ShortTimePresenter => Source.ToShortTimeString();

        public MessageDate(DateTime source)
        {
            Source = source;
        }

        public override string ToString()
        {
            return Presenter;
        }
    }
}
