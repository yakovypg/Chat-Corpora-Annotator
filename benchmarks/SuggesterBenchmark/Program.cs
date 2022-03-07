using BenchmarkDotNet.Running;
using SuggesterBenchmark.Benchmarks;

BenchmarkRunner.Run<MergeQueriesBenchmark>();
BenchmarkRunner.Run<MergeRestrictionsBenchmark>();

BenchmarkRunner.Run<ParseQueryBenchmark>();
BenchmarkRunner.Run<VisitConditionBenchmark>();
BenchmarkRunner.Run<VisitQueryBenchmark>();
BenchmarkRunner.Run<VisitQuerySequenceBenchmark>();
BenchmarkRunner.Run<VisitRestrictionBenchmark>();
BenchmarkRunner.Run<VisitRestrictionsBenchmark>();

BenchmarkRunner.Run<ControlBenchmark>();