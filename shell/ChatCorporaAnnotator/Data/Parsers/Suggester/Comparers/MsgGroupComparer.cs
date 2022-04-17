using System;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester.Comparers
{
    using MagGroup = List<int>;

    public class MsgGroupComparer : IComparer<MagGroup>
    {
        public int Compare(MagGroup? x, MagGroup? y)
        {
            if (x == null && y == null)
                return 0;

            if (x == null)
                return -1;

            if (y == null)
                return 1;

            if (x.Count == 0 && y.Count == 0)
                return 0;

            if (x.Count == 0)
                return -1;

            if (y.Count == 0)
                return 1;

            int minCount = Math.Min(x.Count, y.Count);

            for (int i = 0; i < minCount; ++i)
            {
                int cmpRes = x[i].CompareTo(y[i]);

                if (cmpRes != 0)
                    return cmpRes;
            }

            return x.Count.CompareTo(y.Count);
        }
    }
}
