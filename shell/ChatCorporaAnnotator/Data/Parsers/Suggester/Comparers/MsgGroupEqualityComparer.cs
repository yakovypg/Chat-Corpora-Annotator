using System.Collections.Generic;
using System.Linq;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester.Comparers
{
    using MsgGroup = List<int>;

    public class MsgGroupEqualityComparer : IEqualityComparer<MsgGroup>
    {
        public bool Equals(MsgGroup? x, MsgGroup? y)
        {
            return (x == null && y == null) ||
                   (x != null && y != null && x.SequenceEqual(y));
        }

        public int GetHashCode(MsgGroup obj)
        {
            return obj.Aggregate(0, (sum, item) => sum ^ item.GetHashCode());
        }
    }
}
