using CoreNLPEngine.Extraction;
using Microsoft.Win32;
using System.Runtime.Versioning;

namespace CoreNLPEngine.Diagnostics
{
    public static class ExtractComponentsVerifier
    {
        public static bool IsCoreNLPInstalled(string coreNLPPath)
        {
            if (!Directory.Exists(coreNLPPath))
                return false;

            string[] files = Directory.GetFiles(coreNLPPath);

            return files.Length != 0 && files.Any(t => t.Contains("stanford"));
        }

        [SupportedOSPlatform("windows")]
        public static bool IsJavaInstalled()
        {
            return IsJavaInstalled(out _);
        }

        [SupportedOSPlatform("windows")]
        public static bool IsJavaInstalled(out string? version)
        {
            try
            {
                RegistryKey localMachineRK = Registry.LocalMachine;
                RegistryKey? subKey = localMachineRK.OpenSubKey("SOFTWARE\\JavaSoft\\Java Runtime Environment");

                version = subKey?.GetValue("CurrentVersion")?.ToString();
                return !string.IsNullOrEmpty(version);
            }
            catch
            {
                version = null;
                return false;
            }
        }
    }
}
