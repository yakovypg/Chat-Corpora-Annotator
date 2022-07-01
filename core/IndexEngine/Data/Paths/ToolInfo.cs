namespace IndexEngine.Data.Paths
{
    public static class ToolInfo
    {
        public static string SRparserpath { get; private set; }
        public static string NERpath { get; private set; }
        public static string POSpath { get; private set; }

        public static string sutimeRules { get; private set; }
        public static string root { get; private set; }

        public static string ExtractorComponentSitesPath { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\CCA\extractorcomponentsites.txt";
        public static string ExtractorConfigPath { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\CCA\extractorconfig.txt";

        public static string HistogramsPath { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\CCA\histograms.json";

        public static string RecentProjectsPath { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\CCA\recentprojects.txt";
        public static string TagsetColorIndexPath { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\CCA\tagsetscolors.txt";
        public static string UserDictsPath { get; private set; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\CCA\user_dicts.txt";

        public static void SetModelPaths()
        {
            root = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\CCA" + "\\Models";
            POSpath = root + "\\POS" + @"\gate-EN-twitter.model";
            NERpath = root + "\\NER" + @"\english.muc.7class.caseless.distsim.crf.ser.gz";
            SRparserpath = root + "\\Parser" + @"\englishSR.ser.gz";
            sutimeRules = root + @"\sutime\defs.sutime.txt,"
                              + root + @"\sutime\english.holidays.sutime.txt,"
                              + root + @"\sutime\english.sutime.txt";

            Console.WriteLine(UserDictsPath.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0);
        }

        static ToolInfo()
        {
            SetModelPaths();
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
                File.WriteAllLines(RecentProjectsPath, data ?? new string[0]);
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
