using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester.Histograms
{
    internal class MsgGroupHistogram : ICloneable
    {
        public int Interval { get; }
        public int AxisXLength { get; }
        public Range[] Hits { get; private set; }

        public int MaxX => AxisXLength;
        public int MaxY => Hits.Max(t => t.Value);
        public int HitsSum => Hits.Sum(t => t.Value);

        public MsgGroupHistogram(List<int> msgGroup, int axisXLength, int interval)
        {
            if (interval <= 0)
                throw new ArgumentException("Interval must be greater than zero.");

            if (interval > axisXLength)
                interval = axisXLength;

            Interval = interval;
            AxisXLength = axisXLength;
            Hits = Array.Empty<Range>();

            CountHits(msgGroup);
        }

        public MsgGroupHistogram(Range[] hits, int axisXLength, int interval)
        {
            Interval = interval;
            AxisXLength = axisXLength;

            Hits = new Range[hits.Length];
            hits.CopyTo(Hits, 0);
        }

        public object Clone()
        {
            return new MsgGroupHistogram(Hits, AxisXLength, Interval);
        }

        public MsgGroupHistogram Intersect(MsgGroupHistogram other)
        {
            if (Interval != other.Interval)
                throw new ArgumentException("It is possible to cross only histograms with the same interval.");

            int minLength = Math.Min(Hits.Length, other.Hits.Length);
            var newHits = new Range[minLength];

            for (int i = 0; i < minLength; ++i)
                newHits[i] = Hits[i].IntersectValue(other.Hits[i]);

            return new MsgGroupHistogram(newHits, AxisXLength, Interval);
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
