using BenchmarkDotNet.Attributes;
using IndexEngine.Data.Paths;
using IndexEngine.Indexes;
using IndexEngine.Search;
using System;
using System.IO;

namespace SuggesterBenchmark.Benchmarks.Base
{
    [MinColumn, MaxColumn, MedianColumn, RankColumn]
    public class BenchmarkBase
    {
        protected string ProjectFilePath { get; set; }
        protected string UserDictFilePath { get; set; }

        public BenchmarkBase()
        {
            SetPaths();
            CheckPaths();

            ProjectInfo.LoadProject(ProjectFilePath);
            LuceneService.OpenIndex();

            UserDictsIndex.GetInstance().ImportIndex(UserDictFilePath);
        }

        protected void CheckPaths()
        {
            if (!File.Exists(ProjectFilePath))
                throw new FileNotFoundException(nameof(ProjectFilePath));

            if (!File.Exists(UserDictFilePath))
                throw new FileNotFoundException(nameof(UserDictFilePath));
        }

        private void SetPaths()
        {
            string userDictName = "user_dicts.txt";
            string projectFileLocation = @"table-100k\table-100k.cca";

            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            string ccaFolder = Path.Combine(userFolder, "CCA");
            string projectFilePath = Path.Combine(ccaFolder, projectFileLocation);
            string userDictFilePath = Path.Combine(ccaFolder, userDictName);

            ProjectFilePath = projectFilePath;
            UserDictFilePath = userDictFilePath;
        }
    }
}
