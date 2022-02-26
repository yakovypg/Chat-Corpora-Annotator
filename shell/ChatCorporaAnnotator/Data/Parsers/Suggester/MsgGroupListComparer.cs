using ChatCorporaAnnotator.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester
{
    using MsgGroupList = List<List<int>>;

    internal class MsgGroupListComparer : IComparer<MsgGroupList>
    {
        public int Compare(MsgGroupList x, MsgGroupList y)
        {
            bool isXEmpty = x.IsNullOrEmpty();
            bool isYEmpty = y.IsNullOrEmpty();

            if (isXEmpty && isYEmpty)
                return 0;

            if (isXEmpty)
                return -1;

            if (isYEmpty)
                return 1;

            var xFirstMsg = x[0][0];
            var yFirstMsg = y[0][0];

            return xFirstMsg.CompareTo(yFirstMsg);
        }
    }
}
