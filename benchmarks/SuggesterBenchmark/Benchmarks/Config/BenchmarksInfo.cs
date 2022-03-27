using System.Reflection;

namespace SuggesterBenchmark.Benchmarks.Config
{
    public static class BenchmarksInfo
    {
        public static string[] Benchmarks
        {
            get
            {
                Type[] allTupes = Assembly.GetExecutingAssembly().GetTypes();

                var benchmarkNames = allTupes
                    .Where(t => t.Namespace == "SuggesterBenchmark.Benchmarks")
                    .Select(t => t.Name);

                return benchmarkNames.ToArray();
            }
        }
    }
}
