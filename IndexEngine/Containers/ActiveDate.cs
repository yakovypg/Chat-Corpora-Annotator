using System;

namespace IndexEngine.Containers
{
    // Do not use MessageId in the Equals(), GetHashCode(), operator==()

    public class ActiveDate : IEquatable<ActiveDate>
    {
        public DateTime Date { get; }
        public int MessageId { get; }

        public ActiveDate(DateTime date, int messageId)
        {
            Date = date;
            MessageId = messageId;
        }

        public override string ToString()
        {
            return Date.ToString();
        }

        public bool Equals(ActiveDate other)
        {
            return Date == other.Date;
        }

        public override bool Equals(object obj)
        {
            return obj is ActiveDate other && Date == other.Date;
        }

        public override int GetHashCode()
        {
            return Date.GetHashCode();
        }
    }
}
