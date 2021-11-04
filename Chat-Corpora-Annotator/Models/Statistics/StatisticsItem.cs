namespace ChatCorporaAnnotator.Models.Statistics
{
    internal struct StatisticsItem : IStatisticsItem
    {
        public string Name { get; }
        public double Value { get; }

        public StatisticsItem(string name, double value)
        {
            Name = name;
            Value = value;
        }
    }
}
