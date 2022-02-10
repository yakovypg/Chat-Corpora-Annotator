using BenchmarkDotNet.Running;
using SuggesterBenchmark.Benchmarks;

namespace SuggesterBenchmark
{
    internal class Program
    {
        private static void Main()
        {
            BenchmarkRunner.Run<MergeQueriesBenchmark>();
            BenchmarkRunner.Run<MergeRestrictionsBenchmark>();
            BenchmarkRunner.Run<ParseQueryBenchmark>();
            BenchmarkRunner.Run<VisitConditionBenchmark>();
            BenchmarkRunner.Run<VisitQueryAndBodyBenchmark>();
            BenchmarkRunner.Run<VisitQuerySequenceBenchmark>();
            BenchmarkRunner.Run<VisitRestrictionBenchmark>();
            BenchmarkRunner.Run<VisitRestrictionsBenchmark>();
        }
    }
}