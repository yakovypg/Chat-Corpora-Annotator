using SuggesterBenchmark.Benchmarks.Config;

namespace SuggesterBenchmark.CLI
{
    internal static class InputParser
    {
        public static string[] ParseProjectPaths(string? line)
        {
            return SplitLine(line, " ");
        }

        public static string[] ParseParameters(string? line)
        {
            return SplitLine(line, " ")
                .Select(t => t.ToLower())
                .ToArray();
        }

        public static string[] ParseBenchmarks(string? line)
        {
            string[] benchmarks = SplitLine(line, " ");

            if (benchmarks.FirstOrDefault(t => t.ToLower() == "all") != default)
                benchmarks = BenchmarksInfo.Benchmarks;

            return benchmarks;
        }

        private static string[] SplitLine(string? line, params string[] delimiters)
        {
            return string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line) || delimiters == null || delimiters.Length == 0
                ? Array.Empty<string>()
                : line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
