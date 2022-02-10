using BenchmarkDotNet.Attributes;
using ChatCorporaAnnotator.Data.Parsers.Suggester;
using SuggesterBenchmark.Benchmarks.Base;

namespace SuggesterBenchmark.Benchmarks
{
    public class ParseQueryBenchmark : BenchmarkBase
    {
        [Params(
            // "query", // query_length
            "select haswordofdict(job)", // 25
            "select hasusermentioned(odrisck)", // 32
            "select byuser(odrisck)", // 22
            "select haswordofdict(job) or haswordofdict(skill)", // 49
            "select haswordofdict(job) or byuser(phgilliam)", // 46
            "select haswordofdict(job) and haswordofdict(skill)", // 50
            "select haswordofdict(job) and not haswordofdict(skill)", // 54
            "select haswordofdict(job), haswordofdict(skill) inwin 3", // 55
            "select haswordofdict(job), haswordofdict(skill) inwin 50", // 56
            "select haswordofdict(job), haswordofdict(skill) mess inwin 3", // 60
            "select haswordofdict(job), haswordofdict(skill)", // 47
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev)", // 67
            "select haswordofdict(job), haswordofdict(skill), haswordofdict(dev) mess", // 72
            "select haswordofdict(job) or haswordofdict(dev), haswordofdict(skill)", // 69
            "select haswordofdict(job) or haswordofdict(dev), haswordofdict(skill) mess inwin 3", // 82
            "select (select haswordofdict(job), haswordofdict(skill) inwin 5); (select haswordofdict(area) or haswordofdict(money) or haswordofdict(dev)) inwin 5", // 148
            "select (select haswordofdict(job), haswordofdict(skill) inwin 5); (select haswordofdict(area) or haswordofdict(dev), haswordofdict(money) or haswordofdict(dev) inwin 5) inwin 5", // 169
            "select (select haswordofdict(job), haswordofdict(skill) inwin 5); (select haswordofdict(area) or haswordofdict(dev), haswordofdict(dev) mess inwin 5) inwin 5" // 150
        )]
        public string Query;

        [Benchmark]
        public void ParseTest()
        {
            var result = QueryParser.Parse(Query);
        }
    }
}
