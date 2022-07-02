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
            if (string.IsNullOrEmpty(BenchmarksConfig.ProjectFilePath))
                throw new FileNotFoundException(nameof(BenchmarksConfig.ProjectFilePath));

            string curDir = Directory.GetCurrentDirectory();
            DirectoryInfo? workingDir = Directory.GetParent(curDir)?.Parent?.Parent?.Parent;

            BenchmarksConfig.ReadPathsFromDisk(workingDir?.FullName);
            BenchmarksConfig.CheckPaths();

            ProjectInfo.LoadProject(BenchmarksConfig.ProjectFilePath);
            LuceneService.OpenIndex();

            if (!string.IsNullOrEmpty(BenchmarksConfig.UserDictsFilePath))
                UserDictsIndex.GetInstance().ImportIndex(BenchmarksConfig.UserDictsFilePath);
        }
    }
}
