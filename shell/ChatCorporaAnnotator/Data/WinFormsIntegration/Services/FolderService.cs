using System;
using System.IO;

namespace ChatCorporaAnnotator.Data.WinFormsIntegration.Services
{
    public class FolderService
    {
        private readonly string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public void CheckFolder()
        {
            if (!Directory.Exists(folderPath + "\\CCA"))
                Directory.CreateDirectory(folderPath + "\\CCA");
        }
    }
}
