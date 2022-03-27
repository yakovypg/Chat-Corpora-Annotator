using BenchmarkDotNet.Attributes;
using IndexEngine.Data.Paths;
using IndexEngine.Indexes;
using IndexEngine.Search;
using SuggesterBenchmark.Benchmarks.Config;
using System.IO;

namespace SuggesterBenchmark.Benchmarks.Base
{
    [MinColumn, MaxColumn, MedianColumn, RankColumn]
    public class BenchmarkBase
    {
        public BenchmarkBase()
        {
            string curDir = Directory.GetCurrentDirectory();
            DirectoryInfo? workingDir = Directory.GetParent(curDir)?.Parent?.Parent?.Parent;

            BenchmarksConfig.ReadPathsFromDisk(workingDir?.FullName);
            BenchmarksConfig.CheckPaths();

            ProjectInfo.LoadProject(BenchmarksConfig.ProjectFilePath);
            LuceneService.OpenIndex();

            UserDictsIndex.GetInstance().ImportIndex(BenchmarksConfig.UserDictsFilePath);
        }
    }
}
