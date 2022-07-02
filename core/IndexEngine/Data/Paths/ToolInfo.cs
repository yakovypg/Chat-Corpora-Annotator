namespace IndexEngine.Data.Paths
{
    public static class ToolInfo
    {
        public static string RootDirectory { get; }
        public static string ModelsRootDirectory { get; }

        public static string NerPath { get; }
        public static string PosPath { get; }
        public static string SutimeRules { get; }
        public static string SrParserPath { get; }

        public static string UserDictsPath { get; }
        public static string HistogramsPath { get; }
        public static string RecentProjectsPath { get; }
        public static string TagsetColorIndexPath { get; }
        public static string ExtractorConfigPath { get; }
        public static string ExtractorComponentSitesPath { get; }

        static ToolInfo()
        {
            string userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            RootDirectory = Path.Combine(userPath, "CCA");
            ModelsRootDirectory = Path.Combine(RootDirectory, "Models");

            PosPath = Path.Combine(ModelsRootDirectory, "POS", "gate-EN-twitter.model");
            NerPath = Path.Combine(ModelsRootDirectory, "NER", "english.muc.7class.caseless.distsim.crf.ser.gz");
            SrParserPath = Path.Combine(ModelsRootDirectory, "Parser", "englishSR.ser.gz");

            string sutimePath = Path.Combine(ModelsRootDirectory, "Sutime");

            SutimeRules = string.Join(',',
                Path.Combine(sutimePath, "defs.sutime.txt"),
                Path.Combine(sutimePath, "english.holidays.sutime.txt"),
                Path.Combine(sutimePath, "english.sutime.txt")
            );

            UserDictsPath = Path.Combine(RootDirectory, "user_dicts.txt");
            HistogramsPath = Path.Combine(RootDirectory, "histograms.json");
            RecentProjectsPath = Path.Combine(RootDirectory, "recent_projects.txt");
            TagsetColorIndexPath = Path.Combine(RootDirectory, "tagset_colors.txt");
            ExtractorConfigPath = Path.Combine(RootDirectory, "extractor_config.txt");
            ExtractorComponentSitesPath = Path.Combine(RootDirectory, "extractor_component_sites.txt");
        }

        public static bool TryReadRecentProjectsFile(out string[] data)
        {
            try
            {
                if (!File.Exists(RecentProjectsPath))
                    File.Create(RecentProjectsPath).Close();

                data = File.ReadAllLines(RecentProjectsPath);
                return true;
            }
            catch
            {
                data = Array.Empty<string>();
                return false;
            }
        }

        public static bool TryWriteRecentProjectsFile(string[] data)
        {
            try
            {
                File.WriteAllLines(RecentProjectsPath, data ?? Array.Empty<string>());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool TryReadExtractorComponentSites(out string[] sites)
        {
            try
            {
                sites = File.ReadAllLines(ExtractorComponentSitesPath);
                return true;
            }
            catch
            {
                sites = Array.Empty<string>();
                return false;
            }
        }
    }
}
