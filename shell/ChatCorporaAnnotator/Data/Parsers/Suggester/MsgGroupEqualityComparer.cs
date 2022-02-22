using System.Collections.Generic;
using System.Linq;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester
{
    using MsgGroup = List<int>;

    internal class MsgGroupEqualityComparer<T> : IEqualityComparer<MsgGroup>
    {
        public bool Equals(MsgGroup x, MsgGroup y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;

            if (x.Count != y.Count)
                return false;

            for (int i = 0; i < x.Count; ++i)
            {
                if (!x[i].Equals(y[i]))
                    return false;
            }

            return true;
        }

        public int GetHashCode(MsgGroup obj)
        {
            return obj.Aggregate(0, (sum, item) => sum ^ item.GetHashCode());
        }
    }
}
