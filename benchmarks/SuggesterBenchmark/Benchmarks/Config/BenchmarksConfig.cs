using System.IO;

namespace SuggesterBenchmark.Benchmarks.Config
{
    public static class BenchmarksConfig
    {
        public const string CONFIG_FOLDER = "BenchmarksConfig";

        public static string? ProjectFilePath { get; private set; }
        public static string? UserDictsFilePath { get; private set; }

        public static string WorkingDirectory { get; set; } = Directory.GetCurrentDirectory();

        private static string ConfigProjectFile => Path.Combine(WorkingDirectory, CONFIG_FOLDER, nameof(ProjectFilePath));
        private static string ConfigUserDictsFile => Path.Combine(WorkingDirectory, CONFIG_FOLDER, nameof(UserDictsFilePath));

        public static void SetPaths(string projectFilePath, string userDictsFileName, bool writeToDisk = false)
        {
            DirectoryInfo? ccaDir = Directory.GetParent(projectFilePath)?.Parent;

            string ccaDirPath = ccaDir?.FullName ?? string.Empty;
            string userDictFilePath = Path.Combine(ccaDirPath, userDictsFileName);

            ProjectFilePath = projectFilePath;
            UserDictsFilePath = userDictFilePath;

            if (writeToDisk)
                WritePathsToDisk(ProjectFilePath, UserDictsFilePath);
        }

        public static void WritePathsToDisk(string projectFilePath, string userDictsFilePath)
        {
            if (!Directory.Exists(CONFIG_FOLDER))
                Directory.CreateDirectory(CONFIG_FOLDER);

            File.WriteAllText(ConfigProjectFile, projectFilePath);
            File.WriteAllText(ConfigUserDictsFile, userDictsFilePath);
        }

        public static void ReadPathsFromDisk(string? workingDirectory = null)
        {
            if (workingDirectory != null)
                WorkingDirectory = workingDirectory;

            ProjectFilePath = File.ReadAllText(ConfigProjectFile);
            UserDictsFilePath = File.ReadAllText(ConfigUserDictsFile);
        }

        public static void CheckPaths()
        {
            if (!File.Exists(ProjectFilePath))
                throw new FileNotFoundException(nameof(ProjectFilePath));

            if (!File.Exists(UserDictsFilePath))
                throw new FileNotFoundException(nameof(UserDictsFilePath));
        }
    }
}
