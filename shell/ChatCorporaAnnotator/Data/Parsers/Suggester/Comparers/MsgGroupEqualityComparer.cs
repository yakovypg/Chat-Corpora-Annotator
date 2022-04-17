using System.Collections.Generic;
using System.Linq;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester.Comparers
{
    using MsgGroup = List<int>;

    public class MsgGroupEqualityComparer : IEqualityComparer<MsgGroup>
    {
        public bool Equals(MsgGroup? x, MsgGroup? y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            bool isSeqEqual = x.SequenceEqual(y);
            return isSeqEqual;
        }

        public int GetHashCode(MsgGroup obj)
        {
            return obj.Aggregate(0, (sum, item) => sum ^ item.GetHashCode());
        }
    }
}
