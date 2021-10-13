using IndexingServices.Containers;
using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Models.Chat
{
    // Do not use MessageId in the Equals(), GetHashCode(), operator==()

    internal class MessageDate : IMessageDate, IEquatable<MessageDate>
    {
        public DateTime Date { get; }
        public int MessageId { get; }

        public string Presenter => Date.ToString();

        public string ShortDatePresenter => Date.ToShortDateString();
        public string LongDatePresenter => Date.ToLongDateString();

        public string LongTimePresenter => Date.ToLongTimeString();
        public string ShortTimePresenter => Date.ToShortTimeString();

        public MessageDate(ActiveDate activeDate) : this(activeDate.Date, activeDate.MessageId)
        {
        }

        public MessageDate(DateTime date, int messageId)
        {
            Date = date;
            MessageId = messageId;
        }

        public bool Equals(MessageDate other)
        {
            return Date == other.Date;
        }

        public override string ToString()
        {
            return Presenter;
        }

        public override bool Equals(object obj)
        {
            return obj is MessageDate other && Date == other.Date;
        }

        public override int GetHashCode()
        {
            int hashCode = -1023751529;

            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Presenter);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ShortDatePresenter);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LongDatePresenter);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LongTimePresenter);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ShortTimePresenter);

            return hashCode;
        }
    }
}
