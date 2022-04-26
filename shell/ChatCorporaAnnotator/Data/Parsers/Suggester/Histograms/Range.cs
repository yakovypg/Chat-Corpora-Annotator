using System;

namespace ChatCorporaAnnotator.Data.Parsers.Suggester.Histograms
{
    public struct Range
    {
        public int Start { get; }
        public int End { get; }
        public int Value { get; set; }

        public Range(int start, int end, int value = 0)
        {
            Start = start;
            End = end;
            Value = value;
        }

        public bool IsHit(int value)
        {
            return value >= Start && value <= End;
        }

        public Range IntersectValue(Range other)
        {
            int minValue = Math.Min(Value, other.Value);
            return new Range(Start, End, minValue);
        }

        public Range Intersect(Range other)
        {
            int minValue = Math.Min(Value, other.Value);
            int maxStart = Math.Max(Start, other.Start);
            int minEnd = Math.Min(End, other.End);

            return new Range(maxStart, minEnd, minValue);
        }
    }
}
