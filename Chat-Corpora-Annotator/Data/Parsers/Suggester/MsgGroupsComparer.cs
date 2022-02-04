using System.Collections.Generic;
using System.Linq;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester
{
    using MsgGroups = List<List<int>>;

    internal class MsgGroupsComparer : IComparer<MsgGroups>
    {
        public int Compare(MsgGroups x, MsgGroups y)
        {
            if (x == null || x.Count == 0)
                return -1;

            if (y == null || y.Count == 0)
                return 1;

            var xFirstMsg = x.First().First();
            var yFirstMsg = y.First().First();

            return xFirstMsg.CompareTo(yFirstMsg);
        }
    }
}
