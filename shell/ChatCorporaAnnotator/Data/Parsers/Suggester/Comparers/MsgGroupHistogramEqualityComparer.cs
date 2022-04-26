using ChatCorporaAnnotator.Data.Parsers.Suggester.Histograms;
using System.Collections.Generic;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester.Comparers
{
    public class MsgGroupHistogramEqualityComparer : IEqualityComparer<MsgGroupHistogram>
    {
        private readonly MsgGroupEqualityComparer _msgGroupComparer = new();

        public bool Equals(MsgGroupHistogram? x, MsgGroupHistogram? y)
        {
            return (x == null && y == null) ||
                   (x != null && y != null && _msgGroupComparer.Equals(x.MsgGroup, y.MsgGroup));
        }

        public int GetHashCode(MsgGroupHistogram obj)
        {
            return _msgGroupComparer.GetHashCode(obj.MsgGroup);
        }
    }
}
