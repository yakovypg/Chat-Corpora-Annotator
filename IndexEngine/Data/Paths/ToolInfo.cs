using System;

namespace IndexEngine.Data.Paths
{
    public static class ToolInfo
    {
        public static string SRparserpath { get; private set; }
        public static string NERpath { get; private set; }
        public static string POSpath { get; private set; }

        public static string sutimeRules { get; private set; }
        public static string root { get; private set; }
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
    }
}
