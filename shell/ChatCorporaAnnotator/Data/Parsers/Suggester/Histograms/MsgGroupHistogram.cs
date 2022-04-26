using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester.Histograms
{
    public class MsgGroupHistogram : ICloneable
    {
        public const int DEFAULT_HISTOGRAM_INTERVAL = 100;

        public List<int> MsgGroup { get; }

        public int Interval { get; }
        public int AxisXLength { get; }
        public Range[] Hits { get; private set; }

        public int MaxX => AxisXLength;
        public int MaxY => Hits.Max(t => t.Value);
        public int HitsSum => Hits.Sum(t => t.Value);

        public MsgGroupHistogram(List<int> msgGroup, int axisXLength, int interval, Range[]? hits = null)
        {
            if (interval <= 0)
                throw new ArgumentException("Interval must be greater than zero.");

            if (interval > axisXLength)
                interval = axisXLength;

            MsgGroup = msgGroup;
            Interval = interval;
            AxisXLength = axisXLength;

            if (hits == null)
            {
                Hits = Array.Empty<Range>();
                CountHits(msgGroup);
            }
            else
            {
                Hits = new Range[hits.Length];
                hits.CopyTo(Hits, 0);
            }
        }

        public static MsgGroupHistogram[] CreateHistograms(int interval = DEFAULT_HISTOGRAM_INTERVAL, params List<int>[] groups)
        {
            int axisXLength = groups.Max(t => t.Count) + 1;
            var histograms = new MsgGroupHistogram[groups.Length];

            for (int i = 0; i < histograms.Length; ++i)
                histograms[i] = new MsgGroupHistogram(groups[i], axisXLength, interval);

            return histograms;
        }

        public object Clone()
        {
            return new MsgGroupHistogram(MsgGroup, AxisXLength, Interval, Hits);
        }

        public MsgGroupHistogram Intersect(MsgGroupHistogram other)
        {
            if (Interval != other.Interval)
                throw new ArgumentException("It is possible to cross only histograms with the same interval.");

            int minLength = Math.Min(Hits.Length, other.Hits.Length);
            var newHits = new Range[minLength];

            for (int i = 0; i < minLength; ++i)
                newHits[i] = Hits[i].IntersectValue(other.Hits[i]);

            return new MsgGroupHistogram(MsgGroup, AxisXLength, Interval, newHits);
        }

        private void CountHits(List<int> msgGroup)
        {
            int rangeStart = 0;
            int intervalsCount = AxisXLength / Interval;

            Hits = new Range[intervalsCount];

            for (int i = 0; i < Hits.Length - 1; ++i)
            {
                int rangeEnd = rangeStart + Interval - 1;
                Hits[i] = new Range(rangeStart, rangeEnd);

                rangeStart += Interval;
            }

            Hits[^1] = new Range(rangeStart, AxisXLength - 1);

            for (int i = 0; i < msgGroup.Count; ++i)
            {
                int curMsg = msgGroup[i];

                for (int j = 0; j < Hits.Length; ++j)
                {
                    if (Hits[j].IsHit(curMsg))
                    {
                        Hits[j].Value++;
                        break;
                    }
                }
            }
        }
    }
}
