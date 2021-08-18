using ChatCorporaAnnotator.Infrastructure.Exceptions;
using System;
using System.IO;

namespace ChatCorporaAnnotator.Data.App
{
    internal static class AppDirectories
    {
        public static bool TryCreateProjectFolder(string projectName, out string projectFolderPath)
        {
            return TryCreateProjectFolder(projectName, out projectFolderPath, out ProjectFolderNotCreatedException _);
        }

        public static bool TryCreateProjectFolder(string projectName, out string projectFolderPath, out ProjectFolderNotCreatedException exception)
        {
            projectFolderPath = null;

            try
            {
                string userProfileFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                string ccaFolderPath = Path.Combine(userProfileFolderPath, "CCA");
                projectFolderPath = Path.Combine(ccaFolderPath, projectName);

                if (!Directory.Exists(ccaFolderPath))
                    Directory.CreateDirectory(ccaFolderPath);

                Directory.CreateDirectory(projectFolderPath);

                exception = null;
                return true;
            }
            catch (Exception ex)
            {
                exception = new ProjectFolderNotCreatedException(ex.Message, projectFolderPath, ex.InnerException);
                return false;
            }
        }
    }
}
