using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class ParseQueryBenchmark : BenchmarkBase
    {
        [Params(
            "select haswordofdict(job)",
            "select haswordofdict(job) or haswordofdict(skill)",
            "select haswordofdict(job) and haswordofdict(skill)",
            "select haswordofdict(job), haswordofdict(skill) inwin 5",
            "select haswordofdict(job), haswordofdict(skill) inwin 50",
            "select haswordofdict(job), haswordofdict(skill) unr inwin 5",
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev)",
            "select (select haswordofdict(job), haswordofdict(skill) inwin 5); (select haswordofdict(area) or haswordofdict(money) or haswordofdict(dev)) inwin 15",
            "select (select haswordofdict(job), haswordofdict(skill) inwin 5); (select haswordofdict(area) or haswordofdict(money), haswordofdict(dev) unr inwin 5) inwin 15"
        )]
        public string Query;

        [Benchmark]
        public void ParseTest()
        {
            var result = QueryParser.Parse(Query);
        }
    }
}
