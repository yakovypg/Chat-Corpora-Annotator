using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using SuggesterBenchmark.Benchmarks;
using System.Diagnostics;

namespace SuggesterBenchmark.CLI
{
    internal static class BenchmarkLauncher
    {
        public static IConfig? BenchmarkRunnerConfig { get; private set; }

        private static readonly Dictionary<string, Func<Summary>> Benchmarks;
        public static string[] SupportedBenchmarks => Benchmarks.Keys.ToArray();

        private static readonly Dictionary<string, Action> Parameters;
        public static string[] SupportedParameters => Parameters.Keys.ToArray();

        static BenchmarkLauncher()
        {
            Benchmarks = new Dictionary<string, Func<Summary>>()
            {
                { "ControlBenchmark", () => BenchmarkRunner.Run<ControlBenchmark>(BenchmarkRunnerConfig) },
                { "ExperimentBenchmark", () => BenchmarkRunner.Run<ExperimentBenchmark>(BenchmarkRunnerConfig) },
                { "MergeQueriesBenchmark", () => BenchmarkRunner.Run<MergeQueriesBenchmark>(BenchmarkRunnerConfig) },
                { "MergeRestrictionsBenchmark", () => BenchmarkRunner.Run<MergeRestrictionsBenchmark>(BenchmarkRunnerConfig) },
                { "MergeRestrictionsComparisonBenchmark", () => BenchmarkRunner.Run<MergeRestrictionsComparisonBenchmark>(BenchmarkRunnerConfig) },
                { "NaturalQueriesBenchmark", () => BenchmarkRunner.Run<NaturalQueriesBenchmark>(BenchmarkRunnerConfig) },
                { "ParseQueryBenchmark", () => BenchmarkRunner.Run<ParseQueryBenchmark>(BenchmarkRunnerConfig) },
                { "VisitConditionBenchmark", () => BenchmarkRunner.Run<VisitConditionBenchmark>(BenchmarkRunnerConfig) },
                { "VisitQueryBenchmark", () => BenchmarkRunner.Run<VisitQueryBenchmark>(BenchmarkRunnerConfig) },
                { "VisitQuerySequenceBenchmark", () => BenchmarkRunner.Run<VisitQuerySequenceBenchmark>(BenchmarkRunnerConfig) },
                { "VisitRestrictionBenchmark", () => BenchmarkRunner.Run<VisitRestrictionBenchmark>(BenchmarkRunnerConfig) },
                { "VisitRestrictionsBenchmark", () => BenchmarkRunner.Run<VisitRestrictionsBenchmark>(BenchmarkRunnerConfig) }
            };

            Parameters = new Dictionary<string, Action>()
            {
                { "no-log", () => SetNoLoggerConfig() }
            };
        }

        public static void StartBenchmarking(params string[] benchmarks)
        {
            if (benchmarks == null || benchmarks.Length == 0)
                return;
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (var benchmark in benchmarks)
            {
                ConsoleWriter.PrintMessage($"\n{benchmark} started\n", ConsoleColor.DarkYellow);

                if (RunBenchmark(benchmark, out Summary? summary))
                    ConsoleWriter.PrintBenchmarkSummary(summary, BenchmarkRunnerConfig);
                else
                    ConsoleWriter.PrintMessage($"Failed to launch {benchmark}", ConsoleColor.DarkYellow);
            }

            stopwatch.Stop();

            string message =
                $"\nAll benchmarks are passed." +
                $"\nTotal time: {stopwatch.Elapsed}\n";

            ConsoleWriter.PrintMessage(message, ConsoleColor.Green);
        }

        public static void ApplyParameters(params string[] parameters)
        {
            if (parameters == null)
                return;
            
            foreach (var parameter in parameters)
                ApplyParameter(parameter);
        }

        private static bool ApplyParameter(string parameter)
        {
            if (!Parameters.TryGetValue(parameter, out Action? applyFunc) || applyFunc == null)
                return false;

            applyFunc.Invoke();
            return true;
        }

        private static bool RunBenchmark(string benchmark, out Summary? summary)
        {
            if (!Benchmarks.TryGetValue(benchmark, out Func<Summary>? runFunc) || runFunc == null)
            {
                summary = null;
                return false;
            }

            summary = runFunc.Invoke();
            return true;
        }

        [Obsolete]
        private static void SetNoLoggerConfig()
        {
            var defaultConfig = DefaultConfig.Instance;

            var config = new ManualConfig()
            {
                UnionRule = ConfigUnionRule.AlwaysUseGlobal
            };

            config.Add(defaultConfig.GetColumnProviders().ToArray());
            config.Add(defaultConfig.GetExporters().ToArray());
            config.Add(defaultConfig.GetDiagnosers().ToArray());
            config.Add(defaultConfig.GetAnalysers().ToArray());
            config.Add(defaultConfig.GetJobs().ToArray());
            config.Add(defaultConfig.GetValidators().ToArray());

            BenchmarkRunnerConfig = config;
        }
    }
}
