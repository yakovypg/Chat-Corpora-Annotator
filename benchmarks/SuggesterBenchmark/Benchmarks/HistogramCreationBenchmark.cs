using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using ChatCorporaAnnotator.Data.Parsers.Suggester.Histograms;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class HistogramCreationBenchmark : BenchmarkBase
    {
        private readonly HashSet<MsgGroupHistogram> _loadedHistograms;
        private readonly List<int>[] _loadedHistogramGroups;

        public HistogramCreationBenchmark()
        {
            QueryParser.TryLoadHistograms(out _loadedHistograms);
            _loadedHistogramGroups = _loadedHistograms.Select(t => t.MsgGroup).ToArray();
        }

        [Benchmark(Description = "GroupsFromLoadedHistograms")]
        public void CreateHistogramsTest_0()
        {
            var result = MsgGroupHistogram.CreateHistograms(
                MsgGroupHistogram.DEFAULT_HISTOGRAM_INTERVAL, _loadedHistogramGroups);
        }
    }
}
